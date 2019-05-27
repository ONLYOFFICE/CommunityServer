/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Configuration;
using System.IO;
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
                        if(uidlStucture.folderId != FolderType.Inbox)
                            throw new FormatException("Only inbox messages are supported for downloading.");

                    }

                    var certificatePermit = ConfigurationManager.AppSettings["mail.certificate-permit"] != null &&
                                            Convert.ToBoolean(
                                                ConfigurationManager.AppSettings["mail.certificate-permit"]);

                    string messageEml;
                    using (var client = new MailClient(mailbox, CancellationToken.None, certificatePermit: mailbox.IsTeamlab || certificatePermit))
                    {
                        var message = client.GetInboxMessage(options.MessageUid);
                        messageEml = message.ToString();
                    }

                    Console.WriteLine(@"Try StoreToFile");

                    var now = DateTime.UtcNow;

                    var path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads"),
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
                    folderId = (FolderType) Convert.ToInt32(elements[1])
                };
        }

        private static MailBoxData GetMailBox(int mailboxId)
        {
            var engine = new EngineFactory(-1);
            return engine.MailboxEngine.GetMailboxData(new ConcreteSimpleMailboxExp(mailboxId));
        }
    }
}
