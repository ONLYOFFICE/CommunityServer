using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;

namespace ASC.Mail.Aggregator.Common.Utils
{
    public static class Converter
    {
        public static List<MailBox> ToMailboxList(this ClientConfig config, string email, string password, int tenant,
            string user)
        {
            var tempList = new List<MailBox>();

            if (!config.EmailProvider.OutgoingServer.Any() || !config.EmailProvider.IncomingServer.Any())
                return tempList;

            var address = new MailAddress(email);
            var host = address.Host.ToLowerInvariant();

            tempList.AddRange(from outServer in config.EmailProvider.OutgoingServer
                from inServer in config.EmailProvider.IncomingServer
                let smtpAuthenticationType = outServer.Authentication.ToSaslMechanism()
                select new MailBox
                {
                    EMail = address,
                    Name = "",
                    UserId = user,
                    TenantId = tenant,
                    Restrict = true,
                    BeginDate = DateTime.UtcNow.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta)),
                    Imap = inServer.Type == "imap",
                    Account =
                        inServer.Username.Replace("%EMAILADDRESS%", address.Address)
                            .Replace("%EMAILLOCALPART%", address.User)
                            .Replace("%EMAILDOMAIN%", host)
                            .Replace("%EMAILHOSTNAME%", Path.GetFileNameWithoutExtension(host)),
                    Password = password,
                    Server = inServer.Hostname.Replace("%EMAILDOMAIN%", host),
                    Port = inServer.Port,
                    AuthenticationTypeIn = inServer.Authentication.ToSaslMechanism(),
                    IncomingEncryptionType = inServer.SocketType.ToEncryptionType(),
                    SmtpAccount =
                        outServer.Username.Replace("%EMAILADDRESS%", address.Address)
                            .Replace("%EMAILLOCALPART%", address.User)
                            .Replace("%EMAILDOMAIN%", host)
                            .Replace("%EMAILHOSTNAME%", Path.GetFileNameWithoutExtension(host)),
                    SmtpPassword = password,
                    SmtpServer = outServer.Hostname.Replace("%EMAILDOMAIN%", host),
                    SmtpPort = outServer.Port,
                    SmtpAuth = smtpAuthenticationType != SaslMechanism.None,
                    AuthenticationTypeSmtp = smtpAuthenticationType,
                    OutcomingEncryptionType = outServer.SocketType.ToEncryptionType()
                });

            tempList = tempList.OrderByDescending(t => t.Imap).ToList();

            return tempList;
        }

        public static EncryptionType ToEncryptionType(this string type)
        {
            switch (type.ToLower().Trim())
            {
                case "ssl":
                    return EncryptionType.SSL;
                case "starttls":
                    return EncryptionType.StartTLS;
                case "plain":
                    return EncryptionType.None;
                default:
                    throw new ArgumentException("Unknown mail server socket type: " + type);
            }
        }

        public static string ToNameString(this EncryptionType encryptionType)
        {
            switch (encryptionType)
            {
                case EncryptionType.SSL:
                    return "SSL";
                case EncryptionType.StartTLS:
                    return "STARTTLS";
                case EncryptionType.None:
                    return "plain";
                default:
                    throw new ArgumentException("Unknown mail server EncryptionType: " + Enum.GetName(typeof(EncryptionType), encryptionType));
            }
        }

        public static SaslMechanism ToSaslMechanism(this string type)
        {
            switch (type.ToLower().Trim())
            {
                case "":
                case "oauth2":
                case "password-cleartext":
                    return SaslMechanism.Login;
                case "none":
                    return SaslMechanism.None;
                case "password-encrypted":
                    return SaslMechanism.CramMd5;
                default:
                    throw new ArgumentException("Unknown mail server authentication type: " + type);
            }
        }

        public static string ToNameString(this SaslMechanism saslType)
        {
            switch (saslType)
            {
                case SaslMechanism.Login:
                    return "";
                case SaslMechanism.None:
                    return "none";
                case SaslMechanism.CramMd5:
                    return "password-encrypted";
                default:
                    throw new ArgumentException("Unknown mail server SaslMechanism: " + Enum.GetName(typeof(SaslMechanism), saslType));
            }
        }
    }
}
