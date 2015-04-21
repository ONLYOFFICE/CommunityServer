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