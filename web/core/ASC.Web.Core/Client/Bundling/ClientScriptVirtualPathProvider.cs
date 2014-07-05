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
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.Remoting;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Routing;

namespace ASC.Web.Core.Client.Bundling
{
    class ClientScriptVirtualPathProvider : VirtualPathProvider, IRouteHandler
    {
        public override bool FileExists(string virtualPath)
        {
            return virtualPath.Contains(BundleHelper.CLIENT_SCRIPT_VPATH) ? true : GetPrevius().FileExists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            return virtualPath.Contains(BundleHelper.CLIENT_SCRIPT_VPATH) ? new ClientScriptFile(virtualPath) : GetPrevius().GetFile(virtualPath);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            return GetPrevius().GetCacheDependency(virtualPath, virtualPathDependencies.Cast<string>().Where(p => !p.Contains(BundleHelper.CLIENT_SCRIPT_VPATH)), utcStart);
        }


        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            return GetPrevius().GetFileHash(virtualPath, virtualPathDependencies);
        }

        public override string CombineVirtualPaths(string basePath, string relativePath)
        {
            return GetPrevius().CombineVirtualPaths(basePath, relativePath);
        }

        public override ObjRef CreateObjRef(Type requestedType)
        {
            return GetPrevius().CreateObjRef(requestedType);
        }

        public override bool DirectoryExists(string virtualDir)
        {
            return GetPrevius().DirectoryExists(virtualDir);
        }

        public override bool Equals(object obj)
        {
            return GetPrevius().Equals(obj);
        }

        public override string GetCacheKey(string virtualPath)
        {
            return GetPrevius().GetCacheKey(virtualPath);
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            return GetPrevius().GetDirectory(virtualDir);
        }

        public override int GetHashCode()
        {
            return GetPrevius().GetHashCode();
        }

        public override object InitializeLifetimeService()
        {
            return GetPrevius().InitializeLifetimeService();
        }

        public override string ToString()
        {
            return GetPrevius().ToString();
        }


        private VirtualPathProvider GetPrevius()
        {
            return Previous == null && HostingEnvironment.VirtualPathProvider != this ? HostingEnvironment.VirtualPathProvider : Previous;
        }


        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new ClientScriptHandler();
        }


        class ClientScriptHandler : IHttpHandler
        {
            public bool IsReusable
            {
                get { return true; }
            }

            public void ProcessRequest(HttpContext context)
            {
                var version = ClientScriptReference.GetContentHash(context.Request.Url.AbsolutePath);
                if (string.Equals(context.Request.Headers["If-None-Match"], version))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                }
                else
                {
                    context.Response.Write(CopyrigthTransform.CopyrigthText);
                    context.Response.Write(ClientScriptReference.GetContent(context.Request.Url.AbsolutePath));
                    context.Response.Charset = Encoding.UTF8.WebName;
                    context.Response.ContentType = new ContentType("application/x-javascript") { CharSet = Encoding.UTF8.WebName }.ToString();

                    // cache
                    context.Response.Cache.SetVaryByCustom("*");
                    context.Response.Cache.SetAllowResponseInBrowserHistory(true);
                    context.Response.Cache.SetETag(version);
                    context.Response.Cache.SetCacheability(HttpCacheability.Public);
                    context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                }
            }
        }


        public class ClientScriptFile : VirtualFile
        {
            public override bool IsDirectory
            {
                get { return false; }
            }


            public ClientScriptFile(string virtualPath)
                : base(virtualPath)
            {
            }

            public override Stream Open()
            {
                var stream = new MemoryStream();
                var buffer = Encoding.UTF8.GetBytes(ClientScriptReference.GetContent(VirtualPath));
                stream.Write(buffer, 0, buffer.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }
    }
}