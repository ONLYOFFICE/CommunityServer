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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using ASC.Common.Data;
using ASC.Common.Data.Sql;

namespace ASC.Web.Core.Files
{
    public static class FileUtility
    {
        #region method

        public static string GetFileExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return string.Empty;
            string extension = null;
            try
            {
                extension = Path.GetExtension(fileName);
            }
            catch (Exception)
            {
                var position = fileName.LastIndexOf('.');
                if (0 <= position)
                    extension = fileName.Substring(position).Trim().ToLower();
            }
            return extension == null ? string.Empty : extension.Trim().ToLower();
        }

        public static string GetInternalExtension(string fileName)
        {
            var extension = GetFileExtension(fileName);
            string internalExtension;
            return InternalExtension.TryGetValue(GetFileTypeByExtention(extension), out internalExtension)
                       ? internalExtension
                       : extension;
        }

        public static string GetGoogleDownloadableExtension(string googleExtension)
        {
            googleExtension = GetFileExtension(googleExtension);
            if (googleExtension.Equals(".gdraw")) return ".pdf";
            return GetInternalExtension(googleExtension);
        }

        public static string ReplaceFileExtension(string fileName, string newExtension)
        {
            newExtension = string.IsNullOrEmpty(newExtension) ? string.Empty : newExtension;
            return Path.GetFileNameWithoutExtension(fileName) + newExtension;
        }

        public static FileType GetFileTypeByFileName(string fileName)
        {
            return GetFileTypeByExtention(GetFileExtension(fileName));
        }

        public static FileType GetFileTypeByExtention(string extension)
        {
            extension = extension.ToLower();

            if (ExtsDocument.Contains(extension)) return FileType.Document;
            if (ExtsSpreadsheet.Contains(extension)) return FileType.Spreadsheet;
            if (ExtsPresentation.Contains(extension)) return FileType.Presentation;
            if (ExtsImage.Contains(extension)) return FileType.Image;
            if (ExtsArchive.Contains(extension)) return FileType.Archive;
            if (ExtsAudio.Contains(extension)) return FileType.Audio;
            if (ExtsVideo.Contains(extension)) return FileType.Video;

            return FileType.Unknown;
        }

        public static bool CanImageView(string fileName)
        {
            return ExtsImagePreviewed.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool CanMediaView(string fileName)
        {
            return ExtsMediaPreviewed.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool CanWebView(string fileName)
        {
            return ExtsWebPreviewed.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool CanWebEdit(string fileName)
        {
            return ExtsWebEdited.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool CanWebReview(string fileName)
        {
            return ExtsWebReviewed.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool CanWebCustomFilterEditing(string fileName)
        {
            return ExtsWebCustomFilterEditing.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool CanWebRestrictedEditing(string fileName)
        {
            return ExtsWebRestrictedEditing.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool CanWebComment(string fileName)
        {
            return ExtsWebCommented.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool CanCoAuhtoring(string fileName)
        {
            return ExtsCoAuthoring.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool CanIndex(string fileName)
        {
            return extsIndexing.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        #endregion

        #region member

        private static Dictionary<string, List<string>> _extsConvertible;

        public static Dictionary<string, List<string>> ExtsConvertible
        {
            get
            {
                if (_extsConvertible == null)
                {
                    _extsConvertible = new Dictionary<string, List<string>>();
                    if (string.IsNullOrEmpty(FilesLinkUtility.DocServiceConverterUrl)) return _extsConvertible;

                    const string databaseId = "files";
                    const string tableTitle = "files_converts";

                    using (var dbManager = new DbManager(databaseId))
                    {
                        var sqlQuery = new SqlQuery(tableTitle).Select("input", "output");

                        var list = dbManager.ExecuteList(sqlQuery);

                        list.ForEach(item =>
                            {
                                var input = item[0] as string;
                                var output = item[1] as string;
                                if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(output))
                                    return;
                                input = input.ToLower().Trim();
                                output = output.ToLower().Trim();
                                if (!_extsConvertible.ContainsKey(input))
                                    _extsConvertible[input] = new List<string>();
                                _extsConvertible[input].Add(output);
                            });
                    }
                }
                return _extsConvertible;
            }
        }

        private static List<string> _extsUploadable;

        public static List<string> ExtsUploadable
        {
            get
            {
                if (_extsUploadable == null)
                {
                    _extsUploadable = new List<string>();
                    _extsUploadable.AddRange(ExtsWebPreviewed);
                    _extsUploadable.AddRange(ExtsWebEdited);
                    _extsUploadable.AddRange(ExtsImagePreviewed);
                    _extsUploadable = _extsUploadable.Distinct().ToList();
                }
                return _extsUploadable;
            }
        }


        private static readonly List<string> extsImagePreviewed = (ConfigurationManagerExtension.AppSettings["files.viewed-images"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsMediaPreviewed = (ConfigurationManagerExtension.AppSettings["files.viewed-media"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsWebPreviewed = (ConfigurationManagerExtension.AppSettings["files.docservice.viewed-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsWebEdited = (ConfigurationManagerExtension.AppSettings["files.docservice.edited-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsWebEncrypt = (ConfigurationManagerExtension.AppSettings["files.docservice.encrypted-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsWebReviewed = (ConfigurationManagerExtension.AppSettings["files.docservice.reviewed-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsWebCustomFilterEditing = (ConfigurationManagerExtension.AppSettings["files.docservice.customfilter-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsWebRestrictedEditing = (ConfigurationManagerExtension.AppSettings["files.docservice.formfilling-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsWebCommented = (ConfigurationManagerExtension.AppSettings["files.docservice.commented-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsWebTemplate = (ConfigurationManagerExtension.AppSettings["files.docservice.template-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsMustConvert = (ConfigurationManagerExtension.AppSettings["files.docservice.convert-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsCoAuthoring = (ConfigurationManagerExtension.AppSettings["files.docservice.coauthor-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        private static readonly List<string> extsIndexing = (ConfigurationManagerExtension.AppSettings["files.index.formats"] ?? "").Split(new[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        public static List<string> ExtsImagePreviewed
        {
            get { return extsImagePreviewed; }
        }

        public static List<string> ExtsMediaPreviewed
        {
            get { return extsMediaPreviewed; }
        }

        public static List<string> ExtsWebPreviewed
        {
            get { return string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl) ? new List<string>() : extsWebPreviewed; }
        }

        public static List<string> ExtsWebEdited
        {
            get { return string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl) ? new List<string>() : extsWebEdited; }
        }

        public static List<string> ExtsWebEncrypt
        {
            get { return extsWebEncrypt; }
        }

        public static List<string> ExtsWebReviewed
        {
            get { return string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl) ? new List<string>() : extsWebReviewed; }
        }

        public static List<string> ExtsWebCustomFilterEditing
        {
            get { return string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl) ? new List<string>() : extsWebCustomFilterEditing; }
        }

        public static List<string> ExtsWebRestrictedEditing
        {
            get { return string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl) ? new List<string>() : extsWebRestrictedEditing; }
        }

        public static List<string> ExtsWebCommented
        {
            get { return string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl) ? new List<string>() : extsWebCommented; }
        }

        public static List<string> ExtsWebTemplate
        {
            get { return extsWebTemplate; }
        }

        public static List<string> ExtsMustConvert
        {
            get { return string.IsNullOrEmpty(FilesLinkUtility.DocServiceConverterUrl) ? new List<string>() : extsMustConvert; }
        }

        public static List<string> ExtsCoAuthoring
        {
            get { return extsCoAuthoring; }
        }

        public static readonly List<string> ExtsArchive = new List<string>
            {
                ".zip", ".rar", ".ace", ".arc", ".arj",
                ".bh", ".cab", ".enc", ".gz", ".ha",
                ".jar", ".lha", ".lzh", ".pak", ".pk3",
                ".tar", ".tgz", ".gz", ".uu", ".uue", ".xxe",
                ".z", ".zoo"
            };

        public static readonly List<string> ExtsVideo = new List<string>
            {
                ".3gp", ".asf", ".avi", ".f4v",
                ".fla", ".flv", ".m2ts", ".m4v",
                ".mkv", ".mov", ".mp4", ".mpeg",
                ".mpg", ".mts", ".ogv", ".svi",
                ".vob", ".webm", ".wmv"
            };

        public static readonly List<string> ExtsAudio = new List<string>
            {
                ".aac", ".ac3", ".aiff", ".amr",
                ".ape", ".cda", ".flac", ".m4a",
                ".mid", ".mka", ".mp3", ".mpc",
                ".oga", ".ogg", ".pcm", ".ra",
                ".raw", ".wav", ".wma"
            };

        public static readonly List<string> ExtsImage = new List<string>
            {
                ".bmp", ".cod", ".gif", ".ief", ".jpe", ".jpeg", ".jpg",
                ".jfif", ".tiff", ".tif", ".cmx", ".ico", ".pnm", ".pbm",
                ".png", ".ppm", ".rgb", ".svg", ".xbm", ".xpm", ".xwd",
                ".svgt", ".svgy", ".gdraw", ".webp"
            };

        public static readonly List<string> ExtsSpreadsheet = new List<string>
            {
                ".xls", ".xlsx", ".xlsm",
                ".xlt", ".xltx", ".xltm",
                ".ods", ".fods", ".ots", ".csv",
                ".xlst", ".xlsy", ".xlsb",
                ".gsheet"
            };

        public static readonly List<string> ExtsPresentation = new List<string>
            {
                ".pps", ".ppsx", ".ppsm",
                ".ppt", ".pptx", ".pptm",
                ".pot", ".potx", ".potm",
                ".odp", ".fodp", ".otp",
                ".pptt", ".ppty",
                ".gslides"
            };

        public static readonly List<string> ExtsDocument = new List<string>
            {
                ".doc", ".docx", ".docm",
                ".dot", ".dotx", ".dotm",
                ".odt", ".fodt", ".ott", ".rtf", ".txt",
                ".html", ".htm", ".mht",
                ".pdf", ".djvu", ".fb2", ".epub", ".xps",
                ".doct", ".docy",
                ".gdoc"
            };

        public static readonly List<string> ExtsTemplate = new List<string>
            {
                ".ott", ".ots", ".otp",
                ".dot", ".dotm", ".dotx",
                ".xlt", ".xltm", ".xltx",
                ".pot", ".potm", ".potx",
            };

        public static readonly Dictionary<FileType, string> InternalExtension = new Dictionary<FileType, string>
            {
                { FileType.Document, ConfigurationManagerExtension.AppSettings["files.docservice.internal-doc"] ?? ".docx" },
                { FileType.Spreadsheet, ConfigurationManagerExtension.AppSettings["files.docservice.internal-xls"] ?? ".xlsx" },
                { FileType.Presentation, ConfigurationManagerExtension.AppSettings["files.docservice.internal-ppt"] ?? ".pptx" }
            };

        public enum CsvDelimiter
        {
            None = 0,
            Tab = 1,
            Semicolon = 2,
            Colon = 3,
            Comma = 4,
            Space = 5
        }

        public static readonly string SignatureSecret = GetSignatureSecret();
        public static readonly string SignatureHeader = GetSignatureHeader();

        private static string GetSignatureSecret()
        {
            var result = ConfigurationManagerExtension.AppSettings["files.docservice.secret"] ?? "";

            var regex = new Regex(@"^\s+$");

            if (regex.IsMatch(result))
                result = "";

            return result;
        }

        private static string GetSignatureHeader()
        {
            var result = (ConfigurationManagerExtension.AppSettings["files.docservice.secret.header"] ?? "").Trim();
            if (string.IsNullOrEmpty(result))
                result = "Authorization";
            return result;
        }

        public static readonly bool CanForcesave = GetCanForcesave();

        private static bool GetCanForcesave()
        {
            bool canForcesave;
            return !bool.TryParse(ConfigurationManagerExtension.AppSettings["files.docservice.forcesave"] ?? "", out canForcesave) || canForcesave;
        }

        #endregion
    }
}