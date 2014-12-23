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

#region usings

using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;
using ASC.Api.Interfaces;
using ASC.Api.Logging;
using ASC.Api.Utils;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;

#endregion

namespace ASC.Api.Impl
{
    public class ApiRouteHandler : IApiRouteHandler
    {

        #region IApiRouteHandler Members


        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var container = ServiceLocator.Current.GetInstance<IUnityContainer>();
            var authorizations = container.ResolveAll<IApiAuthorization>().ToList();
            var log = container.Resolve<ILog>();

            //Authorize request first
            log.Debug("Authorizing {0}",requestContext.HttpContext.Request.Url);
            
            
            if (requestContext.RouteData.DataTokens.ContainsKey(DataTokenConstants.RequiresAuthorization) 
                && !(bool)requestContext.RouteData.DataTokens[DataTokenConstants.RequiresAuthorization])
            {
                //Authorization is not required for method
                log.Debug("Authorization is not required");
                return GetHandler(container, requestContext);
            }
            foreach (var apiAuthorization in authorizations)
            {
                log.Debug("Authorizing with:{0}",apiAuthorization.GetType().ToString());
                if (apiAuthorization.Authorize(requestContext.HttpContext))
                {

                    return GetHandler(container,requestContext);
                }
            }
            if (authorizations.Any(apiAuthorization => apiAuthorization.OnAuthorizationFailed(requestContext.HttpContext)))
            {
                log.Debug("Unauthorized");
                return new ErrorHttpHandler((int)HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized.ToString());
            }
            log.Debug("Forbidden");
            return new ErrorHttpHandler((int)HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized.ToString());
        }

        public virtual IHttpHandler GetHandler(IUnityContainer container,RequestContext requestContext)
        {
            return container.Resolve<IApiHttpHandler>(new DependencyOverride(typeof(RouteData), requestContext.RouteData));
        }

        #endregion
    }

    class ApiAsyncRouteHandler : ApiRouteHandler
    {

        public override IHttpHandler GetHandler(IUnityContainer container, RequestContext requestContext)
        {
            throw new NotImplementedException("This handler is not yet implemented");
            
        }
    }
}