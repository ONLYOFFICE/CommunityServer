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
using ASC.Data.Storage;
using ASC.Web.Core.Mail;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.MailService, Location)]
    public partial class MailService : UserControl
    {
        public const string Location = "~/UserControls/Management/MailService/MailService.ascx";

        protected string SqlHost;
        protected string ApiHost;
        protected string User;
        protected string Password;

        protected string HelpLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts("~/UserControls/Management/MailService/js/mailservice.js");
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "mailservice_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebPath.GetPath("UserControls/Management/MailService/css/mailservice.css") + "\">", false);

            var mailServerInfo = CoreContext.Configuration.Standalone ? MailServiceHelper.GetMailServerInfo() : null;

            if (mailServerInfo == null)
            {
                SqlHost = string.Empty;
                ApiHost = string.Empty;
                User = MailServiceHelper.DefaultUser;
                Password = MailServiceHelper.DefaultPassword;
            }
            else
            {
                ApiHost = mailServerInfo.Api.Server;

                var connectionParts = mailServerInfo.DbConnection.Split(';');

                foreach (var connectionPart in connectionParts)
                {
                    if (connectionPart.StartsWith("Server="))
                        SqlHost = connectionPart.Replace("Server=", "");

                    if (connectionPart.StartsWith("User ID="))
                        User = connectionPart.Replace("User ID=", "");

                    if(connectionPart.StartsWith("Password="))
                        Password = connectionPart.Replace("Password=", "");
                }
            }

            HelpLink = CommonLinkUtility.GetHelpLink();
        }
    }
}