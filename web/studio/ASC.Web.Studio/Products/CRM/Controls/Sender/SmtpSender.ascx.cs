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
            get { return PathProvider.GetFileStaticRelativePath("Sender/SmtpSender.ascx"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            phFileUploader.Controls.Add(LoadControl(FileUploader.Location));

            RegisterScript();
        }

        protected string RenderTagSelector(bool isCompany)
        {
            var sb = new StringBuilder();
            var manager = new MailTemplateManager(DaoFactory);
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
            Page.RegisterBodyScripts("~/UserControls/Common/ckeditor/ckeditor-connector.js");

            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.SmtpSender.init({0});", MailSender.GetQuotas());

            Page.RegisterInlineScript(sb.ToString());
        }

    }
}