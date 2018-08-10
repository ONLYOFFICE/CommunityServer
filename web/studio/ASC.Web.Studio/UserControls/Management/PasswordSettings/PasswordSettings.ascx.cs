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


using System;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.PortalSecurity, Location)]
    [AjaxNamespace("PasswordSettingsController")]
    public partial class PasswordSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/PasswordSettings/PasswordSettings.ascx";

        protected bool Enabled
        {
            get { return SetupInfo.IsVisibleSettings("PasswordSettings"); }
        }

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Enabled) return;

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts("~/usercontrols/management/PasswordSettings/js/PasswordSettings.js")
                .RegisterStyle("~/usercontrols/management/passwordsettings/css/passwordsettings.less");

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        [AjaxMethod]
        public object SavePasswordSettings(string objData)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var passwordSettingsObj = jsSerializer.Deserialize<Web.Core.Utility.PasswordSettings>(objData);
                passwordSettingsObj.Save();

                MessageService.Send(HttpContext.Current.Request, MessageAction.PasswordStrengthSettingsUpdated);

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

        [AjaxMethod]
        public string LoadPasswordSettings()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            var passwordSettingsObj = Web.Core.Utility.PasswordSettings.Load();

            return serializer.Serialize(passwordSettingsObj);
        }
    }
}