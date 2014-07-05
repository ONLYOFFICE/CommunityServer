/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Routing;
using ASC.Api.Enums;
using ASC.Api.Impl.Responders;
using ASC.Api.Interfaces;
using ASC.Api.Logging;
using ASC.Api.Utils;
using Microsoft.Practices.Unity;

namespace ASC.Api.Impl
{
    public abstract class ApiHttpHandlerBase : IApiHttpHandler
    {


        [Dependency]
        public ILog Log { get; set; }

        public IApiManager ApiManager { get; set; }

        [Dependency]
        public IUnityContainer Container { get; set; }

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
                    Log.Warn("Custom reponder for {0} failed", method.ToString());
                }
            }
            if (responder == null)
            {
                responder = Container.ResolveAll<IApiResponder>().FirstOrDefault(x => x.CanRespondTo(ApiResponce, context));
            }

            if (responder != null)
            {
                try
                {
                    responder.RespondTo(ApiResponce, context);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error while responding!");
                    throw;
                }
            }
            else
            {
                Log.Error(null, "no formatter error");
                throw new HttpException((int)HttpStatusCode.BadRequest, "No formatter");
            }
        }

        public void Process(HttpContextBase context)
        {
            //Resolve all here
            ApiResponce = Container.Resolve<IApiStandartResponce>();
            ApiManager = Container.Resolve<IApiManager>();
            RouteContext = new RequestContext(context,RouteData);
            ApiContext = new ApiContext(RouteContext);
            ApiResponce.ApiContext = ApiContext;

            //NOTE: Don't register anything it will be resolved when needed
            //Container.RegisterInstance(ApiContext, new HttpContextLifetimeManager2(context));//Regiter only api context

            Method = ApiManager.GetMethod(((Route)RouteData.Route).Url, context.Request.RequestType);//Set method

            DoProcess(context);
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
                Log.Error(e, "thread aborted");
            }
            catch (Exception e)
            {
                Log.Error(e, "error during processing http request");
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

                var filters = Container.ResolveAll<IApiResponceFilter>();
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
        }

        protected void SetError(HttpContextBase context, Exception e, HttpStatusCode code)
        {
            SetError(context, e, code, "Server error");
        }

        protected void SetError(HttpContextBase context, Exception e, HttpStatusCode code, string description)
        {
            Log.Error(e, "method error:{0}", description);
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