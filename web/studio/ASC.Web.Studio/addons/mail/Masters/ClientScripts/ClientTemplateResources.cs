/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.Client.HttpHandlers;

namespace ASC.Web.Mail.Masters.ClientScripts
{
    public class ClientTemplateResources : ClientScriptLocalization
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Mail.Master"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return RegisterClientTemplatesPath(context, 
                "~/addons/mail/templates/foldersTmpl.ascx",
                "~/addons/mail/templates/messagesTmpl.ascx",
                "~/addons/mail/templates/messageTmpl.ascx",
                "~/addons/mail/templates/editMessageTmpl.ascx",
                "~/addons/mail/templates/accountTmpl.ascx",
                "~/addons/mail/templates/accountErrorTmpl.ascx",
                "~/addons/mail/templates/accountWizardTmpl.ascx",
                "~/addons/mail/templates/accountsPanelTmpl.ascx",
                "~/addons/mail/templates/contactsTmpl.ascx",
                "~/addons/mail/templates/contactEditTmpl.ascx",
                "~/addons/mail/templates/accountsTmpl.ascx",
                "~/addons/mail/templates/tagsTmpl.ascx",
                "~/addons/mail/templates/crmIntegrationPopupTmpl.ascx",
                "~/addons/mail/templates/docAttachmentsTmpl.ascx",
                "~/addons/mail/templates/addFirstAccountTmpl.ascx",
                "~/addons/mail/templates/composeMessageBodyTmpl.ascx",
                "~/addons/mail/templates/signatureTmpl.ascx",
                "~/addons/mail/templates/mailAutoreplyTmpl.ascx",
                "~/addons/mail/templates/emptyScrTmpl.ascx",
                "~/addons/mail/templates/alertsTmpl.ascx",
                "~/addons/mail/templates/messagePrintTmpl.ascx",
                "~/addons/mail/templates/administration/mailboxTableTmpl.ascx",
                "~/addons/mail/templates/administration/groupTableTmpl.ascx",
                "~/addons/mail/templates/administration/editMailGroupTmpl.ascx",
                "~/addons/mail/templates/administration/editMailboxTmpl.ascx",
                "~/addons/mail/templates/administration/addQuestionBoxTmpl.ascx",
                "~/addons/mail/templates/administration/createDomainWizardTmpl.ascx",
                "~/addons/mail/templates/administration/createMailboxPopupTmpl.ascx",
                "~/addons/mail/templates/administration/administrationDataTmpl.ascx",
                "~/addons/mail/templates/administration/createMailgroupPopupTmpl.ascx",
                "~/addons/mail/templates/calendarTmpl.ascx",
                "~/addons/mail/templates/commonTmpl.ascx");
        }
    }
}