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


using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Common;
using System;
using System.Text;
using System.Web;

namespace ASC.Web.CRM.Controls.Sender
{
    public partial class SmtpSender : BaseUserControl
    {
        public static String Location
        {
            get { return PathProvider.GetFileStaticRelativePath("sender/smtpsender.ascx"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            phFileUploader.Controls.Add(LoadControl(FileUploader.Location));

            RegisterScript();
        }

        protected string RenderTagSelector(bool isCompany)
        {
            var sb = new StringBuilder();
            var manager = new MailTemplateManager();
            var tags = manager.GetTags(isCompany);

            var current = tags[0].Category;
            sb.AppendFormat("<optgroup label='{0}'>", current);

            foreach (var tag in tags)
            {
                if (tag.Category != current)
                {
                    current = tag.Category;
                    sb.Append("</optgroup>");
                    sb.AppendFormat("<optgroup label='{0}'>", current);
                }

                sb.AppendFormat("<option value='{0}'>{1}</option>",
                                tag.Name,
                                tag.DisplayName);
            }

            sb.Append("</optgroup>");

            return sb.ToString();
        }

        private void RegisterScript()
        {
            Page.RegisterClientScript(new Masters.ClientScripts.SmtpSenderData());
            Page.RegisterBodyScripts("~/usercontrols/common/ckeditor/ckeditor-connector.js");

            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.SmtpSender.init({0});", MailSender.GetQuotas());

            Page.RegisterInlineScript(sb.ToString());
        }

    }
}