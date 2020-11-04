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
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Routing;
using ASC.Api.Enums;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Web.Core.Client;
using Autofac;

namespace ASC.Api.Impl
{
    public abstract class ApiHttpHandlerBase : IApiHttpHandler
    {
        public ILog Log { get; set; }

        public IApiManager ApiManager { get; set; }

        public ILifetimeScope Container { get; set; }

        public RouteData RouteData { get; private set; }

        protected IApiMethodCall Method { get; private set; }


        public IApiStandartResponce ApiResponce { get; private set; }

        public ApiContext ApiContext { get; private set; }




        protected ApiHttpHandlerBase(RouteData routeData)
        {
            RouteData = routeData;
        }

        protected void RespondTo(IApiMethodCall method, HttpContextBase context)
        {
            try
            {
                context.Response.Cache.SetETag(Guid.NewGuid().ToString("N"));
                context.Response.Cache.SetMaxAge(TimeSpan.FromSeconds(0));
            }
            catch (Exception)
            {
                //Nothing happens if we didn't set cache tags   
            }

            IApiResponder responder = null;

            if (method != null && method.Responders != null && method.Responders.Any())
            {
                try
                {
                    //Try custom responders
                    var methodCustomResponders = new List<IApiResponder>(method.Responders);
                    responder = methodCustomResponders.FirstOrDefault(x => x != null && x.CanRespondTo(ApiResponce, context));
                }
                catch (Exception)
                {
                    Log.WarnFormat("Custom reponder for {0} failed", method.ToString());
                }
            }
            if (responder == null)
            {
                responder = Container.Resolve<IEnumerable<IApiResponder>>().FirstOrDefault(x => x.CanRespondTo(ApiResponce, context));
            }

            if (responder != null)
            {
                try
                {
                    responder.RespondTo(ApiResponce, context);
                }
                catch (Exception e)
                {
                    Log.Error("Error while responding!", e);
                    throw;
                }
            }
            else
            {
                Log.Error("no formatter error");
                throw new HttpException((int)HttpStatusCode.BadRequest, "No formatter");
            }
        }

        public void Process(HttpContextBase context)
        {
            using (Container)
            {
                ApiManager = Container.Resolve<IApiManager>();
                RouteContext = new RequestContext(context, RouteData);
                var config = Container.Resolve<IApiConfiguration>();
                ApiContext = Container.Resolve<ApiContext>(new NamedParameter("requestContext", RouteContext), new NamedParameter("apiConfiguration", config));
                ApiResponce = Container.Resolve<IApiStandartResponce>();

                //NOTE: Don't register anything it will be resolved when needed
                //Container.RegisterInstance(ApiContext, new HttpContextLifetimeManager2(context));//Regiter only api context

                Method = ApiManager.GetMethod(((Route) RouteData.Route).Url, context.Request.RequestType); //Set method

                DoProcess(context);
            }
        }

        protected RequestContext RouteContext { get; private set; }

        protected abstract void DoProcess(HttpContextBase context);

        public void ProcessRequest(HttpContext context)
        {
            var contextWrapper = new HttpContextWrapper(context);
            ProcessInternal(contextWrapper);
        }

        private void ProcessInternal(HttpContextWrapper contextWrapper)
        {
            try
            {
                Process(contextWrapper);
            }
            catch (ThreadAbortException e)
            {
                Log.Error("thread aborted", e);
            }
            catch (Exception e)
            {
                var method = contextWrapper.Request != null ? contextWrapper.Request.HttpMethod : null;
                var url = contextWrapper.Request != null ? contextWrapper.Request.Url : null;
                var user = contextWrapper.User != null ? contextWrapper.User.Identity.Name : null;
                Log.Error(string.Format("error during processing http request {0}: {1}, user: {2}", method, url, user), e);
                throw;
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }


        protected void PostProcessResponse(HttpContextBase context, object responce)
        {
            ApiResponce.Status = ApiStatus.Ok;
            //Set 'created' when posted
            if (context.Request.RequestType.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Created;
                context.Response.StatusDescription = HttpStatusCode.Created.ToString();
            }
            if (responce != null)
            {
                //Correct item count
                var beforeFilterCount = Binder.GetCollectionCount(responce);

                var filters = Container.Resolve<IEnumerable<IApiResponceFilter>>();
                //Do filtering
                if (filters != null)
                    responce = filters.Aggregate(responce, (current, apiResponceFilter) => apiResponceFilter.FilterResponce(current,ApiContext));

                ApiResponce.Count = Binder.GetCollectionCount(responce);
                if (ApiResponce.Count == ApiContext.SpecifiedCount && (beforeFilterCount - ApiContext.StartIndex) > ApiContext.SpecifiedCount)
                {
                    //This means we have more
                    ApiResponce.NextPage = ApiContext.SpecifiedStartIndex + ApiContext.SpecifiedCount + 1;
                    //Cut collection by 1
                }
                ApiResponce.TotalCount = ApiContext.TotalCount;
                ApiResponce.Response = responce;
            }
            else
            {
                ApiResponce.Count = 0;
                ApiResponce.Response = new object();
            }
            ApiResponce.Code = context.Response.StatusCode;
            if (context.Handler.GetType() == GetType())
            {
                var acceptEncoding = context.Request.Headers["Accept-Encoding"];

                if (!string.IsNullOrEmpty(acceptEncoding))
                {
                    var encodings = acceptEncoding.Split(',');
                    if(encodings.Contains("gzip") && ClientSettings.GZipEnabled)
                    {
                        context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                        context.Response.AppendHeader("Content-Encoding", "gzip");
                    }
                }

                SecurityContext.Logout();
            }
        }

        protected void SetError(HttpContextBase context, Exception e, HttpStatusCode code)
        {
            SetError(context, e, code, "Server error");
        }

        protected void SetError(HttpContextBase context, Exception e, HttpStatusCode code, string description)
        {
            Log.Error(string.Format("method error: {0} - {1}", context.Request.Url, description), e);
            if (context.Response.StatusCode / 100 == 2)
            {
                context.Response.StatusCode = (int)code;
            }
            context.Response.StatusDescription = description;
            ApiResponce.Response = null;
            ApiResponce.Status = ApiStatus.Error;
            ApiResponce.Code = context.Response.StatusCode;
            ApiResponce.Count = 0;
            ApiResponce.Error = new ErrorWrapper(e);
            context.Response.TrySkipIisCustomErrors = true;//try always
        }
    }
}