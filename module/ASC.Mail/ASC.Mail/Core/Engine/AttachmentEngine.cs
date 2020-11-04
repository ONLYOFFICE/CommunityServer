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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ASC.Common.Logging;
using ASC.Common.Web;
using ASC.Mail.Core.Dao.Expressions.Attachment;
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Search;
using ASC.Mail.Data.Storage;
using ASC.Mail.Enums;
using ASC.Mail.Exceptions;
using ASC.Mail.Extensions;
using ASC.Web.Core.Files;
using ASC.Web.Files.Api;
using ASC.Web.Files.Utils;
using File = ASC.Files.Core.File;

namespace ASC.Mail.Core.Engine
{
    public class AttachmentEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public AttachmentEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.AttachmentEngine");
        }

        public MailAttachmentData GetAttachment(IAttachmentExp exp)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoAttachment = daoFactory.CreateAttachmentDao(Tenant, User);

                var attachment = daoAttachment.GetAttachment(exp);

                return ToAttachmentData(attachment);
            }
        }

        public List<MailAttachmentData> GetAttachments(IAttachmentsExp exp)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoAttachment = daoFactory.CreateAttachmentDao(Tenant, User);

                var attachments = daoAttachment.GetAttachments(exp);

                return attachments.ConvertAll(ToAttachmentData);
            }
        }

        public long GetAttachmentsSize(IAttachmentsExp exp)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoAttachment = daoFactory.CreateAttachmentDao(Tenant, User);

                var size = daoAttachment.GetAttachmentsSize(exp);

                return size;
            }
        }

        public int GetAttachmentNextFileNumber(IAttachmentsExp exp)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoAttachment = daoFactory.CreateAttachmentDao(Tenant, User);

                var number = daoAttachment.GetAttachmentsMaxFileNumber(exp);

                number++;

                return number;
            }
        }

        public MailAttachmentData AttachFileFromDocuments(int tenant, string user, int messageId, string fileId, string version, bool needSaveToTemp = false)
        {
            MailAttachmentData result;

            using (var fileDao = FilesIntegration.GetFileDao())
            {
                var file = string.IsNullOrEmpty(version)
                               ? fileDao.GetFile(fileId)
                               : fileDao.GetFile(fileId, Convert.ToInt32(version));

                if (file == null)
                    throw new AttachmentsException(AttachmentsException.Types.DocumentNotFound, "File not found.");

                if (!FilesIntegration.GetFileSecurity().CanRead(file))
                    throw new AttachmentsException(AttachmentsException.Types.DocumentAccessDenied,
                                                   "Access denied.");

                if (!fileDao.IsExistOnStorage(file))
                {
                    throw new AttachmentsException(AttachmentsException.Types.DocumentNotFound,
                                                   "File not exists on storage.");
                }

                Log.InfoFormat("Original file id: {0}", file.ID);
                Log.InfoFormat("Original file name: {0}", file.Title);
                var fileExt = FileUtility.GetFileExtension(file.Title);
                var curFileType = FileUtility.GetFileTypeByFileName(file.Title);
                Log.InfoFormat("File converted type: {0}", file.ConvertedType);

                if (file.ConvertedType != null)
                {
                    switch (curFileType)
                    {
                        case FileType.Image:
                            fileExt = file.ConvertedType == ".zip" ? ".pptt" : file.ConvertedType;
                            break;
                        case FileType.Spreadsheet:
                            fileExt = file.ConvertedType != ".xlsx" ? ".xlst" : file.ConvertedType;
                            break;
                        default:
                            if (file.ConvertedType == ".doct" || file.ConvertedType == ".xlst" || file.ConvertedType == ".pptt")
                                fileExt = file.ConvertedType;
                            break;
                    }
                }

                var convertToExt = string.Empty;
                switch (curFileType)
                {
                    case FileType.Document:
                        if (fileExt == ".doct")
                            convertToExt = ".docx";
                        break;
                    case FileType.Spreadsheet:
                        if (fileExt == ".xlst")
                            convertToExt = ".xlsx";
                        break;
                    case FileType.Presentation:
                        if (fileExt == ".pptt")
                            convertToExt = ".pptx";
                        break;
                }

                if (!string.IsNullOrEmpty(convertToExt) && fileExt != convertToExt)
                {
                    var fileName = Path.ChangeExtension(file.Title, convertToExt);
                    Log.InfoFormat("Changed file name - {0} for file {1}:", fileName, file.ID);

                    using (var readStream = FileConverter.Exec(file, convertToExt))
                    {
                        if (readStream == null)
                            throw new AttachmentsException(AttachmentsException.Types.DocumentAccessDenied, "Access denied.");

                        using (var memStream = new MemoryStream())
                        {
                            readStream.StreamCopyTo(memStream);
                            result = AttachFileToDraft(tenant, user, messageId, fileName, memStream, memStream.Length, null, needSaveToTemp);
                            Log.InfoFormat("Attached attachment: ID - {0}, Name - {1}, StoredUrl - {2}", result.fileName, result.fileName, result.storedFileUrl);
                        }
                    }
                }
                else
                {
                    using (var readStream = fileDao.GetFileStream(file))
                    {
                        if (readStream == null)
                            throw new AttachmentsException(AttachmentsException.Types.DocumentAccessDenied, "Access denied.");

                        result = AttachFileToDraft(tenant, user, messageId, file.Title, readStream, readStream.CanSeek ? readStream.Length : file.ContentLength, null, needSaveToTemp);
                        Log.InfoFormat("Attached attachment: ID - {0}, Name - {1}, StoredUrl - {2}", result.fileName, result.fileName, result.storedFileUrl);
                    }
                }
            }

            return result;
        }

        public MailAttachmentData AttachFile(int tenant, string user, MailMessageData message,
            string name, Stream inputStream, long contentLength, string contentType = null, bool needSaveToTemp = false)
        {
            if (message == null)
                throw new AttachmentsException(AttachmentsException.Types.MessageNotFound, "Message not found.");

            if (string.IsNullOrEmpty(message.StreamId))
                throw new AttachmentsException(AttachmentsException.Types.MessageNotFound, "StreamId is empty.");

            var messageId = message.Id;

            var engine = new EngineFactory(tenant, user);

            var totalSize =
                engine.AttachmentEngine.GetAttachmentsSize(new ConcreteMessageAttachmentsExp(messageId, tenant, user));

            totalSize += contentLength;

            if (totalSize > Defines.ATTACHMENTS_TOTAL_SIZE_LIMIT)
                throw new AttachmentsException(AttachmentsException.Types.TotalSizeExceeded,
                    "Total size of all files exceeds limit!");

            var fileNumber =
                engine.AttachmentEngine.GetAttachmentNextFileNumber(new ConcreteMessageAttachmentsExp(messageId, tenant,
                    user));

            var attachment = new MailAttachmentData
            {
                fileName = name,
                contentType = string.IsNullOrEmpty(contentType) ? MimeMapping.GetMimeMapping(name) : contentType,
                needSaveToTemp = needSaveToTemp,
                fileNumber = fileNumber,
                size = contentLength,
                data = inputStream.ReadToEnd(),
                streamId = message.StreamId,
                tenant = tenant,
                user = user,
                mailboxId = message.MailboxId
            };

            engine.QuotaEngine.QuotaUsedAdd(contentLength);

            try
            {
                var storage = new StorageManager(tenant, user);
                storage.StoreAttachmentWithoutQuota(attachment);
            }
            catch
            {
                engine.QuotaEngine.QuotaUsedDelete(contentLength);
                throw;
            }

            if (!needSaveToTemp)
            {
                int attachCount;

                using (var daoFactory = new DaoFactory())
                {
                    var db = daoFactory.DbManager;

                    using (var tx = db.BeginTransaction())
                    {
                        var daoAttachment = daoFactory.CreateAttachmentDao(tenant, user);

                        attachment.fileId = daoAttachment.SaveAttachment(attachment.ToAttachmnet(messageId));

                        attachCount = daoAttachment.GetAttachmentsCount(
                            new ConcreteMessageAttachmentsExp(messageId, tenant, user));

                        var daoMailInfo = daoFactory.CreateMailInfoDao(tenant, user);

                        daoMailInfo.SetFieldValue(
                            SimpleMessagesExp.CreateBuilder(tenant, user)
                                .SetMessageId(messageId)
                                .Build(),
                            MailTable.Columns.AttachCount,
                            attachCount);

                        engine.ChainEngine.UpdateMessageChainAttachmentsFlag(daoFactory, tenant, user, messageId);

                        tx.Commit();
                    }
                }

                if (attachCount == 1)
                {
                    var data = new MailWrapper
                    {
                        HasAttachments = true
                    };

                    engine.IndexEngine.Update(data, s => s.Where(m => m.Id, messageId), wrapper => wrapper.HasAttachments);
                }
            }

            return attachment;
        }

        public MailAttachmentData AttachFileToDraft(int tenant, string user, int messageId,
            string name, Stream inputStream, long contentLength, string contentType = null, bool needSaveToTemp = false)
        {
            if (messageId < 1)
                throw new AttachmentsException(AttachmentsException.Types.BadParams, "Field 'id_message' must have non-negative value.");

            if (tenant < 0)
                throw new AttachmentsException(AttachmentsException.Types.BadParams, "Field 'id_tenant' must have non-negative value.");

            if (String.IsNullOrEmpty(user))
                throw new AttachmentsException(AttachmentsException.Types.BadParams, "Field 'id_user' is empty.");

            if (contentLength == 0)
                throw new AttachmentsException(AttachmentsException.Types.EmptyFile, "Empty files not supported.");

            var engine = new EngineFactory(tenant, user);

            var message = engine.MessageEngine.GetMessage(messageId, new MailMessageData.Options());

            if (message.Folder != FolderType.Draft && message.Folder != FolderType.Templates && message.Folder != FolderType.Sending)
                throw new AttachmentsException(AttachmentsException.Types.BadParams, "Message is not a draft or templates.");

            return AttachFile(tenant, user, message, name, inputStream, contentLength, contentType, needSaveToTemp);
        }

        public void StoreAttachmentCopy(int tenant, string user, MailAttachmentData attachment, string streamId)
        {
            try
            {
                if (attachment.streamId.Equals(streamId) && !attachment.isTemp) return;

                string s3Key;

                var dataClient = MailDataStore.GetDataStore(tenant);

                if (attachment.needSaveToTemp || attachment.isTemp)
                {
                    s3Key = MailStoragePathCombiner.GetTempStoredFilePath(attachment);
                }
                else
                {
                    s3Key = MailStoragePathCombiner.GerStoredFilePath(attachment);
                }

                if (!dataClient.IsFile(s3Key)) return;

                attachment.fileNumber =
                    !string.IsNullOrEmpty(attachment.contentId) //Upload hack: embedded attachment have to be saved in 0 folder
                        ? 0
                        : attachment.fileNumber;

                var newS3Key = MailStoragePathCombiner.GetFileKey(user, streamId, attachment.fileNumber,
                                                                    attachment.storedName);

                var copyS3Url = dataClient.Copy(s3Key, string.Empty, newS3Key);

                attachment.storedFileUrl = MailStoragePathCombiner.GetStoredUrl(copyS3Url);

                attachment.streamId = streamId;

                attachment.tempStoredUrl = null;

                Log.DebugFormat("StoreAttachmentCopy() tenant='{0}', user_id='{1}', stream_id='{2}', new_s3_key='{3}', copy_s3_url='{4}', storedFileUrl='{5}',  filename='{6}'",
                    tenant, user, streamId, newS3Key, copyS3Url, attachment.storedFileUrl, attachment.fileName);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("CopyAttachment(). filename='{0}', ctype='{1}' Exception:\r\n{2}\r\n",
                           attachment.fileName,
                           attachment.contentType,
                    ex.ToString());

                throw;
            }
        }

        public void DeleteMessageAttachments(int tenant, string user, int messageId, List<int> attachmentIds)
        {
            var engine = new EngineFactory(tenant, user);

            long usedQuota;
            int attachCount;

            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var exp = new ConcreteMessageAttachmentsExp(messageId, tenant, user, attachmentIds,
                        onlyEmbedded: null);

                    var daoAttachment = daoFactory.CreateAttachmentDao(tenant, user);

                    usedQuota = daoAttachment.GetAttachmentsSize(exp);

                    daoAttachment.SetAttachmnetsRemoved(exp);

                    attachCount = daoAttachment.GetAttachmentsCount(
                        new ConcreteMessageAttachmentsExp(messageId, tenant, user));

                    var daoMailInfo = daoFactory.CreateMailInfoDao(tenant, user);

                    daoMailInfo.SetFieldValue(
                        SimpleMessagesExp.CreateBuilder(tenant, user)
                            .SetMessageId(messageId)
                            .Build(),
                        MailTable.Columns.AttachCount,
                        attachCount);

                    engine.ChainEngine.UpdateMessageChainAttachmentsFlag(daoFactory, tenant, user, messageId);

                    tx.Commit();
                }
            }

            if (attachCount == 0)
            {
                var data = new MailWrapper
                {
                    HasAttachments = false
                };

                engine.IndexEngine.Update(data, s => s.Where(m => m.Id, messageId), wrapper => wrapper.HasAttachments);
            }

            if (usedQuota <= 0)
                return;

            engine.QuotaEngine.QuotaUsedDelete(usedQuota);
        }

        public void StoreAttachments(MailBoxData mailBoxData, List<MailAttachmentData> attachments, string streamId)
        {
            if (!attachments.Any() || string.IsNullOrEmpty(streamId)) return;

            try
            {
                var quotaAddSize = attachments.Sum(a => a.data != null ? a.data.LongLength : a.dataStream.Length);

                var storageManager = new StorageManager(mailBoxData.TenantId, mailBoxData.UserId);

                foreach (var attachment in attachments)
                {
                    var isAttachmentNameHasBadName = string.IsNullOrEmpty(attachment.fileName)
                                                         || attachment.fileName.IndexOfAny(Path.GetInvalidPathChars()) != -1
                                                         || attachment.fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1;
                    if (isAttachmentNameHasBadName)
                    {
                        attachment.fileName = string.Format("attacment{0}{1}", attachment.fileNumber,
                                                                               MimeMapping.GetExtention(attachment.contentType));
                    }

                    attachment.streamId = streamId;
                    attachment.tenant = mailBoxData.TenantId;
                    attachment.user = mailBoxData.UserId;

                    storageManager.StoreAttachmentWithoutQuota(attachment);
                }

                var engine = new EngineFactory(mailBoxData.TenantId);
                engine.QuotaEngine.QuotaUsedAdd(quotaAddSize);
            }
            catch
            {
                var storedAttachmentsKeys = attachments
                                            .Where(a => !string.IsNullOrEmpty(a.storedFileUrl))
                                            .Select(MailStoragePathCombiner.GerStoredFilePath)
                                            .ToList();

                if (storedAttachmentsKeys.Any())
                {
                    var storage = MailDataStore.GetDataStore(mailBoxData.TenantId);

                    storedAttachmentsKeys.ForEach(key => storage.Delete(string.Empty, key));
                }

                Log.InfoFormat("[Failed] StoreAttachments(mailboxId={0}). All message attachments were deleted.", mailBoxData.MailBoxId);

                throw;
            }
        }

        public static MailAttachmentData ToAttachmentData(Attachment attachment)
        {
            if (attachment == null) return null;

            var a = new MailAttachmentData
            {
                fileId = attachment.Id,
                fileName = attachment.Name,
                storedName = attachment.StoredName,
                contentType = attachment.Type,
                size = attachment.Size,
                fileNumber = attachment.FileNumber,
                streamId = attachment.Stream,
                tenant = attachment.Tenant,
                user = attachment.User,
                contentId = attachment.ContentId,
                mailboxId = attachment.MailboxId
            };

            return a;
        }
    }
}
