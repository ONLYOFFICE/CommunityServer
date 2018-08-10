/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

        private void DownloadFile(int attachmentId, HttpContext context)
        {
            var mailBoxManager = new MailBoxManager();

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

            using (var file = openTempStream
                                  ? mailBoxManager.GetAttachmentStream(attachmentId, TenantId)
                                  : mailBoxManager.GetAttachmentStream(attachmentId, TenantId, Username))
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
