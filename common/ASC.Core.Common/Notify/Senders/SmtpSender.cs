/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Net;

using ASC.Common.Logging;
using ASC.Common.Utils;
using ASC.Notify.Messages;
using ASC.Notify.Patterns;

using MailKit.Security;

using MimeKit;

namespace ASC.Core.Notify.Senders
{
    internal class SmtpSender : INotifySender
    {
        private const string HTML_FORMAT =
            @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"">
<html>
<head>
<meta content=""text/html;charset=UTF-8"" http-equiv=""Content-Type"">
</head>
<body>{0}</body>
</html>";

        protected ILog Log { get; private set; }
        private IDictionary<string, string> _initProperties;

        private string _host;
        private int _port;
        private bool _ssl;
        private ICredentials _credentials;
        private SaslMechanism _saslMechanism;
        const int NETWORK_TIMEOUT = 30000;

        public SmtpSender()
        {
            _initProperties = new Dictionary<string, string>();

            Log = LogManager.GetLogger("ASC.Notify");
        }

        public virtual void Init(IDictionary<string, string> properties)
        {
            _initProperties = properties;
        }

        private void BuildSmtpSettings()
        {
            if (CoreContext.Configuration.SmtpSettings.IsDefaultSettings && _initProperties.ContainsKey("host") && !String.IsNullOrEmpty(_initProperties["host"]))
            {
                _host = _initProperties["host"];

                if (_initProperties.ContainsKey("port") && !string.IsNullOrEmpty(_initProperties["port"]))
                {
                    _port = int.Parse(_initProperties["port"]);
                }
                else
                {
                    _port = 25;
                }

                if (_initProperties.ContainsKey("enableSsl") && !string.IsNullOrEmpty(_initProperties["enableSsl"]))
                {
                    _ssl = bool.Parse(_initProperties["enableSsl"]);
                }
                else
                {
                    _ssl = false;
                }

                if (_initProperties.ContainsKey("userName"))
                {
                    var useNtlm = _initProperties.ContainsKey("useNtlm") && bool.Parse(_initProperties["useNtlm"]);
                    _credentials = !useNtlm ? new NetworkCredential(_initProperties["userName"], _initProperties["password"]) : null;
                    _saslMechanism = useNtlm ? new SaslMechanismNtlm(_initProperties["userName"], _initProperties["password"]) : null;
                }
            }
            else
            {
                var s = CoreContext.Configuration.SmtpSettings;

                _host = s.Host;
                _port = s.Port;
                _ssl = s.EnableSSL; 

                if (!string.IsNullOrEmpty(s.CredentialsUserName))
                {
                    _credentials = !s.UseNtlm ? new NetworkCredential(s.CredentialsUserName, s.CredentialsUserPassword) : null;
                    _saslMechanism = s.UseNtlm ? new SaslMechanismNtlm(s.CredentialsUserName, s.CredentialsUserPassword) : null;
                }
            }
        }


        public virtual NoticeSendResult Send(NotifyMessage m)
        {
            CoreContext.TenantManager.SetCurrentTenant(m.Tenant);
            var smtpClient = GetSmtpClient();
            var result = NoticeSendResult.TryOnceAgain;
            try
            {
                try
                {
                    BuildSmtpSettings();

                    var mail = BuildMailMessage(m);

                    Log.DebugFormat("SmtpSender - host={0}; port={1}; enableSsl={2} enableAuth={3}", _host, _port, _ssl, _credentials != null);

                    smtpClient.Connect(_host, _port,
                        _ssl ? SecureSocketOptions.Auto : SecureSocketOptions.None);

                    if (_credentials != null)
                    {
                        smtpClient.Authenticate(_credentials);
                    }
                    else if (_saslMechanism != null)
                    {
                        smtpClient.Authenticate(_saslMechanism);
                    }

                    smtpClient.Send(mail);
                    result = NoticeSendResult.OK;
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("Tenant: {0}, To: {1} - {2}", m.Tenant, m.To, e);
                    throw;
                }
            }
            catch (ObjectDisposedException)
            {
                result = NoticeSendResult.SendingImpossible;
            }
            catch (InvalidOperationException)
            {
                result = string.IsNullOrEmpty(_host) || _port == 0
                    ? NoticeSendResult.SendingImpossible
                    : NoticeSendResult.TryOnceAgain;
            }
            catch (IOException)
            {
                result = NoticeSendResult.TryOnceAgain;
            }
            catch (MailKit.Net.Smtp.SmtpProtocolException)
            {
                result = NoticeSendResult.SendingImpossible;
            }
            catch (MailKit.Net.Smtp.SmtpCommandException e)
            {
                switch (e.StatusCode)
                {
                    case MailKit.Net.Smtp.SmtpStatusCode.MailboxBusy:
                    case MailKit.Net.Smtp.SmtpStatusCode.MailboxUnavailable:
                    case MailKit.Net.Smtp.SmtpStatusCode.ExceededStorageAllocation:
                        result = NoticeSendResult.TryOnceAgain;
                        break;
                    case MailKit.Net.Smtp.SmtpStatusCode.MailboxNameNotAllowed:
                    case MailKit.Net.Smtp.SmtpStatusCode.UserNotLocalWillForward:
                    case MailKit.Net.Smtp.SmtpStatusCode.UserNotLocalTryAlternatePath:
                        result = NoticeSendResult.MessageIncorrect;
                        break;
                    default:
                        if (e.StatusCode != MailKit.Net.Smtp.SmtpStatusCode.Ok)
                        {
                            result = NoticeSendResult.TryOnceAgain;
                        }
                        break;
                }
            }
            catch (Exception)
            {
                result = NoticeSendResult.SendingImpossible;
            }
            finally
            {
                if (smtpClient.IsConnected)
                    smtpClient.Disconnect(true);

                smtpClient.Dispose();
            }
            return result;
        }

        private MimeMessage BuildMailMessage(NotifyMessage m)
        {
            var mimeMessage = new MimeMessage
            {
                Subject = m.Subject
            };

            var fromAddress = MailboxAddress.Parse(ParserOptions.Default, m.From);

            mimeMessage.From.Add(fromAddress);

            foreach (var to in m.To.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                mimeMessage.To.Add(MailboxAddress.Parse(ParserOptions.Default, to));
            }

            if (m.ContentType == Pattern.HTMLContentType)
            {
                var textPart = new TextPart("plain")
                {
                    Text = HtmlUtil.GetText(m.Content),
                    ContentTransferEncoding = ContentEncoding.QuotedPrintable
                };

                var multipartAlternative = new MultipartAlternative { textPart };

                var htmlPart = new TextPart("html")
                {
                    Text = GetHtmlView(m.Content),
                    ContentTransferEncoding = ContentEncoding.QuotedPrintable
                };

                if (m.EmbeddedAttachments != null && m.EmbeddedAttachments.Length > 0)
                {
                    var multipartRelated = new MultipartRelated
                    {
                        Root = htmlPart
                    };

                    foreach (var attachment in m.EmbeddedAttachments)
                    {
                        var mimeEntity = ConvertAttachmentToMimePart(attachment);
                        if (mimeEntity != null)
                            multipartRelated.Add(mimeEntity);
                    }

                    multipartAlternative.Add(multipartRelated);
                }
                else
                {
                    multipartAlternative.Add(htmlPart);
                }

                mimeMessage.Body = multipartAlternative;
            }
            else
            {
                mimeMessage.Body = new TextPart("plain")
                {
                    Text = m.Content,
                    ContentTransferEncoding = ContentEncoding.QuotedPrintable
                };
            }

            if (!string.IsNullOrEmpty(m.ReplyTo))
            {
                mimeMessage.ReplyTo.Add(MailboxAddress.Parse(ParserOptions.Default, m.ReplyTo));
            }

            mimeMessage.Headers.Add("Auto-Submitted", string.IsNullOrEmpty(m.AutoSubmitted) ? "auto-generated" : m.AutoSubmitted);

            return mimeMessage;
        }

        protected string GetHtmlView(string body)
        {
            return string.Format(HTML_FORMAT, body);
        }

        private MailKit.Net.Smtp.SmtpClient GetSmtpClient()
        {
            var sslCertificatePermit = ConfigurationManagerExtension.AppSettings["mail.certificate-permit"] != null &&
                                    Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["mail.certificate-permit"]);

            var smtpClient = new MailKit.Net.Smtp.SmtpClient
            {
                Timeout = NETWORK_TIMEOUT
            };

            if (sslCertificatePermit)
                smtpClient.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            return smtpClient;
        }

        private static MimePart ConvertAttachmentToMimePart(NotifyMessageAttachment attachment)
        {
            try
            {
                if (attachment == null || string.IsNullOrEmpty(attachment.FileName) || string.IsNullOrEmpty(attachment.ContentId) || attachment.Content == null)
                    return null;

                var extension = Path.GetExtension(attachment.FileName);

                if (string.IsNullOrEmpty(extension))
                    return null;

                return new MimePart("image", extension.TrimStart('.'))
                {
                    ContentId = attachment.ContentId,
                    Content = new MimeContent(new MemoryStream(attachment.Content)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = attachment.FileName
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}