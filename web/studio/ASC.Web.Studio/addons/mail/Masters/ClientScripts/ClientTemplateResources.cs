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