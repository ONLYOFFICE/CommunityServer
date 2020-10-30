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
using System.Net;

namespace ASC.Mail.Exceptions
{
    public class ApiHelperException : Exception
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _response;

        public HttpStatusCode StatusCode {
            get { return _statusCode; }
        }

        public string Response
        {
            get { return _response; }
        }

        public ApiHelperException(string message, HttpStatusCode statusCode, string response)
            : base(message)
        {
            _statusCode = statusCode;
            _response = response;
        }

        public override string ToString()
        {
            return string.Format("Api error has been occurred: Message = '{0}' StatusCode = '{1}' Response = '{2}'",
                Message, StatusCode, Response);
        }
    }
}
