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
using System.Linq;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using DotNetOpenAuth.ApplicationBlock;
using DotNetOpenAuth.OAuth;

namespace ASC.Web.Files.Import.Google
{
    internal class GoogleDocumentProvider : IDocumentProvider, IDisposable
    {
        private readonly string _accessToken;
        private readonly ConsumerBase _consumer;

        private readonly string[] _googleSupportedFormats = new[]
                                                                {
                                                                    ".doc", ".html", ".jpeg", ".odt", ".pdf", ".png",
                                                                    ".rtf", ".svg", ".txt", ".zip",
                                                                    ".ppt", ".swf", ".xls", ".csv", ".ods", ".tsv"
                                                                };

        private readonly IEqualityComparer<string> _comparer = StringComparer.CurrentCultureIgnoreCase;

        public string Name
        {
            get { return "google"; }
        }

        public GoogleDocumentProvider(AuthData authData)
        {
            _accessToken = authData.Token;
            _consumer = new WebConsumer(GoogleConsumer.ServiceDescription, ImportConfiguration.GoogleTokenManager);
        }

        public IEnumerable<Document> GetDocuments()
        {
            var result = new List<Document>();
            GetDocuments(null, result);
            return result;
        }

        public Stream GetDocumentStream(string contentLink, out long size)
        {
            return GoogleConsumer.GetDoc(_consumer, _accessToken, contentLink, out size);
        }

        public void Dispose()
        {
            if (_consumer != null) _consumer.Dispose();
        }

        private void GetDocuments(string endpoint, List<Document> result)
        {
            const string ns = "{http://www.w3.org/2005/Atom}";
            var root = GoogleConsumer.GetDocList(_consumer, _accessToken, endpoint).Root;
            if (root == null)
                return;

            foreach (var element in root.Elements(ns + "entry"))
            {
                var entry = new Document();
                var kind = element.Elements(ns + "category")
                    .Where(c => c.AttributeValueOrDefault("scheme") == "http://schemas.google.com/g/2005#kind")
                    .Select(c => c.AttributeValueOrDefault("label"))
                    .FirstOrDefault();

                entry.IsFolder = kind == "folder";
                entry.Title = element.ElementValueOrDefault(ns + "title", "unnamed");
                if (!entry.IsFolder) entry.Title += GetExtension(kind, entry.Title);

                if (!entry.IsFolder && Global.EnableUploadFilter && !FileUtility.ExtsUploadable.Contains(FileUtility.GetFileExtension(entry.Title), _comparer))
                    continue;

                entry.Id = GetId(element.ElementValueOrDefault(ns + "id"));
                if (!string.IsNullOrEmpty(entry.Id) && element.Element(ns + "content") != null)
                {
                    entry.ContentLink = element.Element(ns + "content").AttributeValueOrDefault("src") +
                                        GetExport(entry.Title);
                    entry.CreateBy = element.Element(ns + "author") != null
                                         ? element.Element(ns + "author").ElementValueOrDefault(ns + "name", "noname")
                                         : "noname";

                    entry.Parent = element.Elements(ns + "link")
                        .Where(
                            c =>
                            c.Attribute("rel") != null &&
                            c.AttributeValueOrDefault("rel") == "http://schemas.google.com/docs/2007#parent")
                        .Select(c => GetId(c.AttributeValueOrDefault("href")))
                        .OrderBy(c => c)
                        .FirstOrDefault();

                    var published = element.ElementValueOrDefault(ns + "published");
                    DateTime date;
                    if (DateTime.TryParse(published, out date)) entry.CreateOn = date;

                    result.Add(entry);
                }
            }

            var next = root.Elements(ns + "link")
                .Where(e => e.Attribute("rel") != null && e.AttributeValueOrDefault("rel") == "next")
                .Select(e => e.AttributeValueOrDefault("href"))
                .FirstOrDefault();

            if (next != null)
            {
                GetDocuments(next, result);
            }
        }

        private string GetExtension(string kind, string title)
        {
            var ext = FileUtility.GetFileExtension(title);
            if (_googleSupportedFormats.Contains(ext, _comparer)) return string.Empty; //keep current
            switch (kind)
            {
                case "document":
                    return ".docx";
                case "spreadsheet":
                    return ".xlsx";
                case "presentation":
                    return ".pptx";
                case "drawing":
                    return ".svg";
                default:
                    return string.Empty;
            }
        }

        private string GetExport(string title)
        {
            if (string.IsNullOrEmpty(title)) return string.Empty;

            const string export = "&exportFormat=";
            var ext = FileUtility.GetFileExtension(title);

            if (_googleSupportedFormats.Contains(ext)) return export + ext.TrimStart('.');

            var fileType = FileUtility.GetFileTypeByExtention(ext);

            switch (fileType)
            {
                case FileType.Document:
                    return export + "docx&format=docx"; //NOTE: http://code.google.com/apis/documents/docs/3.0/developers_guide_protocol.html#DownloadingDocs
                case FileType.Spreadsheet:
                    return export + "xlsx";
                case FileType.Presentation:
                    return export + "pptx";
                case FileType.Image:
                    return export + "svg";
                default:
                    return string.Empty;
            }
        }

        private string GetId(string href)
        {
            if (string.IsNullOrEmpty(href)) return string.Empty;
            var pos = href.LastIndexOf("%3A", StringComparison.InvariantCultureIgnoreCase);
            if (0 <= pos)
            {
                href = href.Substring(pos + 3);
                pos = href.IndexOf("?");
                return 0 <= pos ? href.Substring(0, pos) : href;
            }
            return string.Empty;
        }
    }
}