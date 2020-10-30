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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using ASC.Common.Logging;
using ASC.Mail.Clients;
using ASC.Mail.Core;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Engine;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Mail.Utils;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ASC.Mail.Reloader
{
    internal partial class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();

            if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options,
                () => Console.WriteLine(@"Bad command line parameters.")))
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
                                Console.WriteLine(@"[ERROR] json tasks not found. Array is empty.");

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
                        Console.WriteLine(@"[ERROR] MailboxId invalid.");

                        ShowAnyKey();
                        return;
                    }

                    if (options.MessageId < 0)
                    {
                        Console.WriteLine(@"[ERROR] MailboxId invalid.");

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

                        Console.WriteLine(@"Searching account with id {0}", mailboxId);

                        var mailbox = GetMailBox(mailboxId);

                        if (mailbox == null)
                        {
                            Console.WriteLine(@"[ERROR] Account not found.");
                            ShowAnyKey();
                            return;
                        }

                        var messageIds = reloadItem.Value;

                        foreach (var messageId in messageIds)
                        {
                            Console.WriteLine(@"Searching message with id {0}", messageId);

                            if (messageId < 0)
                            {
                                Console.WriteLine(@"[ERROR] MessageId not setup.");
                                continue;
                            }

                            var storedMessage = GetMail(mailbox, messageId);

                            if (storedMessage == null)
                            {
                                Console.WriteLine(@"[ERROR] Message not found.");
                                continue;
                            }

                            var messageUid = storedMessage.Uidl;

                            if (mailbox.Imap)
                            {
                                var uidlStucture = ParserImapUidl(messageUid);
                                if (uidlStucture.folderId != FolderType.Inbox)
                                {
                                    Console.WriteLine(@"Only inbox messages are supported for downloading.");
                                    continue;
                                }
                            }

                            var certificatePermit = ConfigurationManager.AppSettings["mail.certificate-permit"] !=
                                                    null &&
                                                    Convert.ToBoolean(
                                                        ConfigurationManager.AppSettings["mail.certificate-permit"]);

                            var log = new NullLog();

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
                                Console.WriteLine(@"[ERROR] Failed GetInboxMessage(""{0}"") Exception: {1}", messageUid, e);
                                continue;
                            }

                            MailMessageData message;

                            try
                            {
                                message = mimeMessage.ConvertToMailMessage(new MailFolder(FolderType.Inbox, ""),
                                    storedMessage.IsNew, storedMessage.ChainId, storedMessage.ChainDate,
                                    storedMessage.StreamId, mailboxId, false, log);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(@"[ERROR] Failed ConvertToMailMessage(""{0}"") Exception: {1}",
                                    messageUid, ex);
                                continue;
                            }

                            if (!storedMessage.From.Equals(MailUtil.NormalizeStringForMySql(message.From)) ||
                                !storedMessage.To.Equals(MailUtil.NormalizeStringForMySql(message.To)) ||
                                !storedMessage.Subject.Equals(MailUtil.NormalizeStringForMySql(message.Subject)))
                            {
                                Console.WriteLine(@"storedMessage.From = '{0}'", storedMessage.From);
                                Console.WriteLine(@"message.From = '{0}'",
                                    MailUtil.NormalizeStringForMySql(message.From));

                                Console.WriteLine(@"storedMessage.To = '{0}'", storedMessage.To);
                                Console.WriteLine(@"message.To = '{0}'",
                                    MailUtil.NormalizeStringForMySql(message.To));

                                Console.WriteLine(@"storedMessage.Subject = '{0}'", storedMessage.Subject);
                                Console.WriteLine(@"message.Subject = '{0}'",
                                    MailUtil.NormalizeStringForMySql(message.Subject));

                                Console.WriteLine(@"[ERROR] Stored message not equals to server message");
                                continue;
                            }

                            if (storedMessage.Attachments.Any() && message.Attachments.Any())
                            {
                                var newAttachments = new List<MailAttachmentData>();

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

                            if (!MessageEngine.TryStoreMailData(message, mailbox, log))
                            {
                                Console.WriteLine(@"[ERROR] Failed to store mail data");
                                continue;
                            }

                            Console.WriteLine(@"[SUCCESS] Mail ""{0}"" data was reloaded", messageId);
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
            Console.WriteLine(@"Any key to exit...");
            Console.ReadKey();
        }

        private struct ImapUidl
        {
            public FolderType folderId;
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
                folderId = (FolderType) Convert.ToInt32(elements[1])
            };
        }

        private static MailMessageData GetMail(MailBoxData mailbox, int mailId)
        {
            var engine = new EngineFactory(mailbox.TenantId, mailbox.UserId);

            return engine.MessageEngine.GetMessage(mailId, new MailMessageData.Options
            {
                LoadBody = false,
                LoadImages = false,
                NeedProxyHttp = false,
                NeedSanitizer = false,
                OnlyUnremoved = true,
                LoadEmebbedAttachements = true
            });
        }

        private static MailBoxData GetMailBox(int mailboxId)
        {
            var engine = new EngineFactory(-1);
            return engine.MailboxEngine.GetMailboxData(new ConcreteSimpleMailboxExp(mailboxId));
        }
    }
}
