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
using ASC.MessagingSystem;
using ASC.Web.Studio.Core;
using AjaxPro;
using ASC.Web.Core.Security;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using Resources;
using ASC.Core.Users;

namespace ASC.Web.Studio.UserControls.Users.UserProfile
{
    [AjaxNamespace("PwdTool")]
    public partial class PwdTool : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/PwdTool.ascx"; }
        }

        public Guid UserID { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            _pwdRemainderContainer.Options.IsPopup = true;
            _pwdRemainderContainer.Options.InfoMessageText = "";
            _pwdRemainderContainer.Options.InfoType = InfoType.Info;

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/users/UserProfile/js/pwdtool.js"));

            AjaxPro.Utility.RegisterTypeForAjax(GetType());
        }

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse RemindPwd(string email)
        {
            var responce = new AjaxResponse {rs1 = "0"};

            if (!email.TestEmailRegex())
            {
                responce.rs2 = "<div>" + Resource.ErrorNotCorrectEmail + "</div>";
                return responce;
            }

            try
            {
                UserManagerWrapper.SendUserPassword(email);

                responce.rs1 = "1";
                responce.rs2 = String.Format(Resource.MessageYourPasswordSuccessfullySendedToEmail, "<b>" + email + "</b>");

                var user = CoreContext.UserManager.GetUserByEmail(email).DisplayUserName(false);
                MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentPasswordChangeInstructions, user);
            }
            catch(Exception exc)
            {
                responce.rs2 = "<div>" + HttpUtility.HtmlEncode(exc.Message) + "</div>";
            }

            return responce;
        }
    }
}