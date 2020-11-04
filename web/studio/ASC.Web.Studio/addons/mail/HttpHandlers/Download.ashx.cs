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
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Services;

using ASC.Common.Logging;
using ASC.Core;
using ASC.Mail.Core;
using ASC.Mail.Core.Dao.Expressions.Attachment;
using ASC.Mail.Data.Storage;
using ASC.Security.Cryptography;
using ASC.Web.Core.Files;
using ASC.Web.Mail.Resources;

using MimeMapping = ASC.Common.Web.MimeMapping;

namespace ASC.Web.Mail.HttpHandlers
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class DownloadHandler : IHttpHandler
    {
        private static int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private static string Username
        {
            get { return SecurityContext.CurrentAccount.ID.ToString(); }
        }

        public void ProcessRequest(HttpContext context)
        {
            var log = LogManager.GetLogger("ASC.Mail.DownloadHandler");

            try
            {
                context.Response.ContentType = "application/octet-stream";
                context.Response.Charset = Encoding.UTF8.WebName;

                var id = Convert.ToInt32(context.Request.QueryString["attachid"]);

                DownloadFile(id, context);
            }
            catch (HttpException he)
            {
                log.Error("Download handler failed", he);

                context.Response.StatusCode = he.GetHttpCode();
                context.Response.Write(he.Message != null ? HttpUtility.HtmlEncode(he.Message) : MailApiErrorsResource.ErrorInternalServer);
            }
            catch (Exception ex)
            {
                log.Error("Download handler failed", ex);

                context.Response.StatusCode = 404;
                context.Response.Redirect("404.html");
            }
            finally
            {
                try
                {
                    context.Response.Flush();
                    context.Response.SuppressContent = true;
                    context.ApplicationInstance.CompleteRequest();
                }
                catch (HttpException ex)
                {
                    LogManager.GetLogger("ASC").Error("ResponceContactPhotoUrl", ex);
                }
            }
        }

        private static void DownloadFile(int attachmentId, HttpContext context)
        {
            var auth = context.Request[FilesLinkUtility.AuthKey];

            var openTempStream = false;

            if (!string.IsNullOrEmpty(auth))
            {
                var stream = context.Request.QueryString["stream"];

                if (!string.IsNullOrEmpty(stream))
                {
                    int validateTimespan;
                    int.TryParse(ConfigurationManagerExtension.AppSettings["files.stream-url-minute"], out validateTimespan);
                    if (validateTimespan <= 0) validateTimespan = 5;

                    var validateResult = EmailValidationKeyProvider.ValidateEmailKey(attachmentId + stream, auth, TimeSpan.FromMinutes(validateTimespan));
                    if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
                    {
                        var exc = new HttpException((int)HttpStatusCode.Forbidden, "You don't have enough permission to perform the operation");
                        //Global.Logger.Error(string.Format("{0} {1}: {2}", CommonLinkUtility.AuthKey, validateResult, context.Request.Url), exc);
                        throw exc;
                    }

                    openTempStream = true;
                }
            }
            else
            {
                if (!SecurityContext.IsAuthenticated)
                {
                    throw new HttpException(403, "Access denied.");
                }
            }

            var engine = new EngineFactory(TenantId, Username);
            var attachment = engine.AttachmentEngine.GetAttachment(
                openTempStream
                    ? (IAttachmentExp)new ConcreteTenantAttachmentExp(attachmentId, TenantId)
                    : new ConcreteUserAttachmentExp(attachmentId, TenantId, Username));

            long offset = 0;
            long endOffset = -1;
            long length = attachment.size;

            if (context.Request.Headers["Range"] != null)
            {
                var range = context.Request.Headers["Range"].Split('=', '-');
                offset = Convert.ToInt64(range[1]);

                if (range.Count() > 2 && !string.IsNullOrEmpty(range[2]))
                {
                    endOffset = Convert.ToInt64(range[2]);
                }
                if (endOffset < 0 || endOffset >= attachment.size)
                {
                    endOffset = attachment.size - 1;
                }

                length = endOffset - offset + 1;

                if (length <= 0) throw new HttpException("Wrong Range header");

                context.Response.StatusCode = 206;
                context.Response.AddHeader("Content-Range", String.Format(" bytes {0}-{1}/{2}", offset, endOffset, attachment.size));
            }

            using (var file = attachment.ToAttachmentStream((int)offset))
            {
                context.Response.AddHeader("Accept-Ranges", "bytes");
                context.Response.AddHeader("Content-Length", length.ToString(CultureInfo.InvariantCulture));
                context.Response.AddHeader("Content-Disposition", ContentDispositionUtil.GetHeaderValue(file.FileName));
                context.Response.ContentType = MimeMapping.GetMimeMapping(file.FileName);

                if (endOffset != attachment.size - 1)
                {
                    file.FileStream.StreamCopyTo(context.Response.OutputStream);
                }
                else
                {
                    file.FileStream.StreamCopyTo(context.Response.OutputStream, (int)length);
                }
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}