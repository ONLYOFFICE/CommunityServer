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
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Core.Notify;

namespace ASC.Web.Studio.UserControls.Management
{
    public partial class ProfileOperation : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/ProfileOperation.ascx"; }
        }

        public Guid UserId { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            operationBlock.Visible = true;
            result.Visible = false;
            errorBlock.Visible = false;
        }

        protected void DeleteProfile(object sender, EventArgs e)
        {
            try
            {
                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                var user = CoreContext.UserManager.GetUsers(UserId);
                user.Status = EmployeeStatus.Terminated;

                CoreContext.UserManager.SaveUserInfo(user);
                MessageService.Send(HttpContext.Current.Request, MessageInitiator.System, MessageAction.UsersUpdatedStatus, MessageTarget.Create(user.ID), user.DisplayUserName(false));

                CookiesManager.ResetUserCookie(user.ID);
                MessageService.Send(HttpContext.Current.Request, MessageAction.CookieSettingsUpdated);

                if (CoreContext.Configuration.Personal)
                {
                    UserPhotoManager.RemovePhoto(user.ID);
                    CoreContext.UserManager.DeleteUser(user.ID);
                    MessageService.Send(Request, MessageAction.UserDeleted, MessageTarget.Create(user.ID), user.DisplayUserName(false));
                }
                else
                {
                    StudioNotifyService.Instance.SendMsgProfileHasDeletedItself(user);
                }

                operationBlock.Visible = false;
                result.Visible = true;
                errorBlock.Visible = false;
            }
            catch(Exception ex)
            {
                operationBlock.Visible = false;
                result.Visible = false;
                errorBlock.InnerHtml = ex.Message;
                errorBlock.Visible = true;
            }
            finally
            {
                Auth.ProcessLogout();
            }
        }
    }
}