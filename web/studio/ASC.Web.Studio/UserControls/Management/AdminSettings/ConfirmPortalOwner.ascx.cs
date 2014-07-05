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
                log4net.LogManager.GetLogger("ASC.Web").Error(err);
            }
        }
    }
}