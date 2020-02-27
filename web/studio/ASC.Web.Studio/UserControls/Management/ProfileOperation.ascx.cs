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