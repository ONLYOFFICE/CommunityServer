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
using System.IO;
using System.Text;
using ASC.Core.Users;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;
using ASC.Mail.Iterators;
using Newtonsoft.Json;

namespace ASC.Mail.PasswordFinder
{
    internal partial class Program
    {
        const string IMAP = "IMAP";
        const string POP3 = "POP3";
        const string SMTP = "SMTP";

        private static void Main(string[] args)
        {
            var options = new Options();

            if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options,
                () => Console.WriteLine(@"Bad command line parameters.")))
            {
                return;
            }
            
            try
            {
                var mailboxIterator = new MailboxIterator();

                var mailbox = mailboxIterator.First();

                if (mailbox == null)
                {
                    Console.WriteLine(@"Accounts not found.");
                    ShowAnyKey(options);
                    return;
                }

                StringBuilder sbuilder = null;
                List<MailAccount> accounts = null;

                if (!options.NeedJson)
                {
                    sbuilder = new StringBuilder();
                }
                else
                {
                    accounts = new List<MailAccount>();
                }

                var i = 0;

                while (!mailboxIterator.IsDone)
                {
                    if (mailbox.IsRemoved || !mailbox.IsTeamlab)
                    {
                        mailbox = mailboxIterator.Next();
                        continue;
                    }

                    var current = new StringBuilder();

                    var userInfo = mailbox.GetUserInfo();

                    var isUserRemoved = userInfo == null || userInfo.Equals(Constants.LostUser);

                    var user = isUserRemoved ? "user is removed" : userInfo.UserName;

                    current.AppendFormat("#{0}. Id: {1} MailBox: {2}, Tenant: {3}, User: '{4}' ID: {5}\r\n",
                        ++i, mailbox.MailBoxId, mailbox.EMail, mailbox.TenantId, user,
                        mailbox.UserId);

                    current.AppendLine();

                    var inType = mailbox.Imap ? IMAP : POP3;

                    current.AppendFormat("\t\t{0} settings:\r\n", inType);
                    current.AppendFormat("\t\tLogin:\t\t{0}\r\n", mailbox.Account);

                    string inPassword;

                    if (!mailbox.IsOAuth)
                    {
                        inPassword = 
                            string.IsNullOrEmpty(mailbox.Password)
                                ? "[-=! ERROR: Invalid Decription !=-]"
                                : mailbox.Password;
                    }
                    else
                    {
                        inPassword = "[-=! OAuth: no password stored !=-]";
                    }

                     current.AppendFormat("\t\tPassword:\t{0}\r\n", inPassword);

                    current.AppendFormat("\t\tHost:\t\t{0}\r\n", mailbox.Server);
                    current.AppendFormat("\t\tPort:\t\t{0}\r\n", mailbox.Port);

                    var inAuthentication = Enum.GetName(typeof(SaslMechanism), mailbox.Authentication);

                    current.AppendFormat("\t\tAuthType:\t{0}\r\n", inAuthentication);

                    var inEncryption = Enum.GetName(typeof(EncryptionType), mailbox.Encryption);

                    current.AppendFormat("\t\tEncryptType:\t{0}\r\n", inEncryption);

                    current.AppendLine();

                    current.AppendFormat("\t\t{0} settings:\r\n", SMTP);
                    current.AppendFormat("\t\tSmtpAuth:\t{0}\r\n", mailbox.SmtpAuth);
                    if (mailbox.SmtpAuth)
                    {
                        current.AppendFormat("\t\tSmtpLogin:\t{0}\r\n", mailbox.SmtpAccount);
                        if (!mailbox.IsOAuth)
                        {
                            current.AppendFormat("\t\tSmtPassword:\t{0}\r\n",
                                string.IsNullOrEmpty(mailbox.SmtpPassword)
                                    ? "[-=! ERROR: Invalid Decription !=-]"
                                    : mailbox.SmtpPassword);
                        }
                        else
                        {
                            current.AppendFormat("\t\tSmtPassword:\t[-=! OAuth: no password stored !=-]\r\n");
                        }
                    }
                    current.AppendFormat("\t\tHost:\t\t{0}\r\n", mailbox.SmtpServer);
                    current.AppendFormat("\t\tPort:\t\t{0}\r\n", mailbox.SmtpPort);

                    var outAuthentication = Enum.GetName(typeof(SaslMechanism), mailbox.SmtpAuthentication);

                    current.AppendFormat("\t\tAuthType:\t{0}\r\n", outAuthentication);

                    var outEncryption = Enum.GetName(typeof(EncryptionType), mailbox.SmtpEncryption);

                    current.AppendFormat("\t\tEncryptType:\t{0}\r\n", outEncryption);

                    current.AppendLine();

                    Console.WriteLine(current.ToString());

                    if (!options.NeedJson)
                    {
                        if (sbuilder != null) 
                            sbuilder.Append(current);
                    }
                    else
                    {
                        if (accounts != null)
                        {
                            accounts.Add(new MailAccount
                            {
                                email = mailbox.EMail.Address,
                                tenantId = mailbox.TenantId,
                                user = user,
                                userId = mailbox.UserId,
                                settings = new List<MailAccountSetting>
                                {
                                    new MailAccountSetting
                                    {
                                        type = inType,
                                        host = mailbox.Server,
                                        port = mailbox.Port.ToString(),
                                        authentication = inAuthentication,
                                        encryption = inEncryption,
                                        login = mailbox.Account,
                                        password = inPassword
                                    },
                                    new MailAccountSetting
                                    {
                                        type = SMTP,
                                        host = mailbox.SmtpServer,
                                        port = mailbox.SmtpPort.ToString(),
                                        authentication = outAuthentication,
                                        encryption = outEncryption,
                                        login = mailbox.SmtpAccount,
                                        password = inPassword
                                    }
                                }
                            });
                        }
                    }

                    mailbox = mailboxIterator.Next();
                }

                string text = null;

                if (!options.NeedJson)
                {
                    if (sbuilder != null)
                    {
                        text = sbuilder.ToString();
                    }
                }
                else
                {
                    text = JsonConvert.SerializeObject(accounts, Formatting.Indented);
                }

                if (!string.IsNullOrEmpty(text))
                {
                    Console.WriteLine(@"Try StoreToFile");

                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        options.NeedJson ? "mailboxes.json" : "mailboxes.txt");

                    var pathFile = StoreToFile(text, path, true);

                    Console.WriteLine(@"[SUCCESS] File was stored into path ""{0}""", pathFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            ShowAnyKey(options);
        }

        private static void ShowAnyKey(Options options)
        {
            Console.WriteLine();

            if (options.ExitOnEnd)
                return;

            Console.WriteLine(@"Any key to exit...");
            Console.Read();
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
                var savePath = Path.Combine(fileName, Path.GetFileNameWithoutExtension(tempPath) + ".txt");
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

    internal class MailAccount
    {
        public string email;

        public string user;

        public int tenantId;

        public string userId;

        public List<MailAccountSetting> settings;
    }

    internal class MailAccountSetting
    {
        public string type;

        public string host;

        public string port;

        public string authentication;

        public string encryption;

        public string login;

        public string password;
    }
}
