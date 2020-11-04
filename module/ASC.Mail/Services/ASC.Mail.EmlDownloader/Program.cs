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
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;

using ASC.Mail.Clients;
using ASC.Mail.Core;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;

namespace ASC.Mail.EmlDownloader
{
    internal partial class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();

            if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options,
                                                                () => Console.WriteLine(@"Bad command line parameters.")))
            {
                try
                {
                    Console.WriteLine(@"Searching account with id {0}", options.MailboxId);

                    if (string.IsNullOrEmpty(options.MessageUid))
                    {
                        Console.WriteLine(@"MessageUid not setup.");
                        ShowAnyKey();
                        return;
                    }

                    var mailbox = GetMailBox(options.MailboxId);

                    if (mailbox == null)
                    {
                        Console.WriteLine(@"Account not found.");
                        ShowAnyKey();
                        return;
                    }

                    if (mailbox.Imap)
                    {
                        var uidlStucture = ParserImapUidl(options.MessageUid);
                        if (uidlStucture.folderId != FolderType.Inbox)
                            throw new FormatException("Only inbox messages are supported for downloading.");

                    }

                    var certificatePermit = ConfigurationManagerExtension.AppSettings["mail.certificate-permit"] != null &&
                                            Convert.ToBoolean(
                                                ConfigurationManagerExtension.AppSettings["mail.certificate-permit"]);

                    string messageEml;
                    using (var client = new MailClient(mailbox, CancellationToken.None, certificatePermit: mailbox.IsTeamlab || certificatePermit))
                    {
                        var message = client.GetInboxMessage(options.MessageUid);
                        messageEml = message.ToString();
                    }

                    Console.WriteLine(@"Try StoreToFile");

                    var now = DateTime.UtcNow;

                    var path = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Downloads"),
                                       string.Format("uid_{0}_{1}.eml", options.MessageUid,
                                                     now.ToString("dd_MM_yyyy_hh_mm")));


                    var pathFile = StoreToFile(messageEml, path, true);

                    Console.WriteLine(@"[SUCCESS] File was stored into path ""{0}""", pathFile);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            ShowAnyKey();
        }

        private static void ShowAnyKey()
        {
            Console.WriteLine();
            Console.WriteLine(@"Any key to exit...");
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
            public FolderType folderId;
            public int uid;
        }

        private static ImapUidl ParserImapUidl(string uidl)
        {
            var elements = uidl.Split('-');

            return new ImapUidl
            {
                uid = Convert.ToInt32(elements[0]),
                folderId = (FolderType)Convert.ToInt32(elements[1])
            };
        }

        private static MailBoxData GetMailBox(int mailboxId)
        {
            var engine = new EngineFactory(-1);
            return engine.MailboxEngine.GetMailboxData(new ConcreteSimpleMailboxExp(mailboxId));
        }
    }
}
