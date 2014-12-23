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
using System.Text;
using System.Web;
using ASC.Core;
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
            var result =  GetEcho("connect", user);
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
            return new TwilioResponseHelper(this, CoreContext.TenantManager.GetCurrentTenant().TenantAlias).GetEcho(method, user);
        }
    }
}
