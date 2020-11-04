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


using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ASC.CdnCheck
{
    public class CorsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            bool isCorsRequest = request.Headers.Contains("Origin");
            bool isPreflightRequest = request.Method == HttpMethod.Options;
            if (isCorsRequest)
            {
                if (isPreflightRequest)
                {
                    return Task.Factory.StartNew<HttpResponseMessage>(() =>
                    {
                        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                        response.Headers.Add("AccessControlAllowOrigin", request.Headers.GetValues("Origin").First());

                        string accessControlRequestMethod =
                            request.Headers.GetValues("AccessControlRequestMethod").FirstOrDefault();
                        if (accessControlRequestMethod != null)
                        {
                            response.Headers.Add("AccessControlAllowMethods", accessControlRequestMethod);
                        }

                        string requestedHeaders = string.Join(", ",
                            request.Headers.GetValues("AccessControlRequestHeaders"));
                        if (!string.IsNullOrEmpty(requestedHeaders))
                        {
                            response.Headers.Add("AccessControlAllowHeaders", requestedHeaders);
                        }

                        return response;
                    }, cancellationToken);
                }
                return base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>(t =>
                {
                    HttpResponseMessage resp = t.Result;
                    resp.Headers.Add("AccessControlAllowOrigin", request.Headers.GetValues("Origin").First());
                    return resp;
                });
            }
            else
            {
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}