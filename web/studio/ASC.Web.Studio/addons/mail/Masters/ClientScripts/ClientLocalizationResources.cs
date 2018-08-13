/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using ASC.Web.Mail.Resources;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Helpers;
using Resources;

namespace ASC.Web.Mail.Masters.ClientScripts
{
    public class ClientLocalizationResources : ClientScriptLocalization
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Mail.Resources"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var autoreplyDaysInterval = Convert.ToInt32(ConfigurationManager.AppSettings["mail.autoreply-days-interval"] ?? "4");

            return new List<KeyValuePair<string, object>>(6)
            {
                RegisterResourceSet("MailResource", MailResource.ResourceManager),
                RegisterResourceSet("MailScriptResource", MailScriptResource.ResourceManager),
                RegisterResourceSet("MailAttachmentsResource", MailAttachmentsResource.ResourceManager),
                RegisterResourceSet("MailActionCompleteResource", MailActionCompleteResource.ResourceManager),
                RegisterResourceSet("MailAdministrationResource", MailAdministrationResource.ResourceManager),
                RegisterResourceSet("MailApiErrorsResource", MailApiErrorsResource.ResourceManager),
                RegisterObject(new
                {
                    ErrorOpenMessageHelp = string.Format(MailScriptResource.ErrorOpenMessageHelp.HtmlEncode(), "<a href=\"" + MailPage.GetMailSupportUri() + "\" target=\"_blank\">", "</a>"),
                    ErrorParseMessageHelp = string.Format(MailScriptResource.ErrorParseMessageHelp.HtmlEncode(), "<a href=\"" + MailPage.GetMailSupportUri() + "\" target=\"_blank\">", "</a>"),
                    FilesCannotBeAttachedAsLinks = string.Format(MailResource.FilesCannotBeAttachedAsLinks_Body, "<br/>"),
                    Autoreply = string.Format(MailResource.AutoreplyInformationText, string.Format(GrammaticalHelper.ChooseNumeralCase(autoreplyDaysInterval, Resource.DrnAgoDaysI, Resource.DrnAgoDaysR1, Resource.DrnAgoDaysRm), autoreplyDaysInterval))
                })
            };
        }
    }
}
