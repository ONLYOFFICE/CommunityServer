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
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.SessionState;

namespace ASC.Common.Web
{
    public abstract class AbstractHttpAsyncHandler : IHttpAsyncHandler, IReadOnlySessionState
    {
        private Action<HttpContext> processRequest;
        private IPrincipal principal;
        private CultureInfo culture;


        public bool IsReusable
        {
            get { return false; }
        }


        public void ProcessRequest(HttpContext context)
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentPrincipal = principal;
            HttpContext.Current = context;
            OnProcessRequest(context);
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            culture = Thread.CurrentThread.CurrentCulture;
            principal = Thread.CurrentPrincipal;
            processRequest = ProcessRequest;
            return processRequest.BeginInvoke(context, cb, extraData);
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            processRequest.EndInvoke(result);
        }


        public abstract void OnProcessRequest(HttpContext context);
    }
}
