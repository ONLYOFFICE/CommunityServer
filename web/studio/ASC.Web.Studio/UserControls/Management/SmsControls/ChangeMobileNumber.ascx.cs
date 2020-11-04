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