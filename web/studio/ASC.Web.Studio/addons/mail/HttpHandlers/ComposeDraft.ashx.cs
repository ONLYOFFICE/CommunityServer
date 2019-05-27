using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Mail.Core;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Data.Contracts;
using ASC.Web.Mail.Resources;

namespace ASC.Web.Mail.HttpHandlers
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class ComposeDraftHandler : IHttpHandler
    {
        private static int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private static string Username
        {
            get { return SecurityContext.CurrentAccount.ID.ToString(); }
        }

        private static EngineFactory Factory
        {
            get { return new EngineFactory(TenantId, Username); }
        }

        public void ProcessRequest(HttpContext context)
        {
            var log = LogManager.GetLogger("ASC.Mail.ComposeDraftHandler");

            var messageId = 0;

            try
            {
                if (!SecurityContext.IsAuthenticated)
                {
                    throw new HttpException(403, "Access denied.");
                }

                var mailboxes = Factory.MailboxEngine.GetMailboxDataList(new UserMailboxesExp(TenantId, Username));

                if (mailboxes.Any())
                {
                    var mailbox = mailboxes.First();

                    var message = Factory.DraftEngine.Save(0, mailbox.EMail.Address, new List<string>(),
                        new List<string>(), new List<string>(), "", false, "", new List<int>(),
                        "", new List<MailAttachmentData>(), "");

                    messageId = message.Id;

                    var fileId = context.Request.QueryString["fileId"];

                    if (!string.IsNullOrEmpty(fileId))
                    {
                        var version = context.Request.QueryString["version"];
                        Factory.AttachmentEngine
                            .AttachFileFromDocuments(TenantId, Username, message.Id, fileId, version);
                    }
                }
            }
            catch (HttpException he)
            {
                log.Error("ComposeDraft handler failed", he);

                context.Response.StatusCode = he.GetHttpCode();
                context.Response.Write(he.Message != null ? HttpUtility.HtmlEncode(he.Message) : MailApiErrorsResource.ErrorInternalServer);
            }
            catch (Exception ex)
            {
                log.Error("ComposeDraft handler failed", ex);
            }

            if (messageId != 0)
            {
                context.Response.Redirect("~/addons/mail/#draftitem/" + messageId);
            }
            else
            {
                context.Response.Redirect(@"~/addons/mail/");
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}