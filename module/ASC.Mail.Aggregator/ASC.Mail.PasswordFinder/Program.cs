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
using System.IO;
using System.Text;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Core.Iterators;
using log4net.Config;

namespace ASC.Mail.PasswordFinder
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                XmlConfigurator.Configure();

                var mailBoxManager = new MailBoxManager(new NullLogger());

                var mailboxIterator = new MailboxIterator(mailBoxManager);

                var mailbox = mailboxIterator.First();

                if (mailbox == null)
                {
                    Console.WriteLine("Accounts not found.");
                    ShowAnyKey();
                    return;
                }

                var sbuilder = new StringBuilder();

                var i = 0;

                while (!mailboxIterator.IsDone)
                {
                    if (!mailbox.IsRemoved)
                    {
                        var current = new StringBuilder();

                        var userInfo = mailbox.GetUserInfo();

                        current.AppendFormat("#{0}. Id: {1} MailBox: {2}, Tenant: {3}, User: '{4}' \r\n", ++i,
                            mailbox.MailBoxId, mailbox.EMail, mailbox.TenantId,
                            userInfo != null ? userInfo.UserName : mailbox.UserId);

                        current.AppendLine();

                        current.AppendFormat("\t\t{0} settings:\r\n", mailbox.Imap ? "Imap4" : "Pop3");
                        current.AppendFormat("\t\tLogin:\t\t{0}\r\n", mailbox.Account);
                        current.AppendFormat("\t\tPassword:\t{0}\r\n",
                            string.IsNullOrEmpty(mailbox.Password)
                                ? "[-=! ERROR: Invalid Decription !=-]"
                                : mailbox.Password);
                        current.AppendFormat("\t\tHost:\t\t{0}\r\n", mailbox.Server);
                        current.AppendFormat("\t\tPort:\t\t{0}\r\n", mailbox.Port);
                        current.AppendFormat("\t\tAuthType:\t{0}\r\n",
                            Enum.GetName(typeof(SaslMechanism), mailbox.Authentication));
                        current.AppendFormat("\t\tEncryptType:\t{0}\r\n",
                            Enum.GetName(typeof(EncryptionType), mailbox.Encryption));

                        current.AppendLine();

                        current.Append("\t\tSmtp settings:\r\n");
                        current.AppendFormat("\t\tSmtpAuth:\t{0}\r\n", mailbox.SmtpAuth);
                        if (mailbox.SmtpAuth)
                        {
                            current.AppendFormat("\t\tSmtpLogin:\t{0}\r\n", mailbox.SmtpAccount);
                            current.AppendFormat("\t\tSmtPassword:\t{0}\r\n",
                                string.IsNullOrEmpty(mailbox.SmtpPassword)
                                    ? "[-=! ERROR: Invalid Decription !=-]"
                                    : mailbox.SmtpPassword);
                        }
                        current.AppendFormat("\t\tHost:\t\t{0}\r\n", mailbox.SmtpServer);
                        current.AppendFormat("\t\tPort:\t\t{0}\r\n", mailbox.SmtpPort);
                        current.AppendFormat("\t\tAuthType:\t{0}\r\n",
                            Enum.GetName(typeof(SaslMechanism), mailbox.SmtpAuthentication));
                        current.AppendFormat("\t\tEncryptType:\t{0}\r\n",
                            Enum.GetName(typeof(EncryptionType), mailbox.SmtpEncryption));

                        current.AppendLine();

                        Console.WriteLine(current.ToString());

                        sbuilder.Append(current);
                    }

                    mailbox = mailboxIterator.Next();
                }

                Console.WriteLine("Try StoreToFile");

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mailboxes.txt");

                var pathFile = StoreToFile(sbuilder.ToString(), path, true);

                Console.WriteLine("[SUCCESS] File was stored into path \"{0}\"", pathFile);
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

        private static string StoreToFile(string text, string fileName, bool useTemp)
        {
            var tempPath = useTemp ? Path.GetTempFileName() : fileName;
            var sw = File.CreateText(tempPath);
            sw.Write(text);
            sw.Close();

            if (!useTemp)
                return tempPath;

            if (string.IsNullOrEmpty(fileName))
                return tempPath;

            var directory = Path.GetDirectoryName(fileName);
            // Create the traget path iof it does not exist
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (string.IsNullOrEmpty(Path.GetFileName(fileName)))
            {
                var savePath = Path.Combine(fileName, Path.GetFileNameWithoutExtension(tempPath) + ".eml");
                File.Move(tempPath, savePath);
                File.Delete(tempPath);
                return savePath;
            }

            File.Delete(fileName);
            File.Move(tempPath, fileName);
            File.Delete(tempPath);

            return fileName;
        }
    }
}
