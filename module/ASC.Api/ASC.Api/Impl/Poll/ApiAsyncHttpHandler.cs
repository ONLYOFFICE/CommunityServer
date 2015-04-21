/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;
using ASC.Api.Enums;
using ASC.Api.Interfaces;
using ASC.Api.Logging;
using ASC.Api.Publisher;
using ASC.Common.Web;
using Microsoft.Practices.Unity;
using System.Net;

namespace ASC.Api.Impl.Poll
{

    class ApiAsyncHttpHandler :  ApiHttpHandlerBase,IApiAsyncHttpHandler
    {
        public ApiAsyncHttpHandler(RouteData routeData) : base(routeData)
        {
        }

        [Dependency]
        public IApiPubSub PubSub { get; set; }


        private string _key = "";


        private void Respond(HttpContextBase context, object responceData)
        {
            var responce = responceData as ApiMethodCallData;
            if (responce != null)
            {
                PostProcessResponse(context, responce);
                try
                {
                    RespondTo(responce.Method, context);
                }
                catch (HttpException)
                {
                    //Do nothing
                }
                catch (Exception exception)
                {
                    SetError(context, exception, HttpStatusCode.InternalServerError);
                    //Try respond again
                    RespondTo(null, context); //if failed - don't care
                }
            }
        }

        private void OnDataRecieved(object data, object userdata)
        {
            Log.Debug("got data for key: {0}", _key);
            var reqState = (AsyncWaitRequestState) userdata;
            PubSub.UnsubscribeForKey(_key, OnDataRecieved, reqState);

            reqState.ExtraData = data;
            reqState.OnCompleted();
        }


        protected override void DoProcess(HttpContextBase context)
        {
            throw new InvalidOperationException("Only async request supported");
        }


        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            var url = ((Route)RouteContext.RouteData.Route).Url;
            //Trim extension
            int extensionDot;
            if ((extensionDot = url.LastIndexOf('.', url.Length - 5)) != -1)
            {
                url = url.Substring(0, extensionDot);
            }

            _key = url + ":" +
                      PubSubKeyHelper.GetKeyForRoute(RouteContext.RouteData.Route.GetRouteData(RouteContext.HttpContext));

            var reqState = new AsyncWaitRequestState(context, cb, extraData);

            Log.Debug("subscribing to key: {0}", _key);
            PubSub.SubscribeForKey(_key, OnDataRecieved, reqState);

            //TODO: Think how to move it
            //Dispose all pending context shit
            new DisposableHttpContext(context).Dispose();
            return reqState;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            var reqState = (AsyncWaitRequestState)result;
            
            //Neeeded to rollback errors
            reqState.Context.Response.Buffer = true;
            reqState.Context.Response.BufferOutput = true;

            //Set cache
            reqState.Context.Response.Cache.SetETag(Guid.NewGuid().ToString("N"));
            reqState.Context.Response.Cache.SetMaxAge(TimeSpan.FromSeconds(0));
            ApiResponce.Status = ApiStatus.Ok;

            Respond(new HttpContextWrapper(reqState.Context), reqState.ExtraData);

        }
    }
}