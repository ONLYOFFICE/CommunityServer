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
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using JWT;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class GosUslugiLoginProvider : BaseLoginProvider<GosUslugiLoginProvider>
    {
        public string BaseDomain
        {
            get { return this["gosUslugiDomain"]; }
        }

        public override string CodeUrl
        {
            get { return BaseDomain + "/aas/oauth2/ac"; }
        }

        public override string AccessTokenUrl
        {
            get { return BaseDomain + "/aas/oauth2/te"; }
        }

        public override string ClientID
        {
            get { return this["gosUslugiClientId"]; }
        }

        public override string ClientSecret
        {
            get { return this["gosUslugiCert"]; }
        }

        public override string RedirectUri
        {
            get { return this["gosUslugiRedirectUrl"]; }
        }

        public override string Scopes
        {
            get { return "fullname birthdate gender email"; }
        }

        private string GosUslugiProfileUrl
        {
            get { return BaseDomain + "/rs/prns/"; }
        }

        public GosUslugiLoginProvider()
        {
        }

        public GosUslugiLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }


        public override LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            try
            {
                var token = Auth(context, Scopes);

                if (token == null)
                {
                    throw new Exception("Login failed");
                }

                return GetLoginProfile(token.AccessToken);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return LoginProfile.FromError(ex);
            }
        }

        protected override OAuth20Token Auth(HttpContext context, string scopes, Dictionary<string, string> additionalArgs = null)
        {
            var error = context.Request["error"];
            if (!string.IsNullOrEmpty(error))
            {
                if (error == "access_denied")
                {
                    error = "Canceled at provider";
                }
                throw new Exception(error);
            }

            var code = context.Request["code"];
            if (string.IsNullOrEmpty(code))
            {
                RequestCode(HttpContext.Current, scopes);
                return null;
            }
            var state = context.Request["state"];
            return GetAccessToken(state, code);
        }

        private void RequestCode(HttpContext context, string scope = null)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy.MM.dd HH:mm:ss +0000");
            var state = Guid.NewGuid().ToString();//HttpContext.Current.Request.GetUrlRewriter().AbsoluteUri;

            var msg = scope + timestamp + ClientID + state;
            var encodedSignature = SignMsg(msg);
            var clientSecret = HttpServerUtility.UrlTokenEncode(encodedSignature);

            var requestParams = new Dictionary<string, string>
                {
                    { "client_id", ClientID },
                    { "client_secret", clientSecret },
                    { "redirect_uri", RedirectUri },
                    { "scope", scope },
                    { "response_type", "code" },
                    { "state", state },
                    { "timestamp", timestamp },
                    { "access_type", "online" },
                    { "display", "popup" }
                };
            var requestQuery = string.Join("&", requestParams.Select(pair => pair.Key + "=" + HttpUtility.UrlEncode(pair.Value)));//.Replace("+", "%2b");

            var redURL = CodeUrl + "?" + requestQuery;
            context.Response.Redirect(redURL, true);
        }

        private OAuth20Token GetAccessToken(string state, string code)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy.MM.dd HH:mm:ss +0000");

            var msg = Scopes + timestamp + ClientID + state;
            var encodedSignature = SignMsg(msg);
            var clientSecret = HttpServerUtility.UrlTokenEncode(encodedSignature);

            var requestParams = new Dictionary<string, string>
                {
                    { "client_id", ClientID },
                    { "code", code },
                    { "grant_type", "authorization_code" },
                    { "client_secret", clientSecret },
                    { "state", state },
                    { "redirect_uri", RedirectUri },
                    { "scope", Scopes },
                    { "timestamp", timestamp },
                    { "token_type", "Bearer" }
                };
            var requestQuery = string.Join("&", requestParams.Select(pair => pair.Key + "=" + HttpUtility.UrlEncode(pair.Value)));

            var result = RequestHelper.PerformRequest(AccessTokenUrl, "application/x-www-form-urlencoded", "POST", requestQuery);

            return OAuth20Token.FromJson(result);
        }

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            var tokenPayloadString = JsonWebToken.Decode(accessToken, string.Empty, false);
            var tokenPayload = JObject.Parse(tokenPayloadString);
            if (tokenPayload == null)
            {
                throw new Exception("Payload is incorrect");
            }
            var oid = tokenPayload.Value<string>("urn:esia:sbj_id");

            var userInfoString = RequestHelper.PerformRequest(GosUslugiProfileUrl + oid, "application/x-www-form-urlencoded", headers: new Dictionary<string, string> { { "Authorization", "Bearer " + accessToken } });
            var userInfo = JObject.Parse(userInfoString);
            if (userInfo == null)
            {
                throw new Exception("userinfo is incorrect");
            }
            
            var profile = new LoginProfile
                {
                    Id = oid,
                    FirstName = userInfo.Value<string>("firstName"),
                    LastName = userInfo.Value<string>("lastName"),

                    Provider = ProviderConstants.GosUslugi,
                };

            var userContactsString = RequestHelper.PerformRequest(GosUslugiProfileUrl + oid + "/ctts", "application/x-www-form-urlencoded", headers: new Dictionary<string, string> { { "Authorization", "Bearer " + accessToken } });
            var userContacts = JObject.Parse(userContactsString);
            if (userContacts == null)
            {
                throw new Exception("usercontacts is incorrect");
            }

            var contactElements = userContacts.Value<JArray>("elements");
            if (contactElements == null)
            {
                throw new Exception("usercontacts elements is incorrect");
            }

            foreach (var contactElement in contactElements.ToObject<List<string>>())
            {
                var userContactString = RequestHelper.PerformRequest(contactElement, "application/x-www-form-urlencoded", headers: new Dictionary<string, string> { { "Authorization", "Bearer " + accessToken } });

                var userContact = JObject.Parse(userContactString);
                if (userContact == null)
                {
                    throw new Exception("usercontacts is incorrect");
                }

                var type = userContact.Value<string>("type");
                if (type != "EML") continue;

                profile.EMail = userContact.Value<string>("value");
                break;
            }

            return profile;
        }

        private X509Certificate2 GetSignerCert()
        {
            var storeMy = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            storeMy.Open(OpenFlags.ReadOnly);
            var certColl = storeMy.Certificates.Find(X509FindType.FindBySubjectKeyIdentifier, ClientSecret, false);
            storeMy.Close();
            if (certColl.Count == 0)
            {
                throw new Exception("Certificate not found");
            }
            return certColl[0];
        }

        private byte[] SignMsg(string msg)
        {
            var signerCert = GetSignerCert();

            var msgBytes = Encoding.UTF8.GetBytes(msg);
            var contentInfo = new ContentInfo(msgBytes);
            var signedCms = new SignedCms(contentInfo, true);
            var cmsSigner = new CmsSigner(signerCert);
            signedCms.ComputeSignature(cmsSigner);
            return signedCms.Encode();
        }

        //private static bool VerifyMsg(Byte[] msg, byte[] encodedSignature)
        //{
        //    ContentInfo contentInfo = new ContentInfo(msg);
        //    SignedCms signedCms = new SignedCms(contentInfo, true);
        //    signedCms.Decode(encodedSignature);

        //    try
        //    {
        //        signedCms.CheckSignature(true);
        //    }
        //    catch (System.Security.Cryptography.CryptographicException e)
        //    {
        //        return false;
        //    }

        //    return true;
        //}
    }
}