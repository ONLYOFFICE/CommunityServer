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
using System.Web;
using System.Web.Services;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Mail.Core;
using ASC.Mail.Core.Dao.Expressions.Attachment;
using ASC.Mail.Data.Storage;
using ASC.Web.Core.Files;
using ASC.Web.Mail.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Mail.HttpHandlers
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class ViewDocumentHandler : IHttpHandler
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
            var log = LogManager.GetLogger("ASC.Mail.ViewDocumentHandler");

            try
            {
                var id = Convert.ToInt32(context.Request.QueryString["attachid"]);
                ViewDocument(id, context);
            }
            catch (HttpException he)
            {
                log.Error("ViewDocument handler failed", he);

                context.Response.StatusCode = he.GetHttpCode();
                context.Response.Write(he.Message != null ? HttpUtility.HtmlEncode(he.Message) : MailApiErrorsResource.ErrorInternalServer);
            }
            catch (Exception ex)
            {
                log.Error("ViewDocument handler failed", ex);

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

        private static void ViewDocument(int attachmentId, HttpContext context)
        {
            var engine = new EngineFactory(TenantId, Username);
            var file = engine.AttachmentEngine.GetAttachment(
                new ConcreteUserAttachmentExp(attachmentId, TenantId, Username));

            var tempFileUrl = MailStoragePathCombiner.GetPreSignedUrl(file);

            var viewerUrl = FilesLinkUtility.GetFileWebViewerExternalUrl(tempFileUrl, file.fileName);
            viewerUrl = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(viewerUrl)).ToString();
            context.Response.Redirect(viewerUrl, false);
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
