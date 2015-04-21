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


using ASC.Collections;
using ASC.Web.Core.Client.HttpHandlers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace ASC.Web.Core.Client.PageExtensions
{
    public class ClientScriptBundle
    {
        private static readonly IDictionary<string, ClientScriptHandler> Hash =
            new SynchronizedDictionary<string, ClientScriptHandler>();


        public static ClientScriptHandler GetHttpHandler(string path)
        {
            ClientScriptHandler handler;
            if (Hash.TryGetValue(path, out handler))
            {
                return handler;
            }
            throw new NotSupportedException();
        }

        static ClientScriptBundle()
        {
            RouteTable.Routes.Add(new Route("clientscriptbundle/{version:\\d+}/{path}" + ClientSettings.ClientScriptExtension, new HttpBundleRouteHandler(Hash)));
        }

        public static string ResolveHandlerPath(ICollection<Type> types)
        {
            var tenant = ASC.Core.CoreContext.TenantManager.GetCurrentTenant();
            var version = ""; 
            var resultPath = "";
            var listHandlers  = new List<ClientScript>();

            foreach (var type in types)
            {
                if (!typeof(ClientScript).IsAssignableFrom(type))
                {
                    throw new ArgumentException(string.Format("{0} is not assignable to ClientScriptHandler", type));
                }
                var instance = (ClientScript)Activator.CreateInstance(type);
                version += instance.GetCacheHash();
                resultPath = resultPath + type.FullName.ToLowerInvariant().Replace('.', '_');

                listHandlers.Add(instance);
            }

            resultPath = HttpServerUtility.UrlTokenEncode(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(resultPath)));

            if (!Hash.ContainsKey(resultPath))
            {
                Hash[resultPath] = new ClientScriptHandler {ClientScriptHandlers = listHandlers};
            }

            if (tenant != null && types.All(r => r.BaseType != typeof(ClientScriptLocalization)))
                version = String.Join("_",
                                      new[]
                                              {
                                                  tenant.TenantId.ToString(CultureInfo.InvariantCulture),
                                                  tenant.Version.ToString(CultureInfo.InvariantCulture),
                                                  tenant.LastModified.Ticks.ToString(CultureInfo.InvariantCulture),
                                                  version
                                              });

            var versionHash = HttpServerUtility.UrlTokenEncode(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(version)));

            return new Uri(VirtualPathUtility.ToAbsolute("~/clientscriptbundle/" + versionHash + "/" + resultPath + ClientSettings.ClientScriptExtension),
                    UriKind.Relative).ToString();
        }

        #region Nested type: HttpRouteHandler

        public class HttpBundleRouteHandler : IRouteHandler
        {
            private readonly IDictionary<string, ClientScriptHandler> hash;

            public HttpBundleRouteHandler(IDictionary<string, ClientScriptHandler> hash)
            {
                this.hash = hash;
            }

            #region IRouteHandler Members

            public IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                //get name
                var path = (string) requestContext.RouteData.Values["path"];
                ClientScriptHandler handler;
                if (hash.TryGetValue(path, out handler))
                {
                    return handler;
                }
                throw new NotSupportedException();
            }

            #endregion
        }

        #endregion
    }
}