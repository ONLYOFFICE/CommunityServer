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