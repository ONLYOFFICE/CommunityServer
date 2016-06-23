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


using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using ASC.Common.Utils;
using ASC.Notify.Messages;
using ASC.Notify.Patterns;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace ASC.Core.Notify.Senders
{
    class AWSSender : SmtpSender
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Notify.AmazonSES");
        private readonly object locker = new object();
        private AmazonSimpleEmailServiceClient ses;
        private TimeSpan refreshTimeout;
        private DateTime lastRefresh;
        private DateTime lastSend;
        private TimeSpan sendWindow = TimeSpan.MinValue;
        private GetSendQuotaResult quota;


        public override void Init(IDictionary<string, string> properties)
        {
            base.Init(properties);
            var region = properties.ContainsKey("region") ? RegionEndpoint.GetBySystemName(properties["region"]): RegionEndpoint.USEast1;
            ses = new AmazonSimpleEmailServiceClient(properties["accessKey"], properties["secretKey"], region);
            refreshTimeout = TimeSpan.Parse(properties.ContainsKey("refreshTimeout") ? properties["refreshTimeout"] : "0:30:0");
            lastRefresh = DateTime.UtcNow - refreshTimeout; //set to refresh on first send
        }


        public override NoticeSendResult Send(NotifyMessage m)
        {
            var result = default(NoticeSendResult);
            try
            {
                try
                {
                    result = SendMessage(m);
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("Tenant: {0}, To: {1} - {2}", m.Tenant, m.To, e);
                    throw;
                }
            }
            catch (ArgumentException)
            {
                result = NoticeSendResult.MessageIncorrect;
            }
            catch (MessageRejectedException)
            {
                result = NoticeSendResult.SendingImpossible;
            }
            catch (AmazonSimpleEmailServiceException e)
            {
                result = e.ErrorType == ErrorType.Sender ? NoticeSendResult.MessageIncorrect : NoticeSendResult.TryOnceAgain;
            }
            catch (Exception)
            {
                result = NoticeSendResult.SendingImpossible;
            }

            if (result == NoticeSendResult.MessageIncorrect || result == NoticeSendResult.SendingImpossible)
            {
                log.DebugFormat("Amazon sending failed: {0}, fallback to smtp", result);
                result = base.Send(m);
            }
            return result;
        }

        private NoticeSendResult SendMessage(NotifyMessage m)
        {
            //Check if we need to query stats
            RefreshQuotaIfNeeded();
            if (quota != null)
            {
                lock (locker)
                {
                    if (quota.Max24HourSend <= quota.SentLast24Hours)
                    {
                        //Quota exceeded, queue next refresh to +24 hours
                        lastRefresh = DateTime.UtcNow.AddHours(24);
                        log.WarnFormat("Quota limit reached. setting next check to: {0}", lastRefresh);
                        return NoticeSendResult.SendingImpossible;
                    }
                }
            }

            var dest = new Destination
            {
                ToAddresses = m.To.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(a => MailAddressUtils.Create(a).Address).ToList(),
            };

            var subject = new Content(MimeHeaderUtils.EncodeMime(m.Subject)) { Charset = Encoding.UTF8.WebName, };

            Body body;
            if (m.ContentType == Pattern.HTMLContentType)
            {
                body = new Body(new Content(HtmlUtil.GetText(m.Content)) { Charset = Encoding.UTF8.WebName });
                body.Html = new Content(GetHtmlView(m.Content)) { Charset = Encoding.UTF8.WebName };
            }
            else
            {
                body = new Body(new Content(m.Content) { Charset = Encoding.UTF8.WebName });
            }

            var from = MailAddressUtils.Create(m.From).ToEncodedString();
            var request = new SendEmailRequest {Source = from, Destination = dest, Message = new Message(subject, body)};
            if (!string.IsNullOrEmpty(m.ReplyTo))
            {
                request.ReplyToAddresses.Add(MailAddressUtils.Create(m.ReplyTo).Address);
            }

            ThrottleIfNeeded();
              
            if (WorkContext.IsMono)
            {
                ServicePointManager.ServerCertificateValidationCallback = (s, cert, c, p) => true;
            }
                     
            var response = ses.SendEmail(request);
            lastSend = DateTime.UtcNow;

            return response != null ? NoticeSendResult.OK : NoticeSendResult.TryOnceAgain;
        }


        private void ThrottleIfNeeded()
        {
            //Check last send and throttle if needed
            if (sendWindow != TimeSpan.MinValue)
            {
                if (DateTime.UtcNow - lastSend <= sendWindow)
                {
                    //Possible BUG: at high frequncies maybe bug with to little differences
                    //This means that time passed from last send is less then message per second
                    log.DebugFormat("Send rate doesn't fit in send window. sleeping for: {0}", sendWindow);
                    Thread.Sleep(sendWindow);
                }
            }
        }

        private void RefreshQuotaIfNeeded()
        {
            if (!IsRefreshNeeded()) return;

            lock (locker)
            {
                if (IsRefreshNeeded())//Double check
                {
                    log.DebugFormat("refreshing qouta. interval: {0} Last refresh was at: {1}", refreshTimeout, lastRefresh);

                    //Do quota refresh
                    lastRefresh = DateTime.UtcNow.AddMinutes(1);
                    try
                    {
                        if (WorkContext.IsMono)
                        {
                            ServicePointManager.ServerCertificateValidationCallback = (s, cert, c, p) => true;
                        }

                        var r = new GetSendQuotaRequest();
                        quota = ses.GetSendQuota(r);
                        sendWindow = TimeSpan.FromSeconds(1.0 / quota.MaxSendRate);
                        log.DebugFormat("quota: {0}/{1} at {2} mps. send window:{3}", quota.SentLast24Hours, quota.Max24HourSend, quota.MaxSendRate, sendWindow);
                    }
                    catch (Exception e)
                    {
                        log.Error("error refreshing quota", e);
                    }
                }
            }
        }

        private bool IsRefreshNeeded()
        {
            return quota == null || (DateTime.UtcNow - lastRefresh) > refreshTimeout;
        }
    }
}
