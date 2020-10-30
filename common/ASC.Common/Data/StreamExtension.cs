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
using ASC.Data.Storage;

public static class StreamExtension
{
    private const int BufferSize = 2048; //NOTE: set to 2048 to fit in minimum tcp window

    public static Stream GetBuffered(this Stream srcStream)
    {
        if (srcStream == null) throw new ArgumentNullException("srcStream");
        if (!srcStream.CanSeek || srcStream.CanTimeout)
        {
            //Buffer it
            var memStream = TempStream.Create();
            srcStream.StreamCopyTo(memStream);
            memStream.Position = 0;
            return memStream;
        }
        return srcStream;
    }

    public static byte[] GetCorrectBuffer(this Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException("stream");
        }

        using (var mem = stream.GetBuffered())
        {
            var buffer = new byte[mem.Length];
            mem.Position = 0;
            mem.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }

    public static void StreamCopyTo(this Stream srcStream, Stream dstStream)
    {
        if (srcStream == null) throw new ArgumentNullException("srcStream");
        if (dstStream == null) throw new ArgumentNullException("dstStream");

        var buffer = new byte[BufferSize];
        int readed;
        while ((readed = srcStream.Read(buffer, 0, BufferSize)) > 0)
        {
            dstStream.Write(buffer, 0, readed);
        }
    }

    public static void StreamCopyTo(this Stream srcStream, Stream dstStream, int length)
    {
        if (srcStream == null) throw new ArgumentNullException("srcStream");
        if (dstStream == null) throw new ArgumentNullException("dstStream");

        var buffer = new byte[BufferSize];
        int totalRead = 0;
        int readed;
        while ((readed = srcStream.Read(buffer, 0, length - totalRead > BufferSize ? BufferSize : length - totalRead)) > 0 && totalRead < length)
        {
            dstStream.Write(buffer, 0, readed);
            totalRead += readed;
        }
    }
}
