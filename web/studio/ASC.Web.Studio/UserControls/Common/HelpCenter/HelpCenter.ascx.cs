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

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/common/helpcenter/css/help-center.less"));

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