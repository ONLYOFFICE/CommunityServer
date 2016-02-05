/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.IO;
using System.Net;
using System.Text;

namespace ASC.FederatedLogin.Helpers
{
    public class RequestHelper
    {
        public static String PerformRequest(String uri, String contentType = "", String method = "GET", String body = "", Dictionary<string, string> headers = null, int timeout = 30000)
        {
            if (String.IsNullOrEmpty(uri)) throw new ArgumentNullException("uri");

            var request = WebRequest.Create(uri);
            request.Method = method;
            request.Timeout = timeout;
            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    request.Headers.Add(key, headers[key]);
                }
            }


            if (!string.IsNullOrEmpty(contentType))
            {
                request.ContentType = contentType;
            }

            var bytes = Encoding.UTF8.GetBytes(body ?? "");
            if (request.Method != "GET" && bytes.Length > 0)
            {
                request.ContentLength = bytes.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            try
            {
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    return stream != null ? new StreamReader(stream).ReadToEnd() : null;
                }
            }
            catch (WebException)
            {
                request.Abort();
                throw;
            }
        }
    }
}