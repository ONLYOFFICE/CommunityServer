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


using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Services;
using ASC.Core;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.DataStorage;
using ASC.Web.Core.Files;
using Ionic.Zip;
using Ionic.Zlib;

namespace ASC.Web.Mail.HttpHandlers
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class DownloadAllHandler : IHttpHandler
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
                if (!MailPage.IsTurnOnAttachmentsGroupOperations())
                    throw new Exception("Operation is turned off.");

                context.Response.ContentType = "application/octet-stream";
                context.Response.Charset = Encoding.UTF8.WebName;

                int messageId = Convert.ToInt32(context.Request.QueryString["messageid"]);

                DownloadAllZipped(messageId, context);

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

        // ToDo : move constant to config
        private const string ARCHIVE_NAME = "download.zip";

        private void DownloadAllZipped(int messageId, HttpContext context)
        {
            var mailBoxManager = new MailBoxManager();

            var attachments = mailBoxManager.GetMessageAttachments(TenantId, Username, messageId);

            if (attachments.Any())
            {
                using (var zip = new ZipFile())
                {
                    zip.CompressionLevel = CompressionLevel.Level3;
                    zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                    zip.AlternateEncoding = Encoding.GetEncoding(Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage);

                    foreach (var attachment in attachments)
                    {
                        using (var file = AttachmentManager.GetAttachmentStream(attachment))
                        {
                            var filename = file.FileName;

                            if (zip.ContainsEntry(filename))
                            {
                                var counter = 1;
                                var tempName = filename;
                                while (zip.ContainsEntry(tempName))
                                {
                                    tempName = filename;
                                    var suffix = " (" + counter + ")";
                                    tempName = 0 < tempName.IndexOf('.')
                                                   ? tempName.Insert(tempName.LastIndexOf('.'), suffix)
                                                   : tempName + suffix;

                                    counter++;
                                }
                                filename = tempName;
                            }

                            zip.AddEntry(filename, file.FileStream.GetCorrectBuffer());
                        }
                    }

                    context.Response.AddHeader("Content-Disposition", ContentDispositionUtil.GetHeaderValue(ARCHIVE_NAME));

                    zip.Save(context.Response.OutputStream);

                }

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
