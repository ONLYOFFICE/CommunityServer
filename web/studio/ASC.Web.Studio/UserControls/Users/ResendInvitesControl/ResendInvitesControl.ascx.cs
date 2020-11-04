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
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using ASC.MessagingSystem;
using AjaxPro;
using ASC.Web.Studio.Core.Notify;
using ASC.Core;
using ASC.Core.Users;

namespace ASC.Web.Studio.UserControls.Users
{
    [AjaxNamespace("InviteResender")]
    public partial class ResendInvitesControl : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/ResendInvitesControl/ResendInvitesControl.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/Users/ResendInvitesControl/js/resendinvitescontrol.js");

            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            _invitesResenderContainer.Options.IsPopup = true;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object Resend()
        {
            try
            {
                foreach (var user in new List<UserInfo>(CoreContext.UserManager.GetUsers())
                    .FindAll(u => u.ActivationStatus == EmployeeActivationStatus.Pending))
                {
                    if (user.IsVisitor())
                    {
                        StudioNotifyService.Instance.GuestInfoActivation(user);
                    }
                    else
                    {
                        StudioNotifyService.Instance.UserInfoActivation(user);
                    }
                    MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentActivationInstructions, MessageTarget.Create(user.ID), user.DisplayUserName(false));
                }

                return new {status = 1, message = Resources.Resource.SuccessResendInvitesText};
            }
            catch(Exception e)
            {
                return new {status = 0, message = e.Message.HtmlEncode()};
            }
        }

        public static string GetHrefAction()
        {
            return "javascript:InvitesResender.Show();";
        }
    }
}