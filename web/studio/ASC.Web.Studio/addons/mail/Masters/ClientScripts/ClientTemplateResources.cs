/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/foldersTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/messagesTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/messageTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/editMessageTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/accountTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/accountErrorTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/accountWizardTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/accountsPanelTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/contactsTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/accountsTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/tagsTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/crmIntegrationPopupTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/docAttachmentsTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/addFirstAccountTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/composeMessageBodyTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/signatureTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/emptyScrTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/alertsTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/messagePrintTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/administration/mailboxTableTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/administration/groupTableTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/administration/editMailGroupTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/administration/editMailboxTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/administration/addQuestionBoxTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/administration/createDomainWizardTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/administration/createMailboxPopupTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/administration/administrationDataTmpl.ascx", context);
            yield return RegisterClientTemplatesPath("~/addons/mail/templates/administration/createMailgroupPopupTmpl.ascx", context);
        }
    }
}