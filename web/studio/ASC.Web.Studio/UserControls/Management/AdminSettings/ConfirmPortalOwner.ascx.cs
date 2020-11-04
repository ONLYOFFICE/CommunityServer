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
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    partial class ConfirmPortalOwner : UserControl
    {
        public static readonly string Location = "~/UserControls/Management/AdminSettings/ConfirmPortalOwner.ascx";

        protected string _newOwnerName = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var newOwner = Constants.LostUser;
                try
                {
                    newOwner = CoreContext.UserManager.GetUsers(new Guid(Request["uid"]));
                }
                catch
                {
                }
                if (Constants.LostUser.Equals(newOwner))
                {
                    throw new Exception(Resource.ErrorUserNotFound);
                }
                _newOwnerName = newOwner.DisplayUserName();

                if (IsPostBack)
                {
                    if (CoreContext.UserManager.IsUserInGroup(newOwner.ID, Constants.GroupVisitor.ID))
                    {
                        throw new Exception(Resource.ErrorUserNotFound);
                    }

                    var curTenant = CoreContext.TenantManager.GetCurrentTenant();
                    curTenant.OwnerId = newOwner.ID;
                    CoreContext.TenantManager.SaveTenant(curTenant);

                    MessageService.Send(HttpContext.Current.Request, MessageAction.OwnerUpdated, newOwner.DisplayUserName(false));

                    _messageHolder.Visible = true;
                    _confirmContentHolder.Visible = false;
                }
                else
                {
                    _messageHolder.Visible = false;
                    _confirmContentHolder.Visible = true;
                }
            }
            catch(Exception err)
            {
                ((Confirm)Page).ErrorMessage = err.Message.HtmlEncode();
                _messageHolder.Visible = true;
                _confirmContentHolder.Visible = false;
                LogManager.GetLogger("ASC.Web").Error(err);
            }
        }
    }
}