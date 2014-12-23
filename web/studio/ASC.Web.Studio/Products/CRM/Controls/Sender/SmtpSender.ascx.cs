/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Text;
using System.Web;
using ASC.Web.CRM.Controls.Common;
using ASC.Web.Core.Mobile;
using ASC.Web.CRM.Classes;
using AjaxPro;
using ASC.Common.Threading.Progress;
using System.Collections.Generic;
using System.Globalization;

namespace ASC.Web.CRM.Controls.Sender
{
    [AjaxNamespace("AjaxPro.SmtpSender")]
    public partial class SmtpSender : BaseUserControl
    {
        public static String Location
        {
            get { return PathProvider.GetFileStaticRelativePath("sender/smtpsender.ascx"); }
        }

        protected bool MobileVer = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(typeof(SmtpSender));
            
            MobileVer = MobileDetector.IsMobile;

            if (!MobileVer)
            {
                phFileUploader.Controls.Add(LoadControl(FileUploader.Location));
            }

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
            Page.RegisterClientScript(typeof(Masters.ClientScripts.SmtpSenderData));

            if (!MobileVer)
            {
                Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/usercontrols/common/ckeditor/ckeditor-connector.js"));
            }

            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.SmtpSender.init({0});", MailSender.GetQuotas());

            Page.RegisterInlineScript(sb.ToString());
        }

        #region AjaxMethods

        [AjaxMethod]
        public IProgressItem SendEmail(List<int> fileIDs, List<int> contactIds, String subjectTemplate, String bodyTemplate, bool storeInHistory)
        {
            var joinFileIDs = string.Join("|", fileIDs.ConvertAll(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
            var joinContactIDs = string.Join("|", contactIds.ConvertAll(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
            return MailSender.Start(fileIDs, contactIds, subjectTemplate, bodyTemplate, storeInHistory);
        }

        [AjaxMethod]
        public IProgressItem GetStatus()
        {
            return MailSender.GetStatus();
        }

        [AjaxMethod]
        public IProgressItem Cancel()
        {
            var progressItem = GetStatus();

            MailSender.Cancel();

            return progressItem;
        }

        [AjaxMethod]
        public string GetMessagePreview(string template, int contactId)
        {
            var manager = new MailTemplateManager();

            return manager.Apply(template, contactId);
        }

        #endregion
    }
}