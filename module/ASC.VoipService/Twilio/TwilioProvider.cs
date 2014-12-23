/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Linq;
using System.Collections.Generic;
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
            this.authToken = authToken;
            this.accountSid = accountSid;

            client = new TwilioRestClient(accountSid, authToken);
        }

        #region Call

        public string GetRecord(string callId)
        {
            var result = client.ListRecordings(callId, null, null, null);
            ThrowIfError(result);

            var record = result.Recordings.Find(r => r.AccountSid == accountSid);
            return record != null ? record.Uri.ToString() : "";
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

            return new VoipPhone {Id = newNumber.Sid, Number = phoneNumber.Substring(1)};
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
            return result.IncomingPhoneNumbers.Select(r => new VoipPhone {Id = r.Sid, Number = r.PhoneNumber});
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
            return result.AvailablePhoneNumbers.Select(r => new VoipPhone { Number = r.PhoneNumber });
        }

        public TwilioPhone GetPhone(string phoneSid)
        {
            var phone = client.GetIncomingPhoneNumber(phoneSid);
            ThrowIfError(phone);

            return new TwilioPhone(client) {Id = phone.Sid, Number = phone.PhoneNumber, Settings = new TwilioVoipSettings(new Uri(phone.VoiceUrl))};
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
            var result = client.UpdateIncomingPhoneNumber(phone.Id, new PhoneNumberOptions { VoiceUrl = null, VoiceApplicationSid = null });
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