/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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