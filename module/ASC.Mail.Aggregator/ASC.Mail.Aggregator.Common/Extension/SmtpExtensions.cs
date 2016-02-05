using ActiveUp.Net.Mail;
using ASC.Mail.Aggregator.Common.Authorization;
using ASC.Mail.Aggregator.Common.Logging;
using DotNetOpenAuth.Messaging;

namespace ASC.Mail.Aggregator.Common.Extension
{
    public static class SmtpExtensions
    {
        public static void Send(this SmtpClient smptClient, MailBox maibox, Message mimeMessage, ILogger log)
        {
            if (maibox.RefreshToken != null)
            {
                var grantedAccess = new GoogleOAuth2Authorization(log)
                            .RequestAccessToken(maibox.RefreshToken);

                var accessToken = grantedAccess != null ? grantedAccess.AccessToken : "";


                smptClient.SendSsl(mimeMessage, maibox.SmtpServer, maibox.SmtpPort,
                                                        maibox.SmtpAccount, accessToken,
                                                        SaslMechanism.OAuth2);
            }
            else if (maibox.OutcomingEncryptionType == EncryptionType.None)
            {
                if (maibox.AuthenticationTypeSmtp == SaslMechanism.None)
                    smptClient.Send(mimeMessage, maibox.SmtpServer, maibox.SmtpPort);
                else
                    smptClient.Send(mimeMessage, maibox.SmtpServer, maibox.SmtpPort,
                                                        maibox.SmtpAccount, maibox.SmtpPassword,
                                                        maibox.AuthenticationTypeSmtp);
            }
            else
            {
                if (maibox.AuthenticationTypeSmtp == SaslMechanism.None)
                    smptClient.SendSsl(mimeMessage, maibox.SmtpServer, maibox.SmtpPort,
                                                            maibox.OutcomingEncryptionType);
                else
                    smptClient.SendSsl(mimeMessage, maibox.SmtpServer, maibox.SmtpPort,
                                                            maibox.SmtpAccount, maibox.SmtpPassword,
                                                            maibox.AuthenticationTypeSmtp,
                                                            maibox.OutcomingEncryptionType);
            }
        }

        public static void AuthenticateSmtpGoogleOAuth2(this SmtpClient imap, MailBox account, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            var auth = new GoogleOAuth2Authorization(log);
            var grantedAccess = auth.RequestAccessToken(account.RefreshToken);
            if (grantedAccess == null)
                throw new ProtocolException("Access denied");

            log.Info("Smtp SSL connecting to {0}", account.EMail);
            imap.ConnectSsl(account.SmtpServer, account.SmtpPort);

            log.Info("Smtp connecting OK {0}", account.EMail);

            imap.SendEhloHelo();

            log.Info("Smtp logging to {0} via OAuth 2.0", account.EMail);

            imap.Authenticate(account.SmtpAccount, grantedAccess.AccessToken, SaslMechanism.OAuth2);

            log.Info("Smtp logged to {0} via OAuth 2.0", account.EMail);
        }

    }
}
