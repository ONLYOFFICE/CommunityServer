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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Configuration;
using System.Xml.XPath;
using ASC.Mail.Aggregator.Common.Logging;

namespace ASC.Mail.Aggregator.Common
{
    public class FilesUploader
    {
        private static readonly string BaseUrl;

        static FilesUploader()
        {
            BaseUrl = WebConfigurationManager.AppSettings["api.url"].Trim('~', '/');
        }

        public static int UploadToFiles(Stream file, string filename, string content_type, string folder_id,
                                        Guid user_id, ILogger log = null)
        {
            return UploadToFiles(file, filename, content_type, folder_id, user_id, true, log);
        }

        public static int UploadToFiles(Stream file, string filename, string content_type, string folder_id, Guid user_id, bool create_new_if_exist, ILogger log = null)
        {
            var post_parameters = new Dictionary<string, object>
            {
                {"storeOriginalFileFlag", true},
                {"createNewIfExist", create_new_if_exist}
            };

            var request_uri_builder = GetUploadToDocumnetsUrl(folder_id);
            if (log != null) log.Debug("UploadToFiles -> request_uri_builder.Uri = {0}, Host = {1}", request_uri_builder.Uri, request_uri_builder.Host);
            var auth_cookie = ApiHelper.GetAuthCookie(user_id, request_uri_builder.Host);
            if (log != null) log.Debug("UploadToFiles -> auth_cookie = {0}", auth_cookie.ToString());

            post_parameters.Add("file", new FormUpload.FileParameter(file, filename, content_type));
            var response = FormUpload.MultipartFormDataPost(request_uri_builder.Uri.ToString(), "", post_parameters, auth_cookie);
            if (log != null) log.Debug("UploadToFiles -> response StatusCode = {0}", response != null ? response.StatusCode.ToString() : "Response is Null.");

            var uploaded_file_id = ParseUploadResponse(response);
            return uploaded_file_id;
        }

        private static int ParseUploadResponse(WebResponse responce)
        {
            if (responce != null)
            {
                var responce_stream = responce.GetResponseStream();
                if (responce_stream != null)
                {
                    var xdoc = new XPathDocument(responce_stream);
                    var navigator = xdoc.CreateNavigator();
                    var res = navigator.SelectSingleNode("/result/response/id");
                    if (res != null)
                    {
                        return res.ValueAsInt;
                    }
                }
            }
            return -1;
        }

        private static UriBuilder GetUploadToDocumnetsUrl(string folder_id)
        {
            var upload_url = String.Format("{0}/files/{1}/upload", BaseUrl, folder_id);
            return ApiHelper.GetApiRequestUrl(upload_url);
        }
    }
}
