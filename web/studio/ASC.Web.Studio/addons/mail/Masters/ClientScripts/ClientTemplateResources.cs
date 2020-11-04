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


using ASC.Web.Core.Client.HttpHandlers;

namespace ASC.Web.Mail.Masters.ClientScripts
{
    public class ClientTemplateResources : ClientScriptTemplate
    {
        protected override string[] Links
        {
            get
            {
                return new[]
                {
                    "~/Templates/ProgressDialogTemplate.html",
                    "~/addons/mail/templates/foldersTmpl.html",
                    "~/addons/mail/templates/messagesTmpl.html",
                    "~/addons/mail/templates/messageTmpl.html",
                    "~/addons/mail/templates/editMessageTmpl.html",
                    "~/addons/mail/templates/accountTmpl.html",
                    "~/addons/mail/templates/accountErrorTmpl.html",
                    "~/addons/mail/templates/accountWizardTmpl.html",
                    "~/addons/mail/templates/accountsPanelTmpl.html",
                    "~/addons/mail/templates/contactsTmpl.html",
                    "~/addons/mail/templates/contactEditTmpl.html",
                    "~/addons/mail/templates/accountsTmpl.html",
                    "~/addons/mail/templates/tagsTmpl.html",
                    "~/addons/mail/templates/crmIntegrationPopupTmpl.html",
                    "~/addons/mail/templates/docAttachmentsTmpl.html",
                    "~/addons/mail/templates/addFirstAccountTmpl.html",
                    "~/addons/mail/templates/contentLossWarning.html",
                    "~/addons/mail/templates/composeMessageBodyTmpl.html",
                    "~/addons/mail/templates/signatureTmpl.html",
                    "~/addons/mail/templates/mailAutoreplyTmpl.html",
                    "~/addons/mail/templates/emptyScrTmpl.html",
                    "~/addons/mail/templates/alertsTmpl.html",
                    "~/addons/mail/templates/messagePrintTmpl.html",
                    "~/addons/mail/templates/administration/mailboxTableTmpl.html",
                    "~/addons/mail/templates/administration/groupTableTmpl.html",
                    "~/addons/mail/templates/administration/editMailGroupTmpl.html",
                    "~/addons/mail/templates/administration/editMailboxTmpl.html",
                    "~/addons/mail/templates/administration/addQuestionBoxTmpl.html",
                    "~/addons/mail/templates/administration/createDomainWizardTmpl.html",
                    "~/addons/mail/templates/administration/createMailboxPopupTmpl.html",
                    "~/addons/mail/templates/administration/administrationDataTmpl.html",
                    "~/addons/mail/templates/administration/createMailgroupPopupTmpl.html",
                    "~/addons/mail/templates/administration/changeMailboxPasswordPopupTmpl.html",
                    "~/addons/mail/templates/administration/mailboxSettingsPopupTmpl.html",
                    "~/addons/mail/templates/calendarTmpl.html",
                    "~/addons/mail/templates/commonTmpl.html",
                    "~/addons/mail/templates/filters/editFilterTmpl.html",
                    "~/addons/mail/templates/filters/filtersTmpl.html",
                    "~/addons/mail/templates/mailTemplateAdvansedSelectorTmpl.html"
                };
            }
        }
    }
}