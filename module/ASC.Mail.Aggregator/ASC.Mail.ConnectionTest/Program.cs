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
using System.Configuration;
using System.Web.Configuration;
using System.Net.Mail;
using System.Threading;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.Core.Clients;

namespace ASC.Mail.ConnectionTest
{
    internal partial class Program
    {
        private static ILogger _log;
        static bool _sslCertificatesErrorPermit;

        private static void Main(string[] args)
        {
            var options = new Options();
            try
            {
                _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "ConnectionTest");
                _sslCertificatesErrorPermit = false;

                if (ConfigurationManager.AppSettings["mail.certificate-permit"] != null)
                {
                    _sslCertificatesErrorPermit =
                        Convert.ToBoolean(ConfigurationManager.AppSettings["mail.certificate-permit"]);
                }

                if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options,
                    () => _log.Info("Bad command line parameters.")))
                {
                    _log.Info("[START] ASC.Mail.ConnectionTest with config params\r\n");

                    var smtpAuth = bool.Parse(WebConfigurationManager.AppSettings["mailbox.smtp_auth"] ?? "true");

                    var mbox = new MailBox
                    {
                        EMail = new MailAddress(options.Email),
                        Password = WebConfigurationManager.AppSettings["mailbox.password"],
                        Account = WebConfigurationManager.AppSettings["mailbox.account"],
                        Port = int.Parse(WebConfigurationManager.AppSettings["mailbox.port"]),
                        Server = WebConfigurationManager.AppSettings["mailbox.server"],
                        SmtpAccount = WebConfigurationManager.AppSettings["mailbox.smtp_account"],
                        SmtpPassword = WebConfigurationManager.AppSettings["mailbox.smtp_password"],
                        SmtpPort = int.Parse(WebConfigurationManager.AppSettings["mailbox.smtp_port"]),
                        SmtpServer = WebConfigurationManager.AppSettings["mailbox.smtp_server"],
                        Imap = bool.Parse(WebConfigurationManager.AppSettings["mailbox.imap"]),
                        Encryption =
                            (EncryptionType)
                                int.Parse(WebConfigurationManager.AppSettings["mailbox.encryption_type"]),
                        SmtpEncryption =
                            (EncryptionType)
                                int.Parse(WebConfigurationManager.AppSettings["mailbox.smtp_encryption_type"]),
                        Authentication =
                            (SaslMechanism) int.Parse(WebConfigurationManager.AppSettings["mailbox.auth_type"]),
                        SmtpAuthentication = smtpAuth
                            ? (SaslMechanism) int.Parse(WebConfigurationManager.AppSettings["mailbox.smtp_auth_type"])
                            : SaslMechanism.None
                    };

                    TestConnectAccountFromConfig(mbox);
                }
                else
                {
                    _log.Info("[START] ASC.Mail.ConnectionTest -e'{0}' -p'{1}'\r\n", options.Email, options.Password);

                    TestConnectAccountSimple(options.Email, options.Password);
                }
            }
            catch (Exception ex)
            {
                _log.Fatal("Exception: {0}\r\n", ex.ToString());
            }

            _log.Info("[STOP] ASC.Mail.ConnectionTest\r\n");
        }

        private static void TestConnectAccountSimple(string email, string password)
        {
            var mailBoxManager = new MailBoxManager(_log);

            var domain = email.Substring(email.IndexOf('@') + 1);

            _log.Debug("Search mailbox settings for domain '{0}'\r\n", domain);

            var mailboxSettings = mailBoxManager.GetMailBoxSettings(domain);

            if (mailboxSettings == null)
            {
                _log.Error("[TEST FAILED] Unknown mail provider settings.");
                return;
            }

            var testMailboxes = mailboxSettings.ToMailboxList(email, password, 0, Guid.NewGuid().ToString());

            var i = 0;

            foreach (var mbox in testMailboxes)
            {
                LoginResult loginResult;

                _log.Info("Attempt #{0} Account: '{1}' Password: '{2}'", ++i, mbox.Account, mbox.Password);

                _log.Debug("{0}Server: '{1}' Port: '{2}' AuthenticationType: '{3}' EncryptionType: '{4}'",
                    mbox.Imap ? "Imap" : "Pop",
                    mbox.Server,
                    mbox.Port,
                    mbox.Authentication,
                    mbox.Encryption);

                _log.Debug("SmtpServer: '{0}' Port: '{1}' AuthenticationType: '{2}' EncryptionType: '{3}'",
                    mbox.SmtpServer,
                    mbox.SmtpPort,
                    mbox.SmtpAuthentication,
                    mbox.SmtpEncryption);

                using (var client = new MailClient(mbox, CancellationToken.None, 5000,
                    _sslCertificatesErrorPermit))
                {
                    loginResult = client.TestLogin();
                }

                if (!loginResult.IngoingSuccess)
                {
                    _log.Error("{0} failed: '{1}'", mbox.Imap ? "Imap" : "Pop",
                        loginResult.IngoingException.Message + (loginResult.IngoingException.InnerException != null
                            ? " Inner Exception: " + loginResult.IngoingException.InnerException.Message
                            : ""));
                }
                else
                {
                    _log.Info("{0} succeeded", mbox.Imap ? "Imap" : "Pop");
                }

                if (!loginResult.OutgoingSuccess)
                {
                    _log.Error("Smtp failed: '{0}'\r\n",
                        loginResult.OutgoingException.Message + (loginResult.OutgoingException.InnerException != null
                            ? " Inner Exception: " + loginResult.OutgoingException.InnerException.Message
                            : ""));
                }
                else
                {
                    _log.Info("Smtp succeeded\r\n");
                }
            }
        }

        public static void TestConnectAccountFromConfig(MailBox mbox)
        {
            LoginResult loginResult;

            _log.Info("Account: '{0}' Password: '{1}'", mbox.Account, mbox.Password);

            _log.Info("{0}Server: '{1}' Port: '{2}' AuthenticationType: '{3}' EncryptionType: '{4}'",
                mbox.Imap ? "Imap" : "Pop",
                mbox.Server,
                mbox.Port,
                mbox.Authentication,
                mbox.Encryption);

            _log.Info("SmtpServer: '{0}' Port: '{1}' AuthenticationType: '{2}' EncryptionType: '{3}'",
                mbox.SmtpServer,
                mbox.SmtpPort,
                mbox.SmtpAuthentication,
                mbox.SmtpEncryption);

            using (
                var client = new MailClient(mbox, CancellationToken.None, 5000, _sslCertificatesErrorPermit, log: _log)
                )
            {
                loginResult = client.TestLogin();
            }

            if (!loginResult.IngoingSuccess)
            {
                _log.Error("{0} failed with Exception: '{1}'", mbox.Imap ? "Imap" : "Pop",
                    loginResult.IngoingException.ToString());
            }
            else
            {
                _log.Info("{0} succeeded", mbox.Imap ? "Imap" : "Pop");
            }

            if (!loginResult.OutgoingSuccess)
            {
                _log.Error("Smtp failed with Exception: '{0}'\r\n", loginResult.OutgoingException.ToString());
            }
            else
            {
                _log.Info("Smtp succeeded\r\n");
            }
        }
    }
}
