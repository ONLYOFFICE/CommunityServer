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
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Services;
using ASC.Core;
using ASC.Mail.Aggregator;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;

namespace ASC.Web.Mail.HttpHandlers
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class DownloadHandler : IHttpHandler
    {
        private int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private string Username
        {
            get { return SecurityContext.CurrentAccount.ID.ToString(); }
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                context.Response.ContentType = "application/octet-stream";
                context.Response.Charset = Encoding.UTF8.WebName;

                int id = Convert.ToInt32(context.Request.QueryString["attachid"]);

                DownloadFile(id, context);
            }
            catch (Exception)
            {
                context.Response.Redirect("404.html");
            }
            finally
            {
                context.Response.End();
            }
        }

        private void DownloadFile(int attachment_id, HttpContext context)
        {
            var mail_box_manager = new MailBoxManager(0);

            var auth = context.Request[FilesLinkUtility.AuthKey];

            var open_temp_stream = false;

            if (!string.IsNullOrEmpty(auth))
            {
                var stream = context.Request.QueryString["stream"];

                if (!string.IsNullOrEmpty(stream))
                {
                    int validate_timespan;
                    int.TryParse(WebConfigurationManager.AppSettings["files.stream-url-minute"], out validate_timespan);
                    if (validate_timespan <= 0) validate_timespan = 5;

                    var validate_result = EmailValidationKeyProvider.ValidateEmailKey(attachment_id + stream, auth, TimeSpan.FromMinutes(validate_timespan));
                    if (validate_result != EmailValidationKeyProvider.ValidationResult.Ok)
                    {
                        var exc = new HttpException((int)HttpStatusCode.Forbidden, "You don't have enough permission to perform the operation");
                        //Global.Logger.Error(string.Format("{0} {1}: {2}", CommonLinkUtility.AuthKey, validateResult, context.Request.Url), exc);
                        throw exc;
                    }

                    open_temp_stream = true;
                }
            }

            using (var file = open_temp_stream
                                  ? mail_box_manager.GetAttachmentStream(attachment_id, TenantId)
                                  : mail_box_manager.GetAttachmentStream(attachment_id, TenantId, Username))
            {
                context.Response.AddHeader("Content-Disposition", ContentDispositionUtil.GetHeaderValue(file.FileName));
                file.FileStream.StreamCopyTo(context.Response.OutputStream);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
