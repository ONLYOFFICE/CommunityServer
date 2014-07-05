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
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Web.CRM.SocialMedia;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.CRM.Controls.Common;
using ASC.CRM.Core;

using AjaxPro;

namespace ASC.Web.CRM
{
    public partial class BasicTemplate : MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(typeof(SocialMediaUI));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();

            Page.EnableViewState = false;

            Page.RegisterClientScript(typeof(Masters.ClientScripts.CRMSettingsResources));
            Page.RegisterClientScript(typeof(Masters.ClientScripts.ClientCustomResources));
            Page.RegisterClientLocalizationScript(typeof(Masters.ClientScripts.ClientLocalizationResources));
            Page.RegisterClientLocalizationScript(typeof(Masters.ClientScripts.ClientTemplateResources));

            Page.RegisterClientScript(typeof(Masters.ClientScripts.CommonData));
        }

        protected void RegisterScriptForTaskAction()
        {
            Page.RegisterClientScript(typeof(Masters.ClientScripts.TaskActionViewData));
            Page.RegisterInlineScript("ASC.CRM.NavSidePanel.init();");
        }

        protected void InitControls()
        {
            SideNavigation.Controls.Add(LoadControl(NavigationSidePanel.Location));

            Page.RegisterStyleControl(LoadControl(VirtualPathUtility.ToAbsolute("~/products/crm/masters/Styles.ascx")));
            Page.RegisterBodyScripts(LoadControl(VirtualPathUtility.ToAbsolute("~/products/crm/masters/CommonBodyScripts.ascx")));

            RegisterScriptForTaskAction();
        }

        #region Methods

        public string CurrentPageCaption
        {
            get { return _commonContainer.CurrentPageCaption; }
            set { _commonContainer.CurrentPageCaption = value; }
        }

        public String CommonContainerHeader
        {
            set { _commonContainer.Options.HeaderBreadCrumbCaption = value; }
        }

        #endregion
    }
}