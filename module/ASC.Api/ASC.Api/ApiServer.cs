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
using System.Web;
using System.Web.Routing;
using ASC.Api.Batch;
using ASC.Api.Impl;
using ASC.Api.Utils;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System.Text;

namespace ASC.Api
{
    public class ApiServer
    {
        private readonly HttpContextBase _context;
        private readonly ApiBatchHttpHandler _batchHandler;

        public ApiServer()
            : this(HttpContext.Current)
        {
        }

        public ApiServer(HttpContext context)
            : this(new HttpContextWrapper(context))
        { }

        public ApiServer(HttpContextBase context)
        {
            _context = context;
            var container = ServiceLocator.Current.GetInstance<IUnityContainer>();
            var routeHandler = container.Resolve<ApiBatchRouteHandler>();
            var requestContext = new RequestContext(context, new RouteData(new Route("batch", routeHandler), routeHandler));
            _batchHandler = routeHandler.GetHandler(container, requestContext) as ApiBatchHttpHandler;
            if (_batchHandler==null)
                throw new ArgumentException("Couldn't resolve api");
        }

        public string GetApiResponse(string apiUrl)
        {
            return GetApiResponse(apiUrl, null);
        }

        public string GetApiResponse(string apiUrl, string httpMethod)
        {
            return GetApiResponse(apiUrl, httpMethod, null);
        }

        public string GetApiResponse(string apiUrl, string httpMethod, string body)
        {
            return GetApiResponse(new ApiBatchRequest() { Method = httpMethod, RelativeUrl = apiUrl, Body = body });
        }

        public string GetApiResponse(ApiBatchRequest request)
        {
            var response = CallApiMethod(request);
            return response!=null ? response.Data : null;
        }

        public ApiBatchResponse CallApiMethod(string apiUrl)
        {
            return CallApiMethod(apiUrl, "GET");
        }

        public ApiBatchResponse CallApiMethod(string apiUrl, string httpMethod)
        {
            return CallApiMethod(apiUrl, httpMethod, null);
        }

        public ApiBatchResponse CallApiMethod(string apiUrl, string httpMethod, string body)
        {
            return CallApiMethod(new ApiBatchRequest(){Method = httpMethod,RelativeUrl = apiUrl, Body = body});
        }

        public ApiBatchResponse CallApiMethod(ApiBatchRequest request)
        {
            return CallApiMethod(request, true);
        }

        public ApiBatchResponse CallApiMethod(ApiBatchRequest request, bool encode)
        {
            var response = _batchHandler.ProcessBatchRequest(_context, request);
            if (encode && response!=null && response.Data!=null)
                response.Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(response.Data));
            return response;
        }


        public IEnumerable<ApiBatchResponse> CallApiMethods(IEnumerable<ApiBatchRequest> requests)
        {
            return requests.Select(request =>CallApiMethod(request));
        }
    }
}