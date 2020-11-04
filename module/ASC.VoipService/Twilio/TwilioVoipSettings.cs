/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Text;
using System.Web;
using ASC.Core.Common;
using Uri = System.Uri;

namespace ASC.VoipService.Twilio
{
    public class TwilioVoipSettings : VoipSettings
    {
        public TwilioVoipSettings() { }

        public TwilioVoipSettings(Uri voiceUrl) 
        {
            if (string.IsNullOrEmpty(voiceUrl.Query)) return;

            JsonSettings = Encoding.UTF8.GetString(Convert.FromBase64String(HttpUtility.UrlDecode(HttpUtility.ParseQueryString(voiceUrl.Query)["settings"])));
        }

        public TwilioVoipSettings(string settings) : base(settings)
        {
        }

        public override string Connect(bool user = true, string contactId = null)
        {
            var result =  GetEcho("", user);
            if (!string.IsNullOrEmpty(contactId))
            {
                result += "&ContactId=" + contactId;
            }
            return result;
        }

        public override string Redirect(string to)
        {
            return GetEcho("redirect") + "&RedirectTo=" + to;
        }

        public override string Dequeue(bool reject)
        {
            return GetEcho("dequeue") + "&Reject=" + reject;
        }

        private string GetEcho(string method, bool user = true)
        {
            return new TwilioResponseHelper(this, BaseCommonLinkUtility.GetFullAbsolutePath("")).GetEcho(method, user);
        }
    }
}
