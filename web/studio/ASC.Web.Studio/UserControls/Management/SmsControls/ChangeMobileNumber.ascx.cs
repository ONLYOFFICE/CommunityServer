/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.ServiceModel.Security;
using System.Web;
using System.Web.UI;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;
using AjaxPro;
using ASC.Core;
using ASC.Web.Studio.Core.Notify;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("AjaxPro.ChangeMobileNumber")]
    public partial class ChangeMobileNumber : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/SmsControls/ChangeMobileNumber.ascx"; }
        }

        public UserInfo User;

        protected void Page_Load(object sender, EventArgs e)
        {
            _changePhoneContainer.Options.IsPopup = true;

            Page.RegisterBodyScripts("~/UserControls/Management/SmsControls/js/changemobile.js");

            AjaxPro.Utility.RegisterTypeForAjax(GetType());
        }

        [AjaxMethod]
        public string SendNotificationToChange(string userId)
        {
            var user = CoreContext.UserManager.GetUsers(
                string.IsNullOrEmpty(userId)
                    ? SecurityContext.CurrentAccount.ID
                    : new Guid(userId));

            var canChange =
                user.IsMe()
                || SecurityContext.CheckPermissions(new UserSecurityProvider(user.ID), ASC.Core.Users.Constants.Action_EditUser);

            if (!canChange)
                throw new SecurityAccessDeniedException(Resource.ErrorAccessDenied);

            user.MobilePhoneActivationStatus = MobilePhoneActivationStatus.NotActivated;
            CoreContext.UserManager.SaveUserInfo(user);

            if (user.IsMe())
            {
                return CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.PhoneActivation);
            }

            StudioNotifyService.Instance.SendMsgMobilePhoneChange(user);
            return string.Empty;
        }
    }
}