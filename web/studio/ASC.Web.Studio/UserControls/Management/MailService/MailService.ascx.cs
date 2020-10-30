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
        protected string Database;
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
                Database = MailServiceHelper.DefaultDatabase;
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

                    if (connectionPart.StartsWith("Database="))
                        Database = connectionPart.Replace("Database=", "");

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