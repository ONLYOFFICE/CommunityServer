﻿/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using ASC.Common;
using ASC.Common.Logging;
using ASC.Common.Security.Authentication;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Mail.Core.Dao.Expressions.Attachment;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Storage;
using ASC.Mail.Resources;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.PublicResources;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

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
                SetProgress((int?)MailOperationDownloadAllAttachmentsProgress.Init);

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

                SetProgress((int?)MailOperationDownloadAllAttachmentsProgress.GetAttachments);

                var attachments =
                    engine.AttachmentEngine.GetAttachments(new ConcreteMessageAttachmentsExp(MessageId,
                        CurrentTenant.TenantId, CurrentUser.ID.ToString()));

                if (!attachments.Any())
                {
                    Error = MailCoreResource.NoAttachmentsInMessage;

                    throw new Exception(Error);
                }

                SetProgress((int?)MailOperationDownloadAllAttachmentsProgress.Zipping);

                var damagedAttachments = 0;

                using (var stream = TempStream.Create())
                {
                    using (var gzoStream = new GZipOutputStream(stream))
                    using (var gzip = new TarOutputStream(gzoStream, Encoding.UTF8))
                    {
                        gzoStream.IsStreamOwner = false;

                        var attachmentsCount = attachments.Count;
                        var progressMaxValue = (int)MailOperationDownloadAllAttachmentsProgress.ArchivePreparation;
                        var progressMinValue = (int)MailOperationDownloadAllAttachmentsProgress.Zipping;
                        var progresslength = progressMaxValue - progressMinValue;
                        var progressStep = (double)progresslength / attachmentsCount;
                        var zippingProgress = 0.0;

                        var alreadyZipped = new List<string>();

                        foreach (var attachment in attachments)
                        {
                            var filename = attachment.fileName;

                            if (alreadyZipped.Contains(filename))
                            {
                                var counter = 1;
                                var tempName = filename;
                                while (alreadyZipped.Contains(tempName))
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

                            alreadyZipped.Add(filename);

                            try
                            {
                                using (var file = attachment.ToAttachmentStream())
                                {
                                    ZipFile(gzip, filename, file.FileStream);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);

                                Error = string.Format(MailCoreResource.FileNotFoundOrDamaged, attachment.fileName);

                                damagedAttachments++;
                                using (var emptyStream = new MemoryStream())
                                {
                                    ZipFile(gzip, filename, emptyStream); // Zip empty file
                                }
                            }

                            zippingProgress += progressStep;

                            SetProgress(progressMinValue + (int?)zippingProgress);
                        }
                    }

                    SetProgress((int?)MailOperationDownloadAllAttachmentsProgress.ArchivePreparation);

                    if (stream.Length == 0)
                    {
                        Error = "File stream is empty";

                        throw new Exception(Error);
                    }

                    stream.Position = 0;

                    var store = Global.GetStore();
                    var path = string.Format(@"{0}\{1}", ((IAccount)Thread.CurrentPrincipal.Identity).ID, Defines.ARCHIVE_NAME);
                    var storedPath = store.Save(
                        FileConstant.StorageDomainTmp,
                        path,
                        stream,
                        MimeMapping.GetMimeMapping(path),
                        "attachment; filename=\"" + Defines.ARCHIVE_NAME + "\"");

                    Log.DebugFormat("Zipped archive has been stored to {0}", storedPath.ToString());
                }

                SetProgress((int?)MailOperationDownloadAllAttachmentsProgress.CreateLink);

                var baseDomain = CoreContext.Configuration.BaseDomain;

                var source = string.Format("{0}?{1}=bulk&{2}",
                    "/Products/Files/HttpHandlers/filehandler.ashx",
                    FilesLinkUtility.Action, "ext=.tar.gz");

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

        private static void ZipFile(TarOutputStream zip, string filename, Stream fileStream = null)
        {
            filename = filename ?? "file";
            var entry = TarEntry.CreateTarEntry(filename);
            entry.Size = fileStream.Length;
            zip.PutNextEntry(entry);
            fileStream.CopyTo(zip);
            zip.CloseEntry();
        }
    }
}
