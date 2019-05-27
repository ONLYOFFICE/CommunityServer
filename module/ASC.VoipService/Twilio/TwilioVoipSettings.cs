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
