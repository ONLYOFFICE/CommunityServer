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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ASC.Common.Logging;
using Twilio.Clients;
using Twilio.Exceptions;
using Twilio.Jwt;
using Twilio.Jwt.Client;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Api.V2010.Account.AvailablePhoneNumberCountry;
using Twilio.Types;
using RecordingResource = Twilio.Rest.Api.V2010.Account.Call.RecordingResource;


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

        public VoipRecord GetRecord(string callId, string recordSid)
        {
            var logger = LogManager.GetLogger("ASC");
            logger.DebugFormat("recordSid {0}", recordSid);

            var result = new VoipRecord { Id = recordSid };
            var count = 6;

            while (count > 0)
            {
                try
                {
                    var record = RecordingResource.Fetch(callId, recordSid, client: client);

                    if (!record.Price.HasValue)
                    {
                        count--;
                        Thread.Sleep(10000);
                        continue;
                    }

                    result.Price = (-1)*record.Price.Value;
                    logger.DebugFormat("recordSid {0} price {1}", recordSid, result.Price);

                    result.Duration = Convert.ToInt32(record.Duration);
                    if (record.Uri != null)
                    {
                        result.Uri = record.Uri;
                    }
                    break;
                }
                catch (ApiException)
                {
                    count--;
                    Thread.Sleep(10000);
                }
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
            var newNumber = IncomingPhoneNumberResource.Create(
                new CreateIncomingPhoneNumberOptions
                {
                    PathAccountSid = accountSid,
                    PhoneNumber = new PhoneNumber(phoneNumber)
                }, client);

            return new TwilioPhone(client) {Id = newNumber.Sid, Number = phoneNumber.Substring(1)};
        }

        public VoipPhone DeleteNumber(VoipPhone phone)
        {
            IncomingPhoneNumberResource.Delete(phone.Id, client: client);
            return phone;
        }

        public IEnumerable<VoipPhone> GetExistingPhoneNumbers()
        {
            var result = IncomingPhoneNumberResource.Read(client: client);
            return result.Select(r => new TwilioPhone(client) {Id = r.Sid, Number = r.PhoneNumber.ToString()});
        }

        public IEnumerable<VoipPhone> GetAvailablePhoneNumbers(PhoneNumberType phoneNumberType, string isoCountryCode)
        {
            switch (phoneNumberType)
            {
                case PhoneNumberType.Local:
                    return LocalResource.Read(isoCountryCode, voiceEnabled: true, client: client).Select(r => new TwilioPhone(client) { Number = r.PhoneNumber.ToString() });
                case PhoneNumberType.TollFree:
                    return TollFreeResource.Read(isoCountryCode, voiceEnabled: true, client: client).Select(r => new TwilioPhone(client) { Number = r.PhoneNumber.ToString() });
            }

            return new List<VoipPhone>();
        }

        public VoipPhone GetPhone(string phoneSid)
        {
            var phone = IncomingPhoneNumberResource.Fetch(phoneSid, client: client);

            var result = new TwilioPhone(client) { Id = phone.Sid, Number = phone.PhoneNumber.ToString(), Settings = new TwilioVoipSettings() };

            if (phone.VoiceUrl  == null)
            {
                result.Settings.VoiceUrl = result.Settings.Connect(false);
            }

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
                try
                {
                    var call = CallResource.Fetch(result.Id, client: client);
                    if (!call.Price.HasValue || string.IsNullOrEmpty(call.Duration))
                    {
                        count--;
                        Thread.Sleep(10000);
                        continue;
                    }

                    result.Price = (-1)*call.Price.Value;
                    result.DialDuration = Convert.ToInt32(call.Duration);
                    break;
                }
                catch (ApiException)
                {
                    count--;
                    Thread.Sleep(10000);
                }
            }

            return result;
        }

        public string GetToken(Agent agent, int seconds = 60*60*24)
        {
            var scopes = new HashSet<IScope>
            {
                new IncomingClientScope(agent.ClientID)
            };
            var capability = new ClientCapability(accountSid, authToken, scopes: scopes);

            return capability.ToJwt();
        }

        public void UpdateSettings(VoipPhone phone)
        {
            IncomingPhoneNumberResource.Update(phone.Id, voiceUrl: new Uri(phone.Settings.Connect(false)), client: client);
        }

        public void DisablePhone(VoipPhone phone)
        {
            IncomingPhoneNumberResource.Update(phone.Id, voiceUrl: new Uri("https://demo.twilio.com/welcome/voice/"), client: client);
        }

        #endregion
    }

   

    public enum PhoneNumberType
    {
        Local,
        /*            Mobile,*/
        TollFree
    }
}