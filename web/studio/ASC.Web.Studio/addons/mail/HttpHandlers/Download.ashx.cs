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


using System;
using System.Globalization;
using System.Net;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
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
                    int.TryParse(WebConfigurationManager.AppSettings["files.stream-url-minute"], out validateTimespan);
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