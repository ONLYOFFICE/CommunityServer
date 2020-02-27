/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Common.Configuration;
using ASC.Core.Tenants;
using ASC.FederatedLogin.LoginProviders;
using ASC.VoipService.Dao;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ASC.Web.Core.Sms
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
            SmscProvider = ConsumerFactory.Get<SmscProvider>();

            ClickatellProvider = ConsumerFactory.Get<ClickatellProvider>();
            ClickatellUSAProvider = ConsumerFactory.Get<ClickatellUSAProvider>();

            TwilioProvider = ConsumerFactory.Get<TwilioProvider>();
            TwilioSaaSProvider = ConsumerFactory.Get<TwilioSaaSProvider>();
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
                && !string.IsNullOrEmpty(smsUsa = ClickatellProvider["clickatellUSA"]) && Regex.IsMatch(number, smsUsa))
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


    public abstract class SmsProvider : Consumer
    {
        protected static readonly ILog Log = LogManager.GetLogger("ASC");
        protected static readonly ICache Cache = AscCache.Memory;

        protected virtual string SendMessageUrlFormat { get; set; }
        protected virtual string GetBalanceUrlFormat { get; set; }
        protected virtual string Key { get; set; }
        protected virtual string Secret { get; set; }
        protected virtual string Sender { get; set; }

        protected SmsProvider()
        {
        }

        protected SmsProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }

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
        public SmscProvider()
        {
        }

        public SmscProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }

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
            get { return this["smsclogin"]; }
        }

        protected override string Secret
        {
            get { return this["smscpsw"]; }
        }

        protected override string Sender
        {
            get { return this["smscsender"]; }
        }

        public override bool Enable()
        {
            return
                !string.IsNullOrEmpty(Key)
                && !string.IsNullOrEmpty(Secret);
        }

        public string GetBalance(bool eraseCache = false)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            var tenantCache = tenant == null ? Tenant.DEFAULT_TENANT : tenant.TenantId;

            var key = "sms/smsc/" + tenantCache;
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
            var smsCis = this["smsccis"];
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
        protected override string SendMessageUrlFormat
        {
            get { return "https://platform.clickatell.com/messages/http/send?apiKey={secret}&to={phone}&content={text}&from={sender}"; }
            set { }
        }

        protected override string Secret
        {
            get { return this["clickatellapiKey"]; }
            set { }
        }

        protected override string Sender
        {
            get { return this["clickatellSender"]; }
            set { }
        }

        public override bool Enable()
        {
            return !string.IsNullOrEmpty(Secret);
        }

        public ClickatellProvider()
        {
        }

        public ClickatellProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }
    }

    public class ClickatellUSAProvider : ClickatellProvider
    {
        public ClickatellUSAProvider()
        {
        }

        public ClickatellUSAProvider(string name, int order, Dictionary<string, string> additional = null)
            : base(name, order, null, additional)
        {
        }
    }

    public class TwilioProvider : SmsProvider, IValidateKeysProvider
    {
        protected override string Key
        {
            get { return this["twilioAccountSid"]; }
            set { }
        }

        protected override string Secret
        {
            get { return this["twilioAuthToken"]; }
            set { }
        }

        protected override string Sender
        {
            get { return this["twiliosender"]; }
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
                var smsMessage = MessageResource.Create(new PhoneNumber(number), body: message, @from: new PhoneNumber(Sender), client: twilioRestClient);
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

        public TwilioProvider()
        {
        }

        public TwilioProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }


        public bool ValidateKeys()
        {
            try
            {
                new VoipService.Twilio.TwilioProvider(Key, Secret).GetExistingPhoneNumbers();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ClearOldNumbers()
        {
            if (string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(Secret)) return;

            var provider = new VoipService.Twilio.TwilioProvider(Key, Secret);

            var dao = new CachedVoipDao(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            var numbers = dao.GetNumbers();
            foreach (var number in numbers)
            {
                provider.DisablePhone(number);
                dao.DeleteNumber(number.Id);
            }
        }
    }

    public class TwilioSaaSProvider : TwilioProvider
    {
        public TwilioSaaSProvider()
        {
        }

        public TwilioSaaSProvider(string name, int order, Dictionary<string, string> additional = null)
            : base(name, order, null, additional)
        {
        }
    }
}