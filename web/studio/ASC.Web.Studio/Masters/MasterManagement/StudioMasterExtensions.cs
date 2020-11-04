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


using System.Web.UI;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Studio.Masters;

namespace System.Web
{
    public static class StudioMasterExtensions
    {
        #region Style

        public static Page RegisterStyle(this Page page, params string[] scriptPath)
        {
            return page.RegisterStyle(VirtualPathUtility.ToAbsolute, scriptPath);
        }

        public static Page RegisterStyle(this Page page, Func<string, string> converter, params string[] absoluteLessPath)
        {
            if (page == null) throw new ArgumentNullException("page");
            var master = GetStudioMaster(page);
            if (master != null)
            {
                master.AddStyles(converter, absoluteLessPath);
            }
            return page;
        }


        #endregion

        #region Script

        public static Page RegisterBodyScripts(this Page page, params string[] scriptPath)
        {
            page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute, scriptPath);
            return page;
        }

        public static Page RegisterBodyScripts(this Page page, Func<string, string> converter, params string[] scriptPath)
        {
            if (page == null) throw new ArgumentNullException("page");
            var master = GetStudioMaster(page);
            if (master != null)
            {
                master.AddBodyScripts(converter, scriptPath);
            }
            return page;
        }

        public static Page RegisterStaticScripts(this Page page, ScriptBundleData bundleData)
        {
            if (page == null) throw new ArgumentNullException("page");
            var master = GetStudioMaster(page);
            if (master != null)
            {
                master.AddStaticBodyScripts(bundleData);
            }
            return page;
        }

        public static Page RegisterInlineScript(this Page page, string script, bool beforeBodyScript = false, bool onReady = true)
        {
            if (page == null) throw new ArgumentNullException("page");
            var master = GetStudioMaster(page);
            if (master != null)
            {
                master.RegisterInlineScript(script, beforeBodyScript, onReady);
            }
            return page;
        }

        #endregion

        #region ClientScript

        public static Page RegisterClientScript(this Page page, ClientScript clientScript)
        {
            if (page == null) throw new ArgumentNullException("page");
            var master = GetStudioMaster(page);
            if (master != null)
            {
                master.AddClientScript(clientScript);
            }
            return page;
        }

        #endregion

        private static BaseTemplate GetStudioMaster(this Page page)
        {
            if (page == null) throw new ArgumentNullException("page");
            var master = page.Master;
            while (master != null && !(master is BaseTemplate))
            {
                master = master.Master;
            }
            return master as BaseTemplate;
        }
    }
}