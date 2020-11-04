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

namespace ASC.Api.Client
{
    public class ApiErrorException : Exception
    {
        public string ErrorStackTrace { get; internal set; }

        public string ErrorType { get; internal set; }

        public string ErrorMessage { get; internal set; }

        public ApiErrorException()
            : base("TeamLab api returned an error. Use ErrorMessage, ErrorType or ErrorStackTrace to get error specifics.")
        {
        }
    }

    public class HttpErrorException : Exception
    {
        public int StatusCode { get; private set; }

        public string StatusDescription { get; private set; }

        public HttpErrorException(int statusCode, string statusDescription)
            : base(string.Format("{0} ({1})", statusCode, statusDescription))
        {
            StatusCode = statusCode;
            StatusDescription = statusDescription;
        }
    }

    public class UnsupportedMediaTypeException : Exception
    {
        public string MediaType { get; private set; }

        public UnsupportedMediaTypeException(string mediaType)
            : base("Request ended with no errors but response was of unsupported type.")
        {
            MediaType = mediaType;
        }
    }
}
