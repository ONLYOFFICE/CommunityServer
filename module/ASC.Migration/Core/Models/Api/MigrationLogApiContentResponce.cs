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


using System.IO;
using System.Net.Mime;
using System.Text;

using ASC.Api.Interfaces.ResponseTypes;

namespace ASC.Migration.Core.Models.Api
{
    public class MigrationLogApiContentResponce : IApiContentResponce
    {
        private readonly Stream _stream;
        private readonly string _fileName;


        public MigrationLogApiContentResponce(Stream stream, string fileName)
        {
            _stream = stream;
            _fileName = fileName;
        }

        public Encoding ContentEncoding
        {
            get { return Encoding.UTF8; }
        }

        public Stream ContentStream
        {
            get { return _stream; }
        }

        public ContentType ContentType
        {
            get { return new ContentType("text/plain; charset=UTF-8"); }
        }

        public ContentDisposition ContentDisposition
        {
            get { return new ContentDisposition { Inline = false, FileName = _fileName }; }
        }
    }
}
