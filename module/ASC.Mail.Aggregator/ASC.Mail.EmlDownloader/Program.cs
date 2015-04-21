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
using System.IO;
using System.Linq;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ActiveUp.Net.Mail;
using log4net.Config;

namespace ASC.Mail.EmlDownloader
{
    partial class Program
    {
        static ILogger _logger;

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            _logger = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "EMLDownloader");

            var options = new Options();

            if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options,
                                                                () => _logger.Info("Bad command line parameters.")))
            {
                Imap4Client imap = null;
                Pop3Client pop = null;

                try
                {
                    _logger.Info("Searching account with id {0}", options.MailboxId);

                    var mailbox = GetMailBox(options.MailboxId);

                    if (mailbox == null)
                    {
                        _logger.Info("Account not found.");
                        ShowAnyKey();
                        return;
                    }

                    string messageEml;

                    if (mailbox.Imap)
                    {
                        _logger.Info("ConnectionType is IMAP4");

                        imap = MailClientBuilder.Imap();

                        imap.AuthenticateImap(mailbox, _logger);

                        _logger.Info(imap.ServerCapabilities);

                        var mailboxesString = imap.GetImapMailboxes();

                        _logger.Info(mailboxesString);

                        if (string.IsNullOrEmpty(options.MessageUid))
                        {
                            _logger.Info("MessageUid not setup.");
                            ShowAnyKey();
                            return;
                        }

                        var uidlStucture = ParserImapUidl(options.MessageUid);

                        if (uidlStucture.folderId != 1)
                            throw new FormatException("Only inbox messages are supported for downloading.");

                        var mb = imap.SelectMailbox("INBOX");

                        var uidList = mb.UidSearch("UID 1:*");

                        if (!uidList.Any(uid => uid == uidlStucture.uid))
                            throw new FileNotFoundException(string.Format("Message with uid {0} not found in inbox",
                                                                          uidlStucture.uid));

                        _logger.Info("Try Fetch.UidMessageStringPeek");

                        messageEml = mb.Fetch.UidMessageStringPeek(uidlStucture.uid);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(options.MessageUid))
                        {
                            _logger.Info("MessageUid not setup.");
                            ShowAnyKey();
                            return;
                        }

                        _logger.Info("ConnectionType is POP3");

                        pop = MailClientBuilder.Pop();

                        pop.Authorize(new MailServerSettings
                        {
                            AccountName = mailbox.Account,
                            AccountPass = mailbox.Password,
                            AuthenticationType = mailbox.AuthenticationTypeIn,
                            EncryptionType = mailbox.IncomingEncryptionType,
                            Port = mailbox.Port,
                            Url = mailbox.Server
                        }, mailbox.AuthorizeTimeoutInMilliseconds, _logger);

                        _logger.Debug("UpdateStats()");

                        pop.UpdateStats();

                        var index = pop.GetMessageIndex(options.MessageUid);

                        if (index < 1)
                            throw new FileNotFoundException(string.Format("Message with uid {0} not found in inbox",
                                                                          options.MessageUid));

                        messageEml = pop.RetrieveMessageString(index);
                    }

                    _logger.Info("Try StoreToFile");
                    var now = DateTime.Now;
                    var path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads"),
                                            string.Format("uid_{0}_{1}.eml", options.MessageUid,
                                                          now.ToString("dd_MM_yyyy_hh_mm")));
                    var pathFile = StoreToFile(messageEml, path, true);

                    _logger.Info("[SUCCESS] File was stored into path \"{0}\"", pathFile);

                }
                catch (Exception ex)
                {
                    _logger.Info(ex.ToString());
                }
                finally
                {
                    if (imap != null && imap.IsConnected)
                    {
                        imap.Disconnect();
                    }
                    else if (pop != null && pop.IsConnected)
                    {
                        pop.Disconnect();
                    }
                }
            }

            ShowAnyKey();
        }

        private static void ShowAnyKey()
        {
            Console.WriteLine();
            Console.WriteLine("Any key to exit...");
            Console.ReadKey();
        }

        static string StoreToFile(string emlString, string fileName, bool useTemp)
        {
            var tempPath = useTemp ? Path.GetTempFileName() : fileName;
            var sw = File.CreateText(tempPath);
            sw.Write(emlString);
            sw.Close();

            if (useTemp)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    var directory = Path.GetDirectoryName(fileName);
                    // Create the traget path iof it does not exist
                    if (directory != null && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    if (string.IsNullOrEmpty(Path.GetFileName(fileName)))
                    {
                        string savePath = Path.Combine(fileName, Path.GetFileNameWithoutExtension(tempPath) + ".eml");
                        File.Move(tempPath, savePath);
                        File.Delete(tempPath);
                        return savePath;
                    }

                    File.Move(tempPath, fileName);
                    File.Delete(tempPath);

                    return fileName;
                }

                return tempPath;
            }
            return tempPath;
        } 

        private struct ImapUidl
        {
            public int folderId;
            public int uid;
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

        private static MailBox GetMailBox(int mailboxId)
        {
            var manager = new MailBoxManager();
            return manager.GetMailBox(mailboxId);
        }
    }
}
