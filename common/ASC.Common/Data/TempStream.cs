/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

namespace ASC.Common
{
    public static class TempStream
    {
        public static Stream GetBuffered(this Stream srcStream)
        {
            if (srcStream == null) throw new ArgumentNullException("srcStream");
            if (!srcStream.CanSeek || srcStream.CanTimeout)
            {
                //Buffer it
                var memStream = Create();
                srcStream.CopyTo(memStream);
                memStream.Position = 0;
                return memStream;
            }
            return srcStream;
        }

        public static Stream Create()
        {
            return new FileStream(TempPath.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose);
        }
    }
}