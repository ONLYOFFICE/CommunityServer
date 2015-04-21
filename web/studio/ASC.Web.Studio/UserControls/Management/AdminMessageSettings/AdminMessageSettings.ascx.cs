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
using System.Web.UI;
using ASC.Core;
using ASC.MessagingSystem;
using AjaxPro;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System.Web;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.PortalSecurity, Location, SortOrder = 300)]
    [AjaxNamespace("AdmMessController")]
    public partial class AdminMessageSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/AdminMessageSettings/AdminMessageSettings.ascx";

        protected StudioAdminMessageSettings _studioAdmMessNotifSettings;

        protected bool Enabled;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/Management/AdminMessageSettings/js/admmess.js"));

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/AdminMessageSettings/css/admmess.less"));

            _studioAdmMessNotifSettings = SettingsManager.Instance.LoadSettings<StudioAdminMessageSettings>(TenantProvider.CurrentTenantID);

            Enabled = !SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID).Anyone;
        }

        [AjaxMethod]
        public object SaveSettings(bool turnOn)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var passwordSettingsObj = new StudioAdminMessageSettings {Enable = turnOn};
                SettingsManager.Instance.SaveSettings(passwordSettingsObj, TenantProvider.CurrentTenantID);

                MessageService.Send(HttpContext.Current.Request, MessageAction.AdministratorMessageSettingsUpdated);

                return new
                    {
                        Status = 1,
                        Message = Resources.Resource.SuccessfullySaveSettingsMessage
                    };
            }
            catch(Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }
    }
}