using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;
using MimeKit;
using MySql.Data.MySqlClient;

namespace ASC.Mail.Aggregator.Core
{
    public class MailRepository
    {
        public static MailMessage Save(MailBox mailbox, MimeMessage mimeMessage, string uid, MailFolder folder, bool unread = true, ILogger log = null)
        {
            if(mailbox == null)
                throw new ArgumentException("mailbox is null", "mailbox");

            if (mimeMessage == null)
                throw new ArgumentException("message is null", "mimeMessage");

            if (uid == null)
                throw new ArgumentException("uidl is null", "uid");

            if(log == null)
                log = new NullLogger();

            var manager = new MailBoxManager(log);

            var fromEmail = mimeMessage.From.Mailboxes.FirstOrDefault();

            var md5 =
                    string.Format("{0}|{1}|{2}|{3}",
                        mimeMessage.From.Mailboxes.Any() ? mimeMessage.From.Mailboxes.First().Address : "",
                        mimeMessage.Subject, mimeMessage.Date.UtcDateTime, mimeMessage.MessageId).GetMd5();

            var uidl = mailbox.Imap ? string.Format("{0}-{1}", uid, folder.FolderId) : uid;

            var fromThisMailBox = fromEmail != null &&
                                  fromEmail.Address.ToLowerInvariant()
                                      .Equals(mailbox.EMail.Address.ToLowerInvariant());

            var toThisMailBox =
                mimeMessage.To.Mailboxes.Select(addr => addr.Address.ToLowerInvariant())
                    .Contains(mailbox.EMail.Address.ToLowerInvariant());

            List<int> tagsIds = null;

            if (folder.Tags.Any())
            {
                log.Debug("GetOrCreateTags()");

                tagsIds = manager.GetOrCreateTags(mailbox.TenantId, mailbox.UserId, folder.Tags);
            }

            log.Debug("SearchExistingMessagesAndUpdateIfNeeded()");

            var found = manager.SearchExistingMessagesAndUpdateIfNeeded(mailbox, folder.FolderId, uidl, md5,
                mimeMessage.MessageId, fromThisMailBox, toThisMailBox, tagsIds);

            var needSave = !found;
            if (!needSave)
                return null;

            log.Debug("DetectChainId()");

            var chainId = manager.DetectChainId(mailbox, mimeMessage.MessageId, mimeMessage.InReplyTo,
                mimeMessage.Subject);

            var streamId = MailUtil.CreateStreamId();

            log.Debug("Convert MimeMessage->MailMessage");

            var message = ConvertToMailMessage(mimeMessage, folder, unread, chainId, streamId, log);

            log.Debug("TryStoreMailData()");

            if (!TryStoreMailData(message, mailbox, log))
            {
                return null;
            }

            log.Debug("MailSave()");

            if (TrySaveMail(manager, mailbox, message, folder, uidl, md5, log))
            {
                return message;
            }

            if (manager.TryRemoveMailDirectory(mailbox, message.StreamId))
            {
                log.Info("Problem with mail proccessing(Account:{0}). Body and attachment have been deleted", mailbox.EMail);
            }
            else
            {
                throw new Exception("Can't delete mail folder with data");
            }

            return null;
        }

        private static bool TrySaveMail(MailBoxManager manager, MailBox mailbox, MailMessage message, MailFolder folder, string uidl, string md5, ILogger log)
        {
            try
            {
                var folderRestoreId = folder.FolderId == MailFolder.Ids.spam ? MailFolder.Ids.inbox : folder.FolderId;

                var attempt = 1;

                while (attempt < 3)
                {
                    try
                    {
                        message.Id = manager.MailSave(mailbox, message, 0, folder.FolderId, folderRestoreId,
                            uidl, md5, true);

                        break;
                    }
                    catch (MySqlException exSql)
                    {
                        if (!exSql.Message.StartsWith("Deadlock found"))
                            throw;

                        if (attempt > 2)
                            throw;

                        log.Warn("[DEADLOCK] MailSave() try again (attempt {0}/2)", attempt);

                        attempt++;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("TrySaveMail Exception:\r\n{0}\r\n", ex.ToString());
            }

            return false;
        }

        private static MailMessage ConvertToMailMessage(MimeMessage mimeMessage, MailFolder folder, bool unread, string chainId, string streamId, ILogger log)
        {
            MailMessage message;

            try
            {
                message = mimeMessage.CreateMailMessage(folder.FolderId, unread, chainId, streamId, log);
            }
            catch (Exception ex)
            {
                log.Error("Convert MimeMessage->MailMessage: Exception: {0}", ex.ToString());

                log.Debug("Creating fake message with original MimeMessage in attachments");

                message = mimeMessage.CreateCorruptedMesage(folder.FolderId, unread, chainId, streamId);
            }

            return message;
        }

        private static bool TryStoreMailData(MailMessage message, MailBox mailbox, ILogger log)
        {
            var manager = new MailBoxManager(log);

            try
            {
                if (message.Attachments.Any())
                {
                    log.Debug("StoreAttachments()");
                    var index = 0;
                    message.Attachments.ForEach(att =>
                    {
                        att.fileNumber = ++index;
                        att.mailboxId = mailbox.MailBoxId;
                    });
                    manager.StoreAttachments(mailbox, message.Attachments, message.StreamId);

                    log.Debug("MailMessage.ReplaceEmbeddedImages()");
                    message.ReplaceEmbeddedImages(log);
                }

                log.Debug("StoreMailBody()");
                manager.StoreMailBody(mailbox, message);
            }
            catch (Exception ex)
            {
                log.Error("TryStoreMailData(Account:{0}): Exception:\r\n{1}\r\n", mailbox.EMail, ex.ToString());

                //Trying to delete all attachments and mailbody
                if (manager.TryRemoveMailDirectory(mailbox, message.StreamId))
                {
                    log.Info("Problem with mail proccessing(Account:{0}). Body and attachment have been deleted", mailbox.EMail);
                }

                return false;
            }

            return true;
        }
    }
}
