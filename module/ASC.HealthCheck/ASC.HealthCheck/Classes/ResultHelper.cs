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


using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using ASC.HealthCheck.Resources;

namespace ASC.HealthCheck.Classes
{
    public class ResultHelper
    {
        private readonly MediaTypeFormatter formatter;
        public ResultHelper(MediaTypeFormatter formatter)
        {
            this.formatter = formatter;
        }

        public HttpResponseMessage GetContent(object result)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<object>(result, formatter)
            };
        }

        public HttpResponseMessage Success(string status = "", string message = "")
        {
            return GetContent(new {code = 1, status, message});
        }

        public HttpResponseMessage Error(string status, string message = "")
        {
            return GetContent(new { code = 0, status, message });
        }

        public HttpResponseMessage StatusServerError(string message = "")
        {
            return Error(HealthCheckResource.StatusServiceError, message);
        }

        public HttpResponseMessage WrongParameterError()
        {
            return StatusServerError(HealthCheckResource.WrongParameter);
        }

        public HttpResponseMessage ServerError()
        {
            return StatusServerError(HealthCheckResource.ServerError);
        }

        public HttpResponseMessage WrongServiceNameError()
        {
            return StatusServerError(HealthCheckResource.WrongServiceName);
        }
    }
}
