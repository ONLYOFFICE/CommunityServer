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
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Common.Logging;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using AjaxPro;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("AjaxPro.SmsValidationSettingsController")]
    public partial class SmsValidationSettings : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/SmsControls/SmsValidationSettings.ascx"; }
        }

        protected bool SmsEnable;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/Management/SmsControls/js/SmsValidation.js"));

            SmsEnable = StudioSmsNotificationSettings.IsVisibleSettings;

            if (SmsEnable)
            {
                SmsBuyHolder.Controls.Add(LoadControl(SmsBuy.Location));
            }
        }

        [AjaxMethod]
        public object SaveSettings(bool smsEnable)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                if (!StudioSmsNotificationSettings.IsVisibleSettings || CoreContext.PaymentManager.GetApprovedPartner() != null)
                {
                    throw new Exception(Resource.SmsNotAvailable);
                }

                if (smsEnable && StudioSmsNotificationSettings.LeftSms <= 0)
                    throw new Exception(Resource.SmsNotPaidError);

                StudioSmsNotificationSettings.Enable = smsEnable;

                AdminLog.PostAction("Settings: saved sms validation settings to {0}", smsEnable);

                MessageService.Send(HttpContext.Current.Request, MessageAction.TwoFactorAuthenticationSettingsUpdated);

                return new
                    {
                        Status = 1,
                        Message = Resource.SuccessfullySaveSettingsMessage
                    };
            }
            catch(Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }
    }
}