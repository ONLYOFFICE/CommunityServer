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

using System.Text.RegularExpressions;
using System.Web;

namespace ASC.Web.Studio.Controls.FileUploader.HttpModule
{
    internal class ContentDispositionInfo
    {
        internal string name;
        internal string filename;

        internal bool IsFile
        {
            get { return !string.IsNullOrEmpty(filename); }
        }
    }

    internal static class UploadProgressUtils
    {

        private const string PostProtocol = "POST";
        private const string GetVerbName = "GET";

        private static Regex _getUploadId = new Regex(@"(?<=GetUploadProgress\.ashx\?__UixdId=)[^&]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _isMultiPartFormData = new Regex(@"^multipart\/form-data", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _getBoundary = new Regex(@"(?<=boundary=)[^;]+", RegexOptions.Compiled | RegexOptions.Compiled);
        private static readonly Regex _getContentDisposition = new Regex(@"(?<=Content-Disposition\:\s)[^$]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _getContentDispositionName = new Regex(@"(?<=;\s*name\s*="")[^""]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _getContentDispositionFileName = new Regex(@"(?<=;\s*filename\s*="")[^""]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static bool IsUploadStatusRequest(HttpWorkerRequest request, out string id)
        {
            id = string.Empty;

            if (request.GetHttpVerbName() != GetVerbName)
                return false;

            Match m = _getUploadId.Match(request.GetRawUrl());

            if (!m.Success)
                return false;

            id = m.Value;

            return true;
        }


        internal static ContentDispositionInfo GetContentDisposition(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            Match m = _getContentDisposition.Match(value);
            if (!m.Success)
                return null;

            string val = m.Value;

            ContentDispositionInfo cdi = new ContentDispositionInfo();

            m = _getContentDispositionName.Match(val);
            if (m.Success)
                cdi.name = m.Value;


            m = _getContentDispositionFileName.Match(val);
            if (m.Success)
                cdi.filename =GetFileName(m.Value);

            return cdi;
        }

        private static string GetFileName(string path)
        {
            var name = path ?? "";
            var ind = name.LastIndexOf('\\');
            if (ind != -1)
                return name.Substring(ind+1);

            return name;
        }

        internal static bool IsMultiPartFormData(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            return _isMultiPartFormData.IsMatch(contentType);
        }

        internal static bool HasBoundary(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            return _getBoundary.IsMatch(contentType);
        }

        internal static string GetBoundary(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return string.Empty;

            Match m = _getBoundary.Match(contentType);
            return !m.Success ? string.Empty : m.Value;
        }

        internal static bool IsPost(HttpWorkerRequest request)
        {
            return request.GetHttpVerbName() == PostProtocol;
        }

        internal static bool IsUpload(HttpWorkerRequest request)
        {
            string contentType = request.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentType);
            return IsPost(request) && IsMultiPartFormData(contentType) && HasBoundary(contentType);
        }

    }
}