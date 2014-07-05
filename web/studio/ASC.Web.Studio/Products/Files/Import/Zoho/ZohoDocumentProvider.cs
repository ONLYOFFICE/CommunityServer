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
using System.Net;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;

namespace ASC.Web.Files.Import.Zoho
{
    internal class ZohoDocumentProvider : IDocumentProvider
    {
        private readonly string _authTokenWriter;
        private readonly string _authTokenSheet;
        private readonly string _authTokenShow;

        public string Name
        {
            get { return "zoho"; }
        }

        private const string Scheme = "https"; //Fix for may 7 2011.

        public ZohoDocumentProvider(AuthData authData)
        {
            _authTokenWriter = GetAuthToken("ZohoWriter/DataAPI", authData.Login, authData.Password);
            _authTokenSheet = GetAuthToken("ZohoSheet/DataAPI", authData.Login, authData.Password);
            _authTokenShow = GetAuthToken("ZohoShow/DataAPI", authData.Login, authData.Password);
        }

        public Stream GetDocumentStream(string contentLink, out long size)
        {
            var data = contentLink.Split('?');
            if (data.Length < 2) throw new ArgumentException(FilesCommonResource.ErrorMassage_BadRequest);

            string url;
            var docType = (FileType)Enum.Parse(typeof(FileType), data[0]);
            switch (docType)
            {
                case FileType.Document:
                    url = Scheme + "://exportwriter.zoho.com/api/private/docx/download/{0}?apikey={1}&scope=DataAPI&authtoken=" + _authTokenWriter;
                    break;
                case FileType.Spreadsheet:
                    url = Scheme + "://sheet.zoho.com/api/private/xlsx/download/{0}?apikey={1}&scope=DataAPI&authtoken=" + _authTokenSheet;
                    break;
                case FileType.Presentation:
                    url = Scheme + "://show.zoho.com/api/private/ppt/download/{0}?apikey={1}&scope=DataAPI&authtoken=" + _authTokenShow;
                    break;
                default:
                    throw new ArgumentException(FilesCommonResource.ErrorMassage_BadRequest);
            }

            var documentId = data[1];
            url = string.Format(url, documentId, ImportConfiguration.ZohoApiKey);

            var request = (HttpWebRequest)WebRequest.Create(url);

            //hack for zoho. If UserAgent = null, requsert return error_code=1833 'Server Error' - version is null
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:16.0) Gecko/20100101 Firefox/16.0";
            //taken by the current version of FireFox

            var responce = request.GetResponse();

            if (responce == null) throw new IOException("Responce stream empty");

            size = responce.ContentLength;
            return responce.GetResponseStream();
        }

        public IEnumerable<Document> GetDocuments()
        {
            return GetWriters()
                .Concat(GetSheets())
                .Concat(GetShows());
        }

        private IEnumerable<Document> GetWriters()
        {
            try
            {
                var url = string.Format(Scheme + "://exportwriter.zoho.com/api/private/xml/documents?apikey={0}&authtoken={1}&scope=DataAPI",
                                        ImportConfiguration.ZohoApiKey, _authTokenWriter);
                var documents = XDocument.Load(url);
                CheckErrorAndThrow(documents);

                return documents
                    .XPathSelectElements("//document")
                    .Where(d => d.Element("documentId") != null)
                    .Select(d =>
                            new Document
                                {
                                    Id = d.Element("documentId").Value,
                                    ContentLink = FileType.Document + "?" + d.Element("documentId").Value,
                                    Title = d.ElementValueOrDefault("documentName") + ".docx",
                                    CreateBy = d.ElementValueOrDefault("authorName"),
                                    CreateOn = Utils.FromUnixTime(long.Parse(d.ElementValueOrDefault("created_date"))),
                                    IsFolder = false,
                                })
                    .Where(d => !Global.EnableUploadFilter
                                || FileUtility.ExtsUploadable.Contains(FileUtility.GetFileExtension(d.Title), StringComparer.CurrentCultureIgnoreCase));
            }
            catch
            {
                return Enumerable.Empty<Document>();
            }
        }

        private IEnumerable<Document> GetSheets()
        {
            try
            {
                var url = string.Format(Scheme + "://sheet.zoho.com/api/private/xml/books?apikey={0}&authtoken={1}&scope=DataAPI",
                                        ImportConfiguration.ZohoApiKey, _authTokenSheet);
                var documents = XDocument.Load(url);
                CheckErrorAndThrow(documents);

                return documents
                    .XPathSelectElements("//workbook")
                    .Where(d => d.Element("workbookId") != null)
                    .Select(d =>
                            new Document
                                {
                                    Id = d.Element("workbookId").Value,
                                    ContentLink = FileType.Spreadsheet + "?" + d.Element("workbookId").Value,
                                    Title = d.ElementValueOrDefault("workbookName") + ".xlsx",
                                    CreateBy = d.ElementValueOrDefault("ownerName"),
                                    CreateOn = Utils.FromUnixTime(long.Parse(d.ElementValueOrDefault("createdTime"))),
                                    IsFolder = false
                                })
                    .Where(d => !Global.EnableUploadFilter
                                || FileUtility.ExtsUploadable.Contains(FileUtility.GetFileExtension(d.Title), StringComparer.CurrentCultureIgnoreCase));
            }
            catch
            {
                return Enumerable.Empty<Document>();
            }
        }

        private IEnumerable<Document> GetShows()
        {
            try
            {
                var url = string.Format(Scheme + "://show.zoho.com/api/private/xml/presentations?apikey={0}&authtoken={1}&scope=DataAPI",
                                        ImportConfiguration.ZohoApiKey, _authTokenShow);
                var documents = XDocument.Load(url);
                CheckErrorAndThrow(documents);

                return documents
                    .XPathSelectElements("//presentation")
                    .Where(d => d.Element("presentationId") != null)
                    .Select(d =>
                            new Document
                                {
                                    Id = d.Element("presentationId").Value,
                                    ContentLink = FileType.Presentation + "?" + d.Element("presentationId").Value,
                                    Title = d.ElementValueOrDefault("presentationName") + ".ppt",
                                    CreateBy = d.ElementValueOrDefault("ownerName"),
                                    CreateOn = Utils.FromUnixTime(long.Parse(d.ElementValueOrDefault("createdTime"))),
                                    IsFolder = false
                                })
                    .Where(d => !Global.EnableUploadFilter
                                || FileUtility.ExtsUploadable.Contains(FileUtility.GetFileExtension(d.Title), StringComparer.CurrentCultureIgnoreCase));
            }
            catch
            {
                return Enumerable.Empty<Document>();
            }
        }

        private static string GetAuthToken(string scope, string login, string password)
        {
            var address = string.Format("https://accounts.zoho.com/apiauthtoken/nb/create?SCOPE={0}&EMAIL_ID={1}&PASSWORD={2}",
                                        scope,
                                        login,
                                        password);

            var request = WebRequest.Create(address);
            request.Method = "POST";
            request.Timeout = 120000;
            request.ContentLength = 0;

            using (var response = request.GetResponse().GetResponseStream())
            {
                if (response == null)
                    throw new HttpException((int)HttpStatusCode.BadRequest, FilesCommonResource.ErrorMassage_BadRequest);

                var responseFromServer = new StreamReader(response).ReadToEnd();

                var loginInfo = responseFromServer
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => s.Contains("=") && !s.StartsWith("#"))
                    .ToDictionary(s => s.Split('=')[0], s => s.Split('=')[1]);

                if (loginInfo["RESULT"] == "FALSE") throw new UnauthorizedAccessException(FilesCommonResource.ErrorMassage_SecurityException_Login);
                return loginInfo["AUTHTOKEN"];
            }
        }

        private static void CheckErrorAndThrow(XDocument document)
        {
            var err = document.XPathSelectElement("/response/error");
            if (err != null)
            {
                var errorCode = Convert.ToInt32(err.ElementValueOrDefault("code"));

                // Internal Server Error. Occurs if lists are empty. Bug is zoho sheets https://writer.zoho.com/public/help/sheet.Errors/noband. 
                var skipErrorCodes = new[] { 2500, 2501, 2502, 2503 };

                if (skipErrorCodes.Contains(errorCode)) return;

                throw new Exception(string.Format("Code: {0}. {1}",
                                                  errorCode,
                                                  err.ElementValueOrDefault("message", "not specified")));
            }
        }
    }
}