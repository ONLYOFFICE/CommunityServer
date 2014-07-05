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
using System.Xml.Linq;
using ASC.Files.Core;
using ASC.Thrdparty;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;

namespace ASC.Web.Files.Import.Boxnet
{
    internal class BoxDocumentProvider : IDocumentProvider
    {
        private string _authToken;

        private static string AuthTicket
        {
            get { return TokenHolder.GetToken("box.net_auth_ticket"); }
            set { TokenHolder.AddToken("box.net_auth_ticket", value); }
        }

        public string Name
        {
            get { return "boxnet"; }
        }

        private void GetAuthTicket()
        {
            if (string.IsNullOrEmpty(AuthTicket)) throw new ArgumentException("No auth ticket");
            var url = string.Format("https://www.box.com/api/1.0/rest?action=get_auth_token&api_key={0}&ticket={1}",
                                    ImportConfiguration.BoxNetApiKey, AuthTicket);
            var response = XDocument.Load(url).Element("response");
            if (response == null || response.ElementValueOrDefault("status") != "get_auth_token_ok")
            {
                throw new UnauthorizedAccessException(FilesCommonResource.ErrorMassage_SecurityException_Login);
            }
            _authToken = response.ElementValueOrDefault("auth_token");
        }

        public BoxDocumentProvider(AuthData authData)
        {
            if (string.IsNullOrEmpty(authData.Token))
            {
                //Get auth_ticket
                GetAuthTicket();
            }
            else
            {
                _authToken = authData.Token;
            }
        }

        public IEnumerable<Document> GetDocuments()
        {
            var url =
                string.Format("https://www.box.com/api/1.0/rest?action=get_account_tree&api_key={0}&auth_token={1}&folder_id=0&params[]=nozip",
                              ImportConfiguration.BoxNetApiKey, _authToken);
            var response = XDocument.Load(url).Element("response");

            if (response == null)
            {
                throw new ArgumentException("Empty status responce");
            }

            if (response.Element("status").Value != "listing_ok")
            {
                throw new Exception("Bad listing status: " + response.ElementValueOrDefault("status") + ".");
            }

            var docs = new List<Document>();
            if (response.Element("tree") != null)
            {
                foreach (var f in response.Element("tree").Elements("folder"))
                {
                    RetrieveTree(f, docs);
                }
            }
            return docs;
        }

        public Stream GetDocumentStream(string contentLink, out long size)
        {
            var request = WebRequest.Create("https://www.box.com/api/1.0/download/" + _authToken + "/" + contentLink);
            var responce = request.GetResponse();

            if (responce == null) throw new IOException("Responce stream empty");

            size = responce.ContentLength;
            return responce.GetResponseStream();
        }

        private static void RetrieveTree(XElement root, List<Document> result)
        {
            if (root.Element("folders") != null)
            {
                foreach (var f in root.Element("folders").Elements("folder"))
                {
                    if (f.Attribute("id") != null)
                    {
                        var entry = new Document
                            {
                                Id = f.AttributeValueOrDefault("id"),
                                Title = f.AttributeValueOrDefault("name", "noname"),
                                Parent = root.AttributeValueOrDefault("id"),
                                IsFolder = true,
                            };
                        result.Add(entry);
                    }
                    RetrieveTree(f, result);
                }
            }
            if (root.Element("files") != null)
            {
                result.AddRange(root.Element("files")
                                    .Elements("file")
                                    .Where(f => f.Attribute("id") != null)
                                    .Select(f => new Document
                                        {
                                            Id = f.AttributeValueOrDefault("id"),
                                            ContentLink = f.AttributeValueOrDefault("id"),
                                            Title = f.AttributeValueOrDefault("file_name"),
                                            Parent = root.AttributeValueOrDefault("id"),
                                            CreateOn = Utils.FromUnixTime2(long.Parse(f.AttributeValueOrDefault("created"))),
                                            IsFolder = false,
                                        })
                                    .Where(d => !Global.EnableUploadFilter
                                                || FileUtility.ExtsUploadable.Contains(FileUtility.GetFileExtension(d.Title), StringComparer.CurrentCultureIgnoreCase)));
            }
        }
    }
}