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
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Twilio;

namespace ASC.VoipService.Twilio
{
    public class TwilioProvider : IVoipProvider
    {
        private readonly string accountSid;
        private readonly string authToken;
        private readonly TwilioRestClient client;


        public TwilioProvider(string accountSid, string authToken)
        {
            if (string.IsNullOrEmpty(accountSid)) throw new ArgumentNullException("accountSid");
            if (string.IsNullOrEmpty(authToken)) throw new ArgumentNullException("authToken");

            this.authToken = authToken;
            this.accountSid = accountSid;

            client = new TwilioRestClient(accountSid, authToken);
        }

        #region Call

        public VoipRecord GetRecord(string recordSid)
        {
            var logger = log4net.LogManager.GetLogger("ASC");
            logger.DebugFormat("recordSid {0}", recordSid);

            var result = new VoipRecord { Id = recordSid };
            var count = 6;

            while (count > 0)
            {
                var record = client.GetRecording(recordSid);
                ThrowIfError(record);

                if (!record.Price.HasValue)
                {
                    count--;
                    Thread.Sleep(10000);
                    continue;
                }

                result.Price = (-1) * record.Price.Value;
                logger.DebugFormat("recordSid {0} price {1}", recordSid, result.Price);

                result.Duration = record.Duration;
                if (record.Uri != null)
                {
                    result.Uri = record.Uri.ToString();
                }
                break;
            }

            return result;
        }

        public void CreateQueue(VoipPhone newPhone)
        {
            newPhone.Settings.Queue = ((TwilioPhone)newPhone).CreateQueue(newPhone.Number, 5, string.Empty, 5);
        }

        #endregion

        #region Numbers

        public VoipPhone BuyNumber(string phoneNumber)
        {
            var newNumber = client.AddIncomingPhoneNumber(
                new PhoneNumberOptions
                {
                    AccountSid = accountSid,
                    PhoneNumber = phoneNumber
                });

            ThrowIfError(newNumber);

            return new TwilioPhone(client) {Id = newNumber.Sid, Number = phoneNumber.Substring(1)};
        }

        public VoipPhone DeleteNumber(VoipPhone phone)
        {
            client.DeleteIncomingPhoneNumber(phone.Id);
            return phone;
        }

        public IEnumerable<VoipPhone> GetExistingPhoneNumbers()
        {
            var result = client.ListIncomingPhoneNumbers();
            ThrowIfError(result);
            return result.IncomingPhoneNumbers.Select(r => new TwilioPhone(client) {Id = r.Sid, Number = r.PhoneNumber});
        }

        public IEnumerable<VoipPhone> GetAvailablePhoneNumbers(PhoneNumberType phoneNumberType, string isoCountryCode)
        {
            var result = new AvailablePhoneNumberResult();
            var request = new AvailablePhoneNumberListRequest { VoiceEnabled = true };

            switch (phoneNumberType)
            {
                case PhoneNumberType.Local:
                    result = client.ListAvailableLocalPhoneNumbers(isoCountryCode, request);
                    break;
                /*                case PhoneNumberType.Mobile:
                                    result = twilio.ListAvailableMobilePhoneNumbers(isoCountryCode, request);
                                    break;*/
                case PhoneNumberType.TollFree:
                    result = client.ListAvailableTollFreePhoneNumbers(isoCountryCode);
                    break;
            }
            ThrowIfError(result);
            return result.AvailablePhoneNumbers.Select(r => new TwilioPhone(client) { Number = r.PhoneNumber });
        }

        public VoipPhone GetPhone(string phoneSid)
        {
            var phone = client.GetIncomingPhoneNumber(phoneSid);
            ThrowIfError(phone);

            var result = new TwilioPhone(client) { Id = phone.Sid, Number = phone.PhoneNumber, Settings = new TwilioVoipSettings() };

            if (string.IsNullOrEmpty(phone.VoiceUrl))
            {
                phone.VoiceUrl = result.Settings.Connect(false);
            }

            result.Settings.VoiceUrl = phone.VoiceUrl;

            return result;
        }

        public VoipPhone GetPhone(object[] data)
        {
            return new TwilioPhone(client)
                {
                    Id = (string) data[0],
                    Number = (string) data[1],
                    Alias = (string) data[2],
                    Settings = new TwilioVoipSettings((string) data[3])
                };
        }

        public VoipCall GetCall(string callId)
        {
            var result = new VoipCall {Id = callId};
            var count = 6;

            while (count > 0)
            {
                var call = client.GetCall(result.Id);
                if (!call.Price.HasValue || !call.Duration.HasValue)
                {
                    count--;
                    Thread.Sleep(10000);
                    continue;
                }

                result.Price = (-1)*call.Price.Value;
                result.DialDuration = call.Duration.Value;
                break;
            }

            return result;
        }

        public string GetToken(Agent agent, int seconds = 60*60*24)
        {
            var twilioCapability = new TwilioCapability(accountSid, authToken);
            twilioCapability.AllowClientIncoming(agent.ClientID);
            return twilioCapability.GenerateToken(seconds);
        }

        public void UpdateSettings(VoipPhone phone)
        {
            var result = client.UpdateIncomingPhoneNumber(phone.Id, new PhoneNumberOptions { VoiceUrl = phone.Settings.Connect(false), VoiceApplicationSid = null });
            ThrowIfError(result);
        }

        public void DisablePhone(VoipPhone phone)
        {
            var result = client.UpdateIncomingPhoneNumber(phone.Id, new PhoneNumberOptions { VoiceUrl = "", SmsUrl = "" });
            ThrowIfError(result);
        }

        #endregion

        public static void ThrowIfError(TwilioBase twilioBase)
        {
            if (twilioBase.RestException == null) return;
            throw new Exception(twilioBase.RestException.Message);
        }
    }

   

    public enum PhoneNumberType
    {
        Local,
        /*            Mobile,*/
        TollFree
    }
}