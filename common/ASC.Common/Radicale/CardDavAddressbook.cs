/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Text;
using System.Threading.Tasks;
using System.Web;

using ASC.Common.Logging;
using ASC.Common.Radicale.Core;

namespace ASC.Common.Radicale
{
    public class CardDavAddressbook : RadicaleEntity
    {
        private static readonly ILog Logger = BaseLogManager.GetLogger("ASC.Radicale");

        public readonly string strTemplate = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
             "<mkcol xmlns=\"DAV:\" xmlns:C=\"urn: ietf:params:xml: ns: caldav\" xmlns:CR=\"urn: ietf:params:xml: ns: carddav\" xmlns:I=\"http://apple.com/ns/ical/\" xmlns:INF=\"http://inf-it.com/ns/ab/\">" + "" +
             "<set><prop>" +
             "<resourcetype><collection /><CR:addressbook /></resourcetype>" +
             "<displayname>{0}</displayname>" +
             "<INF:addressbook-color>{1}</INF:addressbook-color>" +
             "<CR:addressbook-description>{2}</CR:addressbook-description>" +
             "</prop></set></mkcol>";

        public async Task<DavResponse> Create(string name, string description, string backgroundColor, string uri, string authorization, bool isReadonly = true)
        {
            var rewriterUri = uri.StartsWith("http") ? uri : "";

            var davRequest = new DavRequest()
            {
                Url = uri,
                Authorization = authorization,
                Header = rewriterUri,
                Data = GetData(strTemplate, name, description, backgroundColor)
            };

            return await RadicaleClient.CreateAsync(davRequest).ConfigureAwait(false);
        }

        public async Task<DavResponse> Update(string name, string description, string backgroundColor, string uri, string userName, string authorization, bool isReadonly = true)
        {
            var addbookId = isReadonly ? readonlyAddBookName : defaultAddBookName;

            var header = uri.StartsWith("http") ? uri : "";

            var requestUrl = defaultRadicaleUrl + "/" + HttpUtility.UrlEncode(userName) + "/" + addbookId;

            var davRequest = new DavRequest()
            {
                Url = requestUrl,
                Authorization = authorization,
                Data = GetData(strTemplate, name, description, backgroundColor),
                Header = header
            };

            return await RadicaleClient.UpdateAsync(davRequest).ConfigureAwait(false);
        }


        public async Task<DavResponse> GetCollection(string url, string authorization, string myUri)
        {
            var path = (new Uri(url).AbsolutePath.StartsWith("/carddav")) ? (new Uri(url).AbsolutePath.Remove(0, 8)) : new Uri(url).AbsolutePath;
            var defaultUrlconn = defaultRadicaleUrl + path;
            var davRequest = new DavRequest()
            {
                Url = defaultUrlconn,
                Authorization = authorization,
                Header = myUri
            };

            return await RadicaleClient.GetAsync(davRequest).ConfigureAwait(false);
        }

        public async Task<DavResponse> UpdateItem(string url, string authorization, string data, string headerUrl = "")
        {
            var path = (new Uri(url).AbsolutePath.StartsWith("/carddav")) ? (new Uri(url).AbsolutePath.Remove(0, 8)) : new Uri(url).AbsolutePath;
            var requrl = defaultRadicaleUrl + path;
            var davRequest = new DavRequest()
            {
                Url = requrl,
                Authorization = authorization,
                Header = headerUrl,
                Data = data
            };

            return await RadicaleClient.UpdateItemAsync(davRequest).ConfigureAwait(false);
        }

        public string GetUserSerialization(CardDavItem user)
        {
            var sex = (user.Sex.HasValue) ? user.Sex.Value ? "M" : "W" : string.Empty;

            var builder = new StringBuilder();

            builder.AppendLine("BEGIN:VCARD");
            builder.AppendLine("UID:" + user.ID.ToString());
            builder.AppendLine("N:" + user.LastName + ";" + user.FirstName);
            builder.AppendLine("FN:" + user.FirstName + " " + user.LastName);
            builder.AppendLine("EMAIL:" + user.Email);
            builder.AppendLine("TEL:" + user.MobilePhone);
            builder.AppendLine($"BDAY:{user.BirthDate:s}");
            builder.AppendLine("TITLE:" + user.Title);
            builder.AppendLine("URL:" + "");
            builder.AppendLine("GENDER:" + sex);
            builder.AppendLine($"REV:{DateTime.Now:s}");
            builder.AppendLine("TZ:" + DateTimeOffset.Now.Offset);
            builder.AppendLine("ORG:");
            builder.AppendLine("END:VCARD");

            return builder.ToString();
        }

        public void Delete(string uri, Guid userID, string email, int tenantId = 0)
        {
            var authorization = GetSystemAuthorization();
            var deleteUrlBook = GetRadicaleUrl(uri, email.ToLower(), true, true);
            var davRequest = new DavRequest()
            {
                Url = deleteUrlBook,
                Authorization = authorization
            };
            try
            {
                RadicaleClient.RemoveAsync(davRequest).ConfigureAwait(false);
                var dbConn = new DbRadicale();
                dbConn.RemoveCardDavUser(tenantId, userID.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error("ERROR: " + ex.Message);
            }
        }

        public void UpdateItemForAllAddBooks(List<string> emailList, string uri, CardDavItem user, int tenantId = 0, string changedEmail = null)
        {

            var authorization = GetSystemAuthorization();
            if (changedEmail != null)
            {
                var deleteUrlBook = GetRadicaleUrl(uri, changedEmail.ToLower(), true, true);
                var davRequest = new DavRequest()
                {
                    Url = deleteUrlBook,
                    Authorization = authorization
                };
                RadicaleClient.RemoveAsync(davRequest).ConfigureAwait(false);

                try
                {
                    var dbConn = new DbRadicale();
                    dbConn.RemoveCardDavUser(tenantId, user.ID.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Error("ERROR: " + ex.Message);
                }
            }

            foreach (string email in emailList)
            {
                try
                {
                    var currentEmail = email.ToLower();
                    var userData = GetUserSerialization(user);
                    var requestUrl = GetRadicaleUrl(uri, currentEmail, true, true, itemID: user.ID.ToString());
                    UpdateItem(requestUrl, authorization, userData, uri).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Error("ERROR: " + ex.Message);
                }
            }
        }
    }
}
