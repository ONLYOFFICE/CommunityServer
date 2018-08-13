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
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Common.Caching;
using ASC.FederatedLogin.LoginProviders;
using ASC.Thrdparty.Configuration;
using ASC.VoipService.Twilio;
using ASC.Web.Studio.Utility;
using log4net;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ASC.Web.Studio.Core.SMS
{
    public static class SmsProviderManager
    {
        public static readonly SmscProvider SmscProvider;
        public static readonly ClickatellProvider ClickatellProvider;
        public static readonly TwilioProvider TwilioProvider;

        public static readonly ClickatellProvider ClickatellUSAProvider = null;
        public static readonly TwilioProvider TwilioSaaSProvider = null;

        static SmsProviderManager()
        {
            SmscProvider = new SmscProvider();
            ClickatellProvider = new ClickatellProvider(KeyStorage.Get("clickatellapiKey"));
            ClickatellUSAProvider = new ClickatellProvider(KeyStorage.Get("clickatellUSAapiKey"), KeyStorage.Get("clickatellUSAsender"));
            TwilioProvider = new TwilioProvider(TwilioLoginProvider.TwilioAccountSid, TwilioLoginProvider.TwilioAuthToken, KeyStorage.Get("twiliosender"));
            TwilioSaaSProvider = new TwilioProvider(KeyStorage.Get("twilioSaaSAccountSid"), KeyStorage.Get("twilioSaaSAuthToken"), KeyStorage.Get("twilioSaaSsender"));
        }

        public static bool Enabled()
        {
            return SmscProvider.Enable() || ClickatellProvider.Enable() || ClickatellUSAProvider.Enable() || TwilioProvider.Enable() || TwilioSaaSProvider.Enable();
        }

        public static bool SendMessage(string number, string message)
        {
            if (!Enabled()) return false;

            SmsProvider provider = null;
            if (ClickatellProvider.Enable())
            {
                provider = ClickatellProvider;
            }

            string smsUsa;
            if (ClickatellUSAProvider.Enable()
                && !string.IsNullOrEmpty(smsUsa = KeyStorage.Get("clickatellUSA")) && Regex.IsMatch(number, smsUsa))
            {
                provider = ClickatellUSAProvider;
            }

            if (provider == null && TwilioProvider.Enable())
            {
                provider = TwilioProvider;
            }

            if (provider == null && TwilioSaaSProvider.Enable())
            {
                provider = TwilioSaaSProvider;
            }

            if (SmscProvider.Enable()
                && (provider == null
                    || SmscProvider.SuitableNumber(number)))
            {
                provider = SmscProvider;
            }

            if (provider == null)
            {
                return false;
            }

            return provider.SendMessage(number, message);
        }
    }


    public abstract class SmsProvider
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof (SmsProvider));
        protected static readonly ICache Cache = AscCache.Memory;

        protected virtual string SendMessageUrlFormat { get; set; }
        protected virtual string GetBalanceUrlFormat { get; set; }
        protected virtual string Key { get; set; }
        protected virtual string Secret { get; set; }
        protected virtual string Sender { get; set; }

        public virtual bool Enable()
        {
            return true;
        }

        private string SendMessageUrl()
        {
            return SendMessageUrlFormat
                .Replace("{key}", Key)
                .Replace("{secret}", Secret)
                .Replace("{sender}", Sender);
        }

        public virtual bool SendMessage(string number, string message)
        {
            try
            {
                var url = SendMessageUrl();
                url = url.Replace("{phone}", number).Replace("{text}", HttpUtility.UrlEncode(message));

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 15000;

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var result = reader.ReadToEnd();
                            Log.InfoFormat("SMS was sent to {0}, service returned: {1}", number, result);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to send sms message", ex);
            }
            return false;
        }
    }

    public class SmscProvider : SmsProvider, IValidateKeysProvider
    {
        protected override string SendMessageUrlFormat
        {
            get { return "https://smsc.ru/sys/send.php?login={key}&psw={secret}&phones={phone}&mes={text}&fmt=3&sender={sender}&charset=utf-8"; }
            set { }
        }

        protected override string GetBalanceUrlFormat
        {
            get { return "https://smsc.ru/sys/balance.php?login={key}&psw={secret}"; }
            set { }
        }

        protected override string Key
        {
            get { return KeyStorage.Get("smsclogin"); }
            set { }
        }

        protected override string Secret
        {
            get { return KeyStorage.Get("smscpsw"); }
            set { }
        }

        protected override string Sender
        {
            get { return KeyStorage.Get("smscsender"); }
            set { }
        }

        public override bool Enable()
        {
            return
                !string.IsNullOrEmpty(Key)
                && !string.IsNullOrEmpty(Secret);
        }

        public string GetBalance(bool eraseCache = false)
        {
            var key = "sms/smsc/" + TenantProvider.CurrentTenantID;
            if (eraseCache) Cache.Remove(key);

            var balance = Cache.Get<string>(key);

            if (string.IsNullOrEmpty(balance))
            {
                try
                {
                    var url = GetBalanceUrl();

                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Timeout = 1000;

                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                var result = reader.ReadToEnd();
                                Log.InfoFormat("SMS balance service returned: {0}", result);

                                balance = result;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Failed request sms balance", ex);
                    balance = string.Empty;
                }

                Cache.Insert(key, balance, TimeSpan.FromMinutes(1));
            }

            return balance;
        }

        private string GetBalanceUrl()
        {
            return GetBalanceUrlFormat
                .Replace("{key}", Key)
                .Replace("{secret}", Secret);
        }

        public bool SuitableNumber(string number)
        {
            var smsCis = KeyStorage.Get("smsccis");
            return !string.IsNullOrEmpty(smsCis) && Regex.IsMatch(number, smsCis);
        }

        public bool ValidateKeys()
        {
            double balance;
            return double.TryParse(GetBalance(true), NumberStyles.Number, CultureInfo.InvariantCulture, out balance) && balance > 0;
        }
    }

    public class ClickatellProvider : SmsProvider
    {
        private readonly string _secret;
        private readonly string _sender;

        public ClickatellProvider(string secret, string sender = null)
        {
            _secret = secret;
            _sender = sender;
        }

        protected override string SendMessageUrlFormat
        {
            get { return "https://platform.clickatell.com/messages/http/send?apiKey={secret}&to={phone}&content={text}&from={sender}"; }
            set { }
        }

        protected override string Secret
        {
            get { return _secret; }
            set { }
        }

        protected override string Sender
        {
            get { return _sender; }
            set { }
        }

        public override bool Enable()
        {
            return !string.IsNullOrEmpty(Secret);
        }
    }

    public class TwilioProvider : SmsProvider
    {
        private readonly string _key;
        private readonly string _secret;
        private readonly string _sender;

        public TwilioProvider(string key, string secret, string sender)
        {
            _key = key;
            _secret = secret;
            _sender = sender;
        }

        protected override string Key
        {
            get { return _key; }
            set { }
        }

        protected override string Secret
        {
            get { return _secret; }
            set { }
        }

        protected override string Sender
        {
            get { return _sender; }
            set { }
        }

        public override bool Enable()
        {
            return
                !string.IsNullOrEmpty(Key)
                && !string.IsNullOrEmpty(Secret)
                && !string.IsNullOrEmpty(Sender);
        }

        public override bool SendMessage(string number, string message)
        {
            if (!number.StartsWith("+")) number = "+" + number;
            var twilioRestClient = new TwilioRestClient(Key, Secret);

            try
            {
                var smsMessage = MessageResource.Create(new PhoneNumber(number), body: message, from: new PhoneNumber(Sender), client: twilioRestClient);
                Log.InfoFormat("SMS was sent to {0}, status: {1}", number, smsMessage.Status);
                if (!smsMessage.ErrorCode.HasValue)
                {
                    return true;
                }
                Log.Error("Failed to send sms. code: " + smsMessage.ErrorCode.Value + " message: " + smsMessage.ErrorMessage);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to send sms message via tiwilio", ex);
            }

            return false;
        }
    }
}