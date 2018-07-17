/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.Core.Clients;
using log4net.Config;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ASC.Mail.Reloader
{
    internal partial class Program
    {
        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var options = new Options();

            if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options,
                () => Console.WriteLine("Bad command line parameters.")))
            {
                ShowAnyKey();
                return;
            }

            try
            {
                var reloadList = new Dictionary<int, List<int>>();

                if (!string.IsNullOrEmpty(options.PathToJson) && File.Exists(options.PathToJson))
                {
                    using (var streamReader = new StreamReader(options.PathToJson))
                    {
                        using (var reader = new JsonTextReader(streamReader))
                        {
                            var jObject = JObject.Load(reader);

                            if (jObject["data"] == null || !jObject["data"].Any())
                            {
                                Console.WriteLine("[ERROR] json tasks not found. Array is empty.");

                                ShowAnyKey();
                                return;
                            }

                            var results = jObject["data"].ToList().GroupBy(
                                p => Convert.ToInt32(p["id_mailbox"]),
                                p => Convert.ToInt32(p["id"]),
                                (key, g) => new {MailboxId = key, Ids = g.ToList()});

                            foreach (var result in results)
                            {
                                if(reloadList.ContainsKey(result.MailboxId))
                                    continue;

                                reloadList.Add(result.MailboxId, result.Ids);
                            }
                        }
                    }
                }
                else
                {
                    if (options.MailboxId < 0)
                    {
                        Console.WriteLine("[ERROR] MailboxId invalid.");

                        ShowAnyKey();
                        return;
                    }

                    if (options.MessageId < 0)
                    {
                        Console.WriteLine("[ERROR] MailboxId invalid.");

                        ShowAnyKey();
                        return;
                    }


                    reloadList.Add(options.MailboxId, new List<int> {options.MessageId});
                }

                foreach (var reloadItem in reloadList)
                {
                    MailClient client = null;

                    try
                    {
                        var mailboxId = reloadItem.Key;

                        Console.WriteLine("\r\nSearching account with id {0}", mailboxId);

                        var mailbox = GetMailBox(mailboxId);

                        if (mailbox == null)
                        {
                            Console.WriteLine("[ERROR] Account not found.");
                            ShowAnyKey();
                            return;
                        }

                        var messageIds = reloadItem.Value;

                        foreach (var messageId in messageIds)
                        {
                            Console.WriteLine("\r\nSearching message with id {0}", messageId);

                            if (messageId < 0)
                            {
                                Console.WriteLine("[ERROR] MessageId not setup.");
                                continue;
                            }

                            var storedMessage = GetMail(mailbox, messageId);

                            if (storedMessage == null)
                            {
                                Console.WriteLine("[ERROR] Message not found.");
                                continue;
                            }

                            var messageUid = storedMessage.Uidl;

                            if (mailbox.Imap)
                            {
                                var uidlStucture = ParserImapUidl(messageUid);
                                if (uidlStucture.folderId != MailFolder.Ids.inbox)
                                {
                                    Console.WriteLine("Only inbox messages are supported for downloading.");
                                    continue;
                                }
                            }

                            var certificatePermit = ConfigurationManager.AppSettings["mail.certificate-permit"] !=
                                                    null &&
                                                    Convert.ToBoolean(
                                                        ConfigurationManager.AppSettings["mail.certificate-permit"]);

                            var log = new NullLogger();

                            if (client == null)
                                client = new MailClient(mailbox, CancellationToken.None,
                                    certificatePermit: certificatePermit);

                            MimeMessage mimeMessage;

                            try
                            {
                                mimeMessage = client.GetInboxMessage(messageUid);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("[ERROR] Failed GetInboxMessage(\"{0}\") Exception: {1}", messageUid, e);
                                continue;
                            }

                            var message = ConvertToMailMessage(mimeMessage,
                                new MailFolder(MailFolder.Ids.inbox, ""),
                                storedMessage.IsNew, storedMessage.ChainId, storedMessage.StreamId, log);

                            if (message == null)
                            {
                                Console.WriteLine("[ERROR] Failed ConvertToMailMessage(\"{0}\")", messageUid);
                                continue;
                            }

                            if (!storedMessage.From.Equals(MailUtil.NormalizeStringForMySql(message.From)) ||
                                !storedMessage.To.Equals(MailUtil.NormalizeStringForMySql(message.To)) ||
                                !storedMessage.Subject.Equals(MailUtil.NormalizeStringForMySql(message.Subject)))
                            {
                                Console.WriteLine("storedMessage.From = '{0}'", storedMessage.From);
                                Console.WriteLine("message.From = '{0}'",
                                    MailUtil.NormalizeStringForMySql(message.From));

                                Console.WriteLine("storedMessage.To = '{0}'", storedMessage.To);
                                Console.WriteLine("message.To = '{0}'",
                                    MailUtil.NormalizeStringForMySql(message.To));

                                Console.WriteLine("storedMessage.Subject = '{0}'", storedMessage.Subject);
                                Console.WriteLine("message.Subject = '{0}'",
                                    MailUtil.NormalizeStringForMySql(message.Subject));

                                Console.WriteLine("[ERROR] Stored message not equals to server message");
                                continue;
                            }

                            if (storedMessage.Attachments.Any() && message.Attachments.Any())
                            {
                                var newAttachments = new List<MailAttachment>();

                                foreach (var attachment in message.Attachments)
                                {
                                    var storedAttachment =
                                        storedMessage.Attachments.FirstOrDefault(
                                            a =>
                                                (!string.IsNullOrEmpty(a.fileName) &&
                                                 a.fileName.Equals(attachment.fileName,
                                                     StringComparison.InvariantCultureIgnoreCase)) ||
                                                !string.IsNullOrEmpty(a.contentId) &&
                                                a.contentId.Equals(attachment.contentId,
                                                    StringComparison.InvariantCultureIgnoreCase) ||
                                                !string.IsNullOrEmpty(a.contentLocation) &&
                                                a.contentLocation.Equals(attachment.contentLocation,
                                                    StringComparison.InvariantCultureIgnoreCase));

                                    if (storedAttachment == null)
                                        continue;

                                    attachment.fileName = storedAttachment.fileName;
                                    attachment.storedName = storedAttachment.storedName;
                                    attachment.fileNumber = storedAttachment.fileNumber;
                                    attachment.streamId = storedAttachment.streamId;
                                    attachment.mailboxId = storedAttachment.mailboxId;
                                    attachment.tenant = storedAttachment.tenant;
                                    attachment.user = storedAttachment.user;

                                    newAttachments.Add(attachment);
                                }

                                message.Attachments = newAttachments;
                            }

                            if (!TryStoreMailData(message, mailbox, log))
                            {
                                Console.WriteLine("[ERROR] Failed to store mail data");
                                continue;
                            }

                            Console.WriteLine("[SUCCESS] Mail \"{0}\" data was reloaded", messageId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        if (client != null)
                        {
                            client.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            ShowAnyKey();
        }

        private static void ShowAnyKey()
        {
            Console.WriteLine();
            Console.WriteLine("Any key to exit...");
            Console.ReadKey();
        }

        private static bool TryStoreMailData(MailMessage message, MailBox mailbox, ILogger log)
        {
            var manager = new MailBoxManager(log);

            try
            {
                if (message.Attachments.Any())
                {
                    Console.WriteLine("StoreAttachments()");

                    manager.StoreAttachments(mailbox, message.Attachments, message.StreamId);

                    Console.WriteLine("MailMessage.ReplaceEmbeddedImages()");
                    message.ReplaceEmbeddedImages(log);
                }

                Console.WriteLine("StoreMailBody()");
                manager.StoreMailBody(mailbox, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("TryStoreMailData(Account:{0}): Exception:\r\n{1}\r\n", mailbox.EMail, ex);

                /*//Trying to delete all attachments and mailbody
                if (manager.TryRemoveMailDirectory(mailbox, message.StreamId))
                {
                    Console.WriteLine("Problem with mail proccessing(Account:{0}). Body and attachment have been deleted", mailbox.EMail);
                }*/

                return false;
            }

            return true;
        }

        private static MailMessage ConvertToMailMessage(MimeMessage mimeMessage, MailFolder folder, bool unread, string chainId, string streamId, ILogger log)
        {
            try
            {
                var message = mimeMessage.CreateMailMessage(folder.FolderId, unread, chainId, streamId, log);

                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Convert MimeMessage->MailMessage: Exception: {0}", ex);               
            }

            return null;
        }

        private struct ImapUidl
        {
            public int folderId;
#pragma warning disable 414
            public int uid;
#pragma warning restore 414
        }

        private static ImapUidl ParserImapUidl(string uidl)
        {
            var elements = uidl.Split('-');

            return new ImapUidl
                {
                    uid = Convert.ToInt32(elements[0]),
                    folderId = Convert.ToInt32(elements[1])
                };
        }

        private static MailMessage GetMail(MailBox mailbox, int mailId)
        {
            var manager = new MailBoxManager(new NullLogger());
            return manager.GetMailInfo(mailbox.TenantId, mailbox.UserId, mailId, new MailMessage.Options
            {
                LoadBody = false,
                LoadImages = false,
                NeedProxyHttp = false,
                NeedSanitizer = false,
                OnlyUnremoved = true,
                LoadEmebbedAttachements = true
            });
        }

        private static MailBox GetMailBox(int mailboxId)
        {
            var manager = new MailBoxManager(new NullLogger());
            return manager.GetMailBox(mailboxId);
        }
    }
}
