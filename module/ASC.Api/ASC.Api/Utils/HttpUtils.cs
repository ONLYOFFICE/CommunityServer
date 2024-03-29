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
using System.Web;

namespace ASC.Api.Utils
{
    public static class HttpUtils
    {
        private const int BufferReadLength = 2048;

        public static void WriteStreamToResponce(this HttpResponseBase response, Stream stream)
        {
            //set unbuffered output
            response.Buffer = false;
            response.BufferOutput = false;
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            var buffer = new byte[BufferReadLength];
            int readed;
            while ((readed = stream.Read(buffer, 0, BufferReadLength)) > 0)
            {
                var subbufer = new byte[readed];
                Array.Copy(buffer, subbufer, readed);
                response.BinaryWrite(subbufer);
            }
        }
    }
}