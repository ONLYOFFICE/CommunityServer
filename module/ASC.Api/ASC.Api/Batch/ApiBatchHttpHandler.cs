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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Routing;

using ASC.Api.Collections;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ASC.Api.Batch
{
    public class ApiBatchHttpHandler : ApiHttpHandlerBase
    {
        public ApiBatchHttpHandler(RouteData routeData)
            : base(routeData)
        {
        }

        protected override void DoProcess(HttpContextBase context)
        {
            //Read body
            var batch = context.Request["batch"];
            IEnumerable<ApiBatchRequest> requests = null;
            if (!string.IsNullOrEmpty(batch))
            {
                requests = JsonConvert.DeserializeObject<IEnumerable<ApiBatchRequest>>(batch,new JsonSerializerSettings
                            {
                                DefaultValueHandling = DefaultValueHandling.Ignore,
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            });

            }
            else
            {
                if (!"GET".Equals(context.Request.HttpMethod, StringComparison.InvariantCultureIgnoreCase))
                {
                    //Try bind form
                    requests = Utils.Binder.Bind<IEnumerable<ApiBatchRequest>>(context.Request.Form);
                }
            }

            if (requests!=null && requests.Any())
            {
                try
                {
                    Log.Debug("processing batch started");
                    ProcessBatch(context, requests);
                    Log.Debug("processing batch finished");
                }
                catch (Exception e)
                {
                    Log.Error("batch process error", e);
                    //Set status to error
                    SetError(context, e, HttpStatusCode.InternalServerError);
                    //Try respond again
                    RespondTo(null, context);
                }
            }
            else
            {
                ApiResponce.Error = new ErrorWrapper(new InvalidOperationException("no batch specified"));
            }

            RespondTo(null, context);
        }

        private void ProcessBatch(HttpContextBase context, IEnumerable<ApiBatchRequest> requests)
        {
            var resonse = requests.OrderBy(x => x.Order).Select(x => ProcessBatchRequest(context, x)).ToList();

            ApiResponce.Response = resonse;
            PostProcessResponse(context, resonse);
        }

        internal ApiBatchResponse ProcessBatchRequest(HttpContextBase context, ApiBatchRequest apiBatchRequest)
        {
            if (context.Request == null) throw new InvalidOperationException("Request is empty");
            if (context.Request.Url == null) throw new InvalidOperationException("Url is empty");

            using (var writer = new StringWriter())
            {
                var path = apiBatchRequest.RelativeUrl;
                if (context.Request.ApplicationPath != null && path.StartsWith(context.Request.ApplicationPath))
                {
                    path = path.Substring(context.Request.ApplicationPath.Length);
                }
                var uri = new Uri(context.Request.Url,"/"+path.TrimStart('/'));
                
                var workerRequest = new ApiWorkerRequest(Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),uri.Query.TrimStart('?'), writer, context, new ContentType(apiBatchRequest.BodyContentType));
                workerRequest.HttpVerb = apiBatchRequest.Method;

                if (!string.IsNullOrEmpty(apiBatchRequest.Body))
                {
                    var contentType = new ContentType(apiBatchRequest.BodyContentType);
                    var encoding = Encoding.GetEncoding(contentType.CharSet);
                    workerRequest.EntityBody = encoding.GetBytes(apiBatchRequest.Body);
                }
                var workContext = new HttpContext(workerRequest) {Handler = this};
                var newContext = new HttpContextWrapper(workContext);
                
                //Make a faked request
                var routeData = RouteTable.Routes.GetRouteData(newContext);
                if (routeData != null)
                {
                    //Construct new context
                    Container.BeginLifetimeScope().Resolve<IApiHttpHandler>(new TypedParameter(typeof(RouteData), routeData)).Process(newContext);
                    newContext.Response.Flush();
                    
                    //Form response
                    var response = new ApiBatchResponse(apiBatchRequest)
                                       {
                                           Data = writer.GetStringBuilder().ToString(),
                                           Headers = new ItemDictionary<string, string>()
                                       };
                    foreach (var responseHeaderKey in workerRequest.ResponseHeaders.AllKeys)
                    {
                        response.Headers.Add(responseHeaderKey, workerRequest.ResponseHeaders[responseHeaderKey]);
                    }
                    response.Status = workerRequest.HttpStatus;
                    response.Name = apiBatchRequest.Name;
                    return response;
                }
                
            }
            return null;
        }
    }
}