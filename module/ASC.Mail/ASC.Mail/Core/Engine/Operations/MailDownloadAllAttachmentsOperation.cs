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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using ASC.Common.Logging;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Resources;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Core.Dao.Expressions.Attachment;
using ASC.Mail.Data.Storage;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Files.Core;
using ASC.Data.Storage;

using Ionic.Zip;
using Ionic.Zlib;
using Resources;

namespace ASC.Mail.Core.Engine.Operations
{
    public class MailDownloadAllAttachmentsOperation : MailOperation
    {
        public int MessageId { get; set; }

        public ILog Log { get; set; }

        public override MailOperationType OperationType
        {
            get { return MailOperationType.DownloadAllAttachments; }
        }

        public MailDownloadAllAttachmentsOperation(Tenant tenant, IAccount user, int messageId)
            : base(tenant, user)
        {
            MessageId = messageId;

            Log = LogManager.GetLogger("ASC.Mail.MailDownloadAllAttachmentsOperation");
        }

        protected override void Do()
        {
            try
            {
                SetProgress((int?) MailOperationDownloadAllAttachmentsProgress.Init);

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);

                try
                {
                    SecurityContext.AuthenticateMe(CurrentUser);
                }
                catch
                {
                    Error = Resource.SsoSettingsNotValidToken;
                    Logger.Error(Error);
                }

                var engine = new EngineFactory(CurrentTenant.TenantId, CurrentUser.ID.ToString());

                SetProgress((int?) MailOperationDownloadAllAttachmentsProgress.GetAttachments);

                var attachments =
                    engine.AttachmentEngine.GetAttachments(new ConcreteMessageAttachmentsExp(MessageId,
                        CurrentTenant.TenantId, CurrentUser.ID.ToString()));

                if (!attachments.Any())
                {
                    Error = MailCoreResource.NoAttachmentsInMessage;

                    throw new Exception(Error);
                }

                SetProgress((int?) MailOperationDownloadAllAttachmentsProgress.Zipping);

                var damagedAttachments = 0;

                using (var stream = TempStream.Create())
                {
                    using (var zip = new ZipOutputStream(stream, true))
                    {
                        zip.CompressionLevel = CompressionLevel.Level3;
                        zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                        zip.AlternateEncoding =
                            Encoding.GetEncoding(Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage);

                        var attachmentsCount = attachments.Count;
                        var progressMaxValue = (int) MailOperationDownloadAllAttachmentsProgress.ArchivePreparation;
                        var progressMinValue = (int) MailOperationDownloadAllAttachmentsProgress.Zipping;
                        var progresslength = progressMaxValue - progressMinValue;
                        var progressStep = (double) progresslength/attachmentsCount;
                        var zippingProgress = 0.0;

                        foreach (var attachment in attachments)
                        {
                            try
                            {
                                using (var file = attachment.ToAttachmentStream())
                                {
                                    ZipFile(zip, file.FileName, file.FileStream);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);

                                Error = string.Format(MailCoreResource.FileNotFoundOrDamaged, attachment.fileName);

                                damagedAttachments++;

                                ZipFile(zip, attachment.fileName); // Zip empty file
                            }

                            zippingProgress += progressStep;

                            SetProgress(progressMinValue + (int?) zippingProgress);
                        }
                    }

                    SetProgress((int?) MailOperationDownloadAllAttachmentsProgress.ArchivePreparation);

                    if (stream.Length == 0)
                    {
                        Error = "File stream is empty";

                        throw new Exception(Error);
                    }

                    stream.Position = 0;

                    var store = Global.GetStore();

                    var path = store.Save(
                        FileConstant.StorageDomainTmp,
                        string.Format(@"{0}\{1}", ((IAccount) Thread.CurrentPrincipal.Identity).ID, Defines.ARCHIVE_NAME),
                        stream,
                        "application/zip",
                        "attachment; filename=\"" + Defines.ARCHIVE_NAME + "\"");

                    Log.DebugFormat("Zipped archive has been stored to {0}", path.ToString());
                }

                SetProgress((int?) MailOperationDownloadAllAttachmentsProgress.CreateLink);

                var baseDomain = CoreContext.Configuration.BaseDomain;

                var source = string.Format("{0}?{1}=bulk",
                    "/products/files/httphandlers/filehandler.ashx",
                    FilesLinkUtility.Action);

                if (damagedAttachments > 1)
                    Error = string.Format(MailCoreResource.FilesNotFound, damagedAttachments);

                SetProgress((int?)MailOperationDownloadAllAttachmentsProgress.Finished, null, source);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Mail operation error -> Download all attachments: {0}", ex.ToString());
                Error = string.IsNullOrEmpty(Error)
                    ? "InternalServerError"
                    : Error;
            }
        }

        private static void ZipFile(ZipOutputStream zip, string filename, Stream fileStream = null)
        {
            filename = filename ?? "file";

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

            zip.PutNextEntry(filename);

            if (fileStream != null)
            {
                fileStream.CopyTo(zip);
            }
        }
    }
}
