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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using ASC.Web.Files.Classes;

namespace ASC.Web.Files.ThirdPartyApp
{
    public class RequestHelper
    {
        public static String PerformRequest(String uri, String contentType = "", String method = "", String queryString = "", Dictionary<string, string> headers = null)
        {
            if (String.IsNullOrEmpty(uri))
            {
                Global.Logger.Error("ThirdPartyApp: request with empty url");
                return null;
            }

            if (String.IsNullOrEmpty(method))
                method = "GET";

            byte[] bytes = null;
            if (!String.IsNullOrEmpty(queryString))
                bytes = Encoding.UTF8.GetBytes(queryString);

            var request = WebRequest.Create(uri);
            request.Method = method;
            request.Timeout = 5000;
            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    request.Headers.Add(key, headers[key]);
                }
            }

            if (request.Method != "GET" && !String.IsNullOrEmpty(contentType) && bytes != null)
            {
                request.ContentType = contentType;
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
            catch (WebException e)
            {
                Global.Logger.Error("ThirdPartyApp: WebException - ", e);
                request.Abort();
            }

            return null;
        }
    }
}