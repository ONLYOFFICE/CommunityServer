/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.IO;
using System.Threading;
using ASC.Data.Storage;

public static class Extensions
{
    private const int BufferSize = 2048;//NOTE: set to 2048 to fit in minimum tcp window

    public static Stream IronReadStream(this IDataStore store, string domain, string path, int tryCount)
    {
        var ms = TempStream.Create();
        IronReadToStream(store, domain, path, tryCount, ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public static void IronReadToStream(this IDataStore store, string domain, string path, int tryCount, Stream readTo)
    {
        if (tryCount < 1) throw new ArgumentOutOfRangeException("tryCount", "Must be greater or equal 1.");
        if (!readTo.CanWrite) throw new ArgumentException("stream cannot be written", "readTo");

        var tryCurrent = 0;
        var offset = 0;

        while (tryCurrent < tryCount)
        {
            try
            {
                tryCurrent++;
                using (var stream = store.GetReadStream(domain, path, offset))
                {
                    var buffer = new byte[BufferSize];
                    var readed = 0;
                    while ((readed = stream.Read(buffer, 0, BufferSize)) > 0)
                    {
                        readTo.Write(buffer, 0, readed);
                        offset += readed;
                    }
                }
                break;
            }
            catch (Exception ex)
            {
                if (tryCurrent >= tryCount)
                {
                    throw new IOException("Can not read stream. Tries count: " + tryCurrent + ".", ex);
                }
                Thread.Sleep(tryCount * 50);
            }
        }
    }
}