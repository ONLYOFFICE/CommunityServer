/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using log4net;
using ASC.Web.Studio.Utility;
using ASC.Web.Core;
using System.Collections.Generic;
using ASC.Web.Studio.Core;
using System.Web;

namespace ASC.Web.CRM.Classes
{
    class AsyncRequestSender
    {
        public string RequestUrl { get; set; }
        public string RequestMethod { get; set; }
        public string RequestBody { get; set; }

        public void BeginGetResponse()
        {
            var request = (HttpWebRequest)WebRequest.Create(CommonLinkUtility.GetFullAbsolutePath(RequestUrl));

            request.Headers.Add("Authorization", CookiesManager.GetCookies(CookiesType.AuthKey));
            request.Method = RequestMethod;
            request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = RequestBody.Length;

            var getRequestStream = request.BeginGetRequestStream(null, null);
            var writer = new StreamWriter(request.EndGetRequestStream(getRequestStream));
            writer.Write(RequestBody);
            writer.Close();

            request.BeginGetResponse(OnAsyncCallback, request);
        }

        private static void OnAsyncCallback(IAsyncResult asyncResult)
        {
            //var httpWebRequest = (HttpWebRequest)asyncResult.AsyncState;
            //var response = httpWebRequest.EndGetResponse(asyncResult);
            //var stream = response.GetResponseStream();
            //if (stream != null)
            //{
            //    var reader = new StreamReader(stream);
            //    var responseString = reader.ReadToEnd();
            //}
        }

    }

    public class MailAggregatorManager
    {
        public void UpdateMailAggregator(IEnumerable<string> emails, IEnumerable<Guid> userIds)
        {
            var apiServer = new Api.ApiServer();

            var body = GetPostBody(emails, userIds);

            apiServer.GetApiResponse(
                String.Format("{0}mail/messages/update_crm.json", SetupInfo.WebApiBaseUrl),
                "POST",
                body);
        }

        public void UpdateMailAggregatorAsync(IEnumerable<string> emails, IEnumerable<Guid> userIds)
        {
            var sender = new AsyncRequestSender
            {
                RequestUrl = String.Format("{0}mail/messages/update_crm.json", SetupInfo.WebApiBaseUrl),
                RequestMethod = "POST",
                RequestBody = GetPostBody(emails, userIds)
            };

            sender.BeginGetResponse();
        }

        private string GetPostBody(IEnumerable<string> emails, IEnumerable<Guid> userIds)
        {
            var sb = new StringBuilder();

            var itemFormat = HttpUtility.UrlEncode("emails[]") + "={0}&";
            foreach (var data in emails)
            {
                sb.Append(String.Format(itemFormat, HttpUtility.UrlEncode(data)));
            }

            itemFormat = HttpUtility.UrlEncode("userIds[]") + "={0}&";
            foreach (var data in userIds)
            {
                sb.Append(String.Format(itemFormat, HttpUtility.UrlEncode(data.ToString())));
            }

            return sb.ToString();
        }
    }
}
