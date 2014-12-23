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
using System.Linq;
using System.Web.Configuration;
using ASC.Common.Data;
using ASC.Common.Data.Sql;

namespace ASC.Web.Core.Files
{
    public static class FileUtility
    {
        #region method

        public static string GetFileExtension(string fileName)
        {
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

        public static bool CanWebView(string fileName)
        {
            return ExtsWebPreviewed.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool CanWebEdit(string fileName)
        {
            return ExtsWebEdited.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool CanCoAuhtoring(string fileName)
        {
            return ExtsCoAuthoring.Contains(GetFileExtension(fileName), StringComparer.CurrentCultureIgnoreCase);
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

        public static List<string> ExtsImagePreviewed
        {
            get { return (WebConfigurationManager.AppSettings["files.viewed-images"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        public static List<string> ExtsWebPreviewed
        {
            get
            {
                return
                    string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl) ?
                        new List<string>()
                        : (WebConfigurationManager.AppSettings["files.docservice.viewed-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        public static List<string> ExtsWebEdited
        {
            get
            {
                return
                    string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl) ?
                        new List<string>()
                        : (WebConfigurationManager.AppSettings["files.docservice.edited-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        public static List<string> ExtsMustConvert
        {
            get
            {
                return
                    string.IsNullOrEmpty(FilesLinkUtility.DocServiceConverterUrl)
                        ? new List<string>()
                        : (WebConfigurationManager.AppSettings["files.docservice.convert-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        public static List<string> ExtsCoAuthoring
        {
            get { return (WebConfigurationManager.AppSettings["files.docservice.coauthor-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        public static List<string> ExtsNewService
        {
            get
            {
                return
                    string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrlNew) ?
                        new List<string>()
                        : (WebConfigurationManager.AppSettings["files.docservice.new"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        public static readonly List<string> ExtsArchive = new List<string>
            {
                ".zip", ".rar", ".ace", ".arc", ".arj",
                ".bh", ".cab", ".enc", ".gz", ".ha",
                ".jar", ".lha", ".lzh", ".pak", ".pk3",
                ".tar", ".tgz", ".uu", ".uue", ".xxe",
                ".z", ".zoo"
            };

        public static readonly List<string> ExtsVideo = new List<string>
            {
                ".avi", ".mpg", ".mkv", ".mp4",
                ".mov", ".3gp", ".vob", ".m2ts"
            };

        public static readonly List<string> ExtsAudio = new List<string>
            {
                ".wav", ".mp3", ".wma", ".ogg"
            };

        public static readonly List<string> ExtsImage = new List<string>
            {
                ".bmp", ".cod", ".gif", ".ief", ".jpe", ".jpeg", ".jpg",
                ".jfif", ".tiff", ".tif", ".cmx", ".ico", ".pnm", ".pbm",
                ".png", ".ppm", ".rgb", ".svg", ".xbm", ".xpm", ".xwd",
                ".svgt", ".svgy"
            };

        public static readonly List<string> ExtsSpreadsheet = new List<string>
            {
                ".xls", ".xlsx",
                ".ods", ".csv",
                ".xlst", ".xlsy",
                ".gsheet"
            };

        public static readonly List<string> ExtsPresentation = new List<string>
            {
                ".pps", ".ppsx",
                ".ppt", ".pptx",
                ".odp",
                ".pptt", ".ppty",
                ".gslides"
            };

        public static readonly List<string> ExtsDocument = new List<string>
            {
                ".docx", ".doc", ".odt", ".rtf", ".txt",
                ".html", ".htm", ".mht", ".pdf", ".djvu",
                ".fb2", ".epub", ".xps",
                ".doct", ".docy",
                ".gdoc"
            };

        public static readonly Dictionary<FileType, string> InternalExtension = new Dictionary<FileType, string>
            {
                { FileType.Document, WebConfigurationManager.AppSettings["files.docservice.internal-doc"] ?? ".docx" },
                { FileType.Spreadsheet, WebConfigurationManager.AppSettings["files.docservice.internal-xls"] ?? ".xlsx" },
                { FileType.Presentation, WebConfigurationManager.AppSettings["files.docservice.internal-ppt"] ?? ".pptx" }
            };

        public enum CsvDelimiter
        {
            None = 0,
            Tab = 1,
            Semicolon = 2,
            Сolon = 3,
            Comma = 4,
            Space = 5
        }

        #endregion
    }
}