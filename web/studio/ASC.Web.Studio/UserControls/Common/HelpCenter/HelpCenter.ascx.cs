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
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;
using ASC.Web.Core;
using ASC.Web.Studio.Core.HelpCenter;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Common.HelpCenter
{
    public partial class HelpCenter : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/HelpCenter/HelpCenter.ascx"; }
        }

        public bool IsSideBar { get; set; }
        public Guid ModuleId { get; set; }

        protected List<HelpCenterItem> HelpCenterItems { get; set; }
        protected String HelpLink { get; set; }
        protected String HelpLinkBlock { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink(true)))
            {
                return;
            }

            Page.RegisterStyle("~/usercontrols/common/helpcenter/css/help-center.less");

            string module;
            string mainLink;

            if (Page is Studio.Management)
            {
                module = "configuration.aspx";
                mainLink = CommonLinkUtility.GetAdministration(ManagementType.HelpCenter);
            }
            else
            {
                var currentModule = GetProduct();
                if (currentModule == null) return;

                module = currentModule.ProductClassName + ".aspx";

                var link = currentModule.StartURL;
                mainLink = VirtualPathUtility.ToAbsolute(link + (link.LastIndexOf("/", StringComparison.Ordinal) == 0 ? "" : "/"));

                if (currentModule.ID != WebItemManager.DocumentsProductID
                && currentModule.ID != WebItemManager.MailProductID)
                {
                    mainLink += "help.aspx";
                }
            }
            const string index = "#help";
            HelpLink = mainLink + index;
            HelpLinkBlock = mainLink + index + "=";

            HelpCenterItems = HelpCenterHelper.GetHelpCenter(module, HelpLinkBlock);
        }

        private IWebItem GetProduct()
        {
            var currentModule = WebItemManager.Instance[ModuleId] ?? CommonLinkUtility.GetWebItemByUrl(Context.Request.Url.AbsoluteUri);

            if (currentModule is IAddon)
                return currentModule;

            var product = currentModule as IProduct;
            if (product != null)
                return product;

            if (currentModule == null) return null;

            IModule module;
            CommonLinkUtility.GetLocationByUrl(CommonLinkUtility.GetFullAbsolutePath(currentModule.StartURL), out product, out module);
            return product;
        }


        public static string RenderControlToString()
        {
            return RenderControlToString(null);
        }

        public static string RenderControlToString(Guid? moduleId)
        {
            var cntrl = new HelpCenter();
            cntrl = (HelpCenter)cntrl.LoadControl(Location);
            if (moduleId.HasValue)
            {
                cntrl.ModuleId = moduleId.Value;
            }

            var page = new Page();
            page.Controls.Add(cntrl);

            var writer = new StringWriter();
            HttpContext.Current.Server.Execute(page, writer, false);
            var renderedControl = writer.ToString();
            writer.Close();

            return renderedControl;
        }
    }
}