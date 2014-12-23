/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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