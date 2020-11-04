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
            var autoreplyDaysInterval = Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.autoreply-days-interval"] ?? "1");

            return new List<KeyValuePair<string, object>>(6)
            {
                RegisterResourceSet("MailResource", MailResource.ResourceManager),
                RegisterResourceSet("MailScriptResource", MailScriptResource.ResourceManager),
                RegisterResourceSet("MailAttachmentsResource", MailAttachmentsResource.ResourceManager),
                RegisterResourceSet("MailActionCompleteResource", MailActionCompleteResource.ResourceManager),
                RegisterResourceSet("MailAdministrationResource", MailAdministrationResource.ResourceManager),
                RegisterResourceSet("MailApiErrorsResource", MailApiErrorsResource.ResourceManager),
                RegisterResourceSet("MailApiResource",MailApiResource.ResourceManager),
                RegisterObject(new
                {
                    ErrorOpenMessageHelp = string.Format(MailScriptResource.ErrorOpenMessageHelp.HtmlEncode(), "<a href=\"" + MailPage.GetMailSupportUri() + "\" target=\"_blank\">", "</a>"),
                    ErrorParseMessageHelp = string.Format(MailScriptResource.ErrorParseMessageHelp.HtmlEncode(), "<a href=\"" + MailPage.GetMailSupportUri() + "\" target=\"_blank\">", "</a>"),
                    FilesCannotBeAttachedAsLinks = string.Format(MailResource.FilesCannotBeAttachedAsLinks_Body, "<br/>"),
                    Autoreply = string.Format(MailResource.AutoreplyInformationText, string.Format(GrammaticalHelper.ChooseNumeralCase(autoreplyDaysInterval, Resource.DrnAgoDaysI, Resource.DrnAgoDaysR1, Resource.DrnAgoDaysRm), autoreplyDaysInterval)),
                    Resource.ErrorPasswordMessage,
                    Resource.ErrorPasswordLength,
                    Resource.ErrorPasswordNoDigits,
                    Resource.ErrorPasswordNoUpperCase,
                    Resource.ErrorPasswordNoSpecialSymbols,
                    Resource.CopyEmailAndPassword,
                    Resource.EmailAndPasswordCopiedToClipboard,
                    Resource.ChangePasswordSuccess,
                    Resource.ErrorPasswordOnlyLatinLetters,
                    Resource.ErrorPasswordNoSpaces
                })
            };
        }
    }
}
