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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ASC.ZohoApi
{
    public enum ZohoDocumentKind
    {
        Writer,
        Sheet,
        Show
    }

    public class ZohoException:InvalidOperationException
    {
        public int Code { get; set; }

        public ZohoException(int code, string message):base(message)
        {
            Code = code;
        }
    }


    [DebuggerDisplay("{Folder}/{Title} {Childs.Count}")]
    public class ZohoDocument
    {
        public string AuthorName { get; set; }
        public DateTime Published { get; set; }
        public DateTime Updated { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string Folder { get; set; }
        public ZohoDocumentKind Kind { get; set; }
        public List<ZohoDocument> Childs { get; set; }

        internal ZohoDocument()
        {
            Childs = new List<ZohoDocument>();
        }
    }

    public class Zoho
    {
        private readonly string _apiKey;
        private readonly string _zohoId;
        private readonly string _password;

        private const string ZohoLoginUrl =
            "https://accounts.zoho.com/login?FROM_AGENT=true&LOGIN_ID={0}&PASSWORD={1}";

        private const string ZohoDocumentUrl =
            "http://export.writer.zoho.com/api/private/xml/documents";

        private const string ZohoSheetsUrl = "http://sheet.zoho.com/api/private/xml/books";

        private const string ZohoSheetsDownloadUrl = "http://sheet.zoho.com/api/private/{0}/download/{1}";
        private const string ZohoDocumentDownloadUrl = "http://export.writer.zoho.com/api/private/{0}/download/{1}";

        private Dictionary<string,string> _loginParams = new Dictionary<string, string>();
        private Dictionary<ZohoDocumentKind, string> _defFormats = new Dictionary<ZohoDocumentKind, string>();
        private ZohoDocument _doc;

        public Zoho(string apiKey, string zohoId, string password)
        {
            _apiKey = apiKey;
            _zohoId = zohoId;
            _password = password;

            _defFormats.Add(ZohoDocumentKind.Writer, "odt");
            _defFormats.Add(ZohoDocumentKind.Sheet, "sxc");

            var responce = GetClient().DownloadString(GetLoginUrl());
            _loginParams =
                (from paramString in responce.Split('\n')
                where !paramString.StartsWith("#") && !string.IsNullOrEmpty(paramString) && paramString.Contains('=') let split = paramString.Split('=')
                select new KeyValuePair<string, string>(split[0],split[1])).ToDictionary(x=>x.Key,y=>y.Value);
            if (!IsAuthentificated)
            {
                throw new UnauthorizedAccessException("Login failed");
            }
        }

        public ZohoDocument GetDocuments()
        {
            if (_doc == null)
            {
                _doc = new ZohoDocument();
                _doc.Childs.AddRange(DownloadDocumentInfo());
                _doc.Childs.AddRange(DownloadSheetsInfo());
            }
            return _doc;
        }

        public MemoryStream GetDocumentStream(ZohoDocument document)
        {
            return GetDocumentStream(document, null);
        }

        public MemoryStream GetDocumentStream(ZohoDocument document, string ext)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (ext == null)
            {
                ext = _defFormats[document.Kind];
            }
            var urlPattern = ZohoDocumentDownloadUrl;
            if (document.Kind==ZohoDocumentKind.Sheet)
            {
                urlPattern = ZohoSheetsDownloadUrl;
            }
            return new MemoryStream(GetClient().DownloadData(CreateUrl(string.Format(urlPattern, ext, document.Id))));
        }

        private IEnumerable<ZohoDocument> DownloadDocumentInfo()
        {
            var documentData = GetClient().DownloadData(CreateUrl(ZohoDocumentUrl));
            XDocument doc = GetDoc(documentData);
            CheckErrorAndThrow(doc);
            var unix = new DateTime(1970, 1, 1);
            return from d in doc.XPathSelectElements("//document") 
                            select new ZohoDocument()
                                       {
                                           AuthorName = d.Element("authorName").Value,
                                           FileName = d.Element("document_name_url").Value + "." + _defFormats[ZohoDocumentKind.Writer],
                                           Title = d.Element("documentName").Value,
                                           Folder = d.Element("category").Value,
                                           Id = d.Element("documentId").Value,
                                           Published = unix.AddMilliseconds(long.Parse(d.Element("created_date").Value)),
                                           Updated = unix.AddMilliseconds(long.Parse(d.Element("lastModifiedTime").Value)),
                                           Kind = ZohoDocumentKind.Writer
                                       };
        }

        private XDocument GetDoc(byte[] documentData)
        {
            XDocument doc;
            using (var ms = new MemoryStream(documentData))
            using (var reader = XmlReader.Create(ms))
                doc=  XDocument.Load(reader);
            return doc;
        }

        private IEnumerable<ZohoDocument> DownloadSheetsInfo()
        {
            var documentData = GetClient().DownloadData(CreateUrl(ZohoSheetsUrl));
            XDocument doc = GetDoc(documentData);
            CheckErrorAndThrow(doc);
            var unix = new DateTime(1970, 1, 1);
            return from d in doc.XPathSelectElements("//workbook")
                   select new ZohoDocument()
                   {
                       AuthorName = d.Element("ownerName").Value,
                       FileName = d.Element("workbookName").Value + "." + _defFormats[ZohoDocumentKind.Sheet],
                       Title = d.Element("workbookName").Value,
                       Id = d.Element("workbookId").Value,
                       Published = unix.AddMilliseconds(long.Parse(d.Element("createdTime").Value)),
                       Updated = unix.AddMilliseconds(long.Parse(d.Element("lastModifiedTime").Value)),
                       Kind = ZohoDocumentKind.Sheet
                   };
        }

        private void CheckErrorAndThrow(XDocument doc)
        {
            var error = doc.XPathSelectElement("/response/error");
            if (error!=null)
            {

                throw new ZohoException(int.Parse(error.Element("code").Value), error.Element("message").Value);
            }
        }

        private string CreateUrl(string endPointUrl)
        {
            return endPointUrl + GetUrlAuthorization();
        }

        private string GetUrlAuthorization()
        {
            return string.Format("?apikey={0}&ticket={1}", _apiKey, Ticket);
        }

        public string Ticket
        {
            get { return _loginParams["TICKET"]; }
        }

        public bool IsAuthentificated
        {
            get { return bool.Parse(_loginParams["RESULT"]); }
        }

        private string GetLoginUrl()
        {
            return string.Format(ZohoLoginUrl, _zohoId, _password);
        }

        private WebClient GetClient()
        {
            return new WebClient();
        }


    }
}