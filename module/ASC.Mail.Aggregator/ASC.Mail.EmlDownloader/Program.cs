/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

namespace ASC.Mail.EmlDownloader
{
    partial class Program
    {
        static readonly ILogger Logger = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Nlog, "EMLDownloader");

        private static void Main(string[] args)
        {
            var options = new Options();

            if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options,
                                                                () => Logger.Info("Bad command line parameters.")))
            {
                Imap4Client imap = null;
                Pop3Client pop = null;

                try
                {
                    Logger.Info("Searching account with id {0}", options.MailboxId);

                    var mailbox = GetMailBox(options.MailboxId);

                    if (mailbox == null)
                    {
                        Logger.Info("Account not found.");
                        ShowAnyKey();
                        return;
                    }

                    string message_eml;

                    if (mailbox.Imap)
                    {
                        Logger.Info("ConnectionType is IMAP4");

                        imap = MailClientBuilder.Imap();

                        imap.AuthenticateImap(mailbox, Logger);

                        Logger.Info(imap.ServerCapabilities);

                        var mailboxes_string = imap.GetImapMailboxes();

                        Logger.Info(mailboxes_string);

                        if (string.IsNullOrEmpty(options.MessageUid))
                        {
                            Logger.Info("MessageUid not setup.");
                            ShowAnyKey();
                            return;
                        }

                        var uidl_stucture = ParserImapUidl(options.MessageUid);

                        if (uidl_stucture.folder_id != 1)
                            throw new FormatException("Only inbox messages are supported for downloading.");

                        var mb = imap.SelectMailbox("INBOX");

                        var uid_list = mb.UidSearch("UID 1:*");

                        if (!uid_list.Any(uid => uid == uidl_stucture.uid))
                            throw new FileNotFoundException(string.Format("Message with uid {0} not found in inbox",
                                                                          uidl_stucture.uid));

                        Logger.Info("Try Fetch.UidMessageStringPeek");

                        message_eml = mb.Fetch.UidMessageStringPeek(uidl_stucture.uid);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(options.MessageUid))
                        {
                            Logger.Info("MessageUid not setup.");
                            ShowAnyKey();
                            return;
                        }

                        Logger.Info("ConnectionType is POP3");

                        pop = MailClientBuilder.Pop();

                        pop.Authorize(new MailServerSettings
                        {
                            AccountName = mailbox.Account,
                            AccountPass = mailbox.Password,
                            AuthenticationType = mailbox.AuthenticationTypeIn,
                            EncryptionType = mailbox.IncomingEncryptionType,
                            Port = mailbox.Port,
                            Url = mailbox.Server
                        }, mailbox.AuthorizeTimeoutInMilliseconds, Logger);

                        Logger.Debug("UpdateStats()");

                        pop.UpdateStats();

                        var index = pop.GetMessageIndex(options.MessageUid);

                        if (index < 1)
                            throw new FileNotFoundException(string.Format("Message with uid {0} not found in inbox",
                                                                          options.MessageUid));

                        message_eml = pop.RetrieveMessageString(index);
                    }

                    Logger.Info("Try StoreToFile");
                    var now = DateTime.Now;
                    var path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads"),
                                            string.Format("uid_{0}_{1}.eml", options.MessageUid,
                                                          now.ToString("dd_MM_yyyy_hh_mm")));
                    var path_file = StoreToFile(message_eml, path, true);

                    Logger.Info("[SUCCESS] File was stored into path \"{0}\"", path_file);

                }
                catch (Exception ex)
                {
                    Logger.Info(ex.ToString());
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

        static string StoreToFile(string eml_string, string file_name, bool use_temp)
        {
            var temp_path = use_temp ? Path.GetTempFileName() : file_name;
            var sw = File.CreateText(temp_path);
            sw.Write(eml_string);
            sw.Close();

            if (use_temp)
            {
                if (!string.IsNullOrEmpty(file_name))
                {
                    var directory = Path.GetDirectoryName(file_name);
                    // Create the traget path iof it does not exist
                    if (directory != null && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    if (string.IsNullOrEmpty(Path.GetFileName(file_name)))
                    {
                        string save_path = Path.Combine(file_name, Path.GetFileNameWithoutExtension(temp_path) + ".eml");
                        File.Move(temp_path, save_path);
                        File.Delete(temp_path);
                        return save_path;
                    }

                    File.Move(temp_path, file_name);
                    File.Delete(temp_path);

                    return file_name;
                }

                return temp_path;
            }
            return temp_path;
        } 

        private struct ImapUidl
        {
            public int folder_id;
            public int uid;
        }

        private static ImapUidl ParserImapUidl(string uidl)
        {
            var elements = uidl.Split('-');

            return new ImapUidl
                {
                    uid = Convert.ToInt32(elements[0]),
                    folder_id = Convert.ToInt32(elements[1])
                };
        }

        private static MailBox GetMailBox(int mailbox_id)
        {
            var manager = new MailBoxManager(25);
            return manager.GetMailBox(mailbox_id);
        }
    }
}
