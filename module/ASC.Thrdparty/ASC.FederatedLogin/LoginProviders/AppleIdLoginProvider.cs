/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Web;

namespace ASC.FederatedLogin.LoginProviders
{
    public class AppleIdLoginProvider : BaseLoginProvider<AppleIdLoginProvider>
    {
        public override string AccessTokenUrl { get { return "https://appleid.apple.com/auth/token"; } }
        public override string RedirectUri { get { return this["appleIdRedirectUrl"]; } }
        public override string ClientID { get { return (this["appleIdClientIdMobile"] != null && HttpContext.Current != null && HttpContext.Current.Request.MobileApp()) ? this["appleIdClientIdMobile"] : this["appleIdClientId"]; } }
        public override string ClientSecret => GenerateSecret();
        public override string CodeUrl { get { return "https://appleid.apple.com/auth/authorize"; } }
        public override string Scopes { get { return ""; } }

        public string TeamId { get { return this["appleIdTeamId"]; } }
        public string KeyId { get { return this["appleIdKeyId"]; } }
        public string PrivateKey { get { return this["appleIdPrivateKey"]; } }

        public AppleIdLoginProvider() { }
        public AppleIdLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null) : base(name, order, props, additional) { }

        public override LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            try
            {
                var token = Auth(context, Scopes);
                var claims = ValidateIdToken(JObject.Parse(token.OriginJson).Value<string>("id_token"));
                return GetProfileFromClaims(claims);
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

        public override LoginProfile GetLoginProfile(string authCode)
        {
            if (string.IsNullOrEmpty(authCode))
                throw new Exception("Login failed");

            var token = OAuth20TokenHelper.GetAccessToken<AppleIdLoginProvider>(authCode);
            var claims = ValidateIdToken(JObject.Parse(token.OriginJson).Value<string>("id_token"));
            return GetProfileFromClaims(claims);
        }

        public override LoginProfile GetLoginProfile(OAuth20Token token)
        {
            if (token == null)
                throw new Exception("Login failed");

            var claims = ValidateIdToken(JObject.Parse(token.OriginJson).Value<string>("id_token"));
            return GetProfileFromClaims(claims);
        }

        private LoginProfile GetProfileFromClaims(ClaimsPrincipal claims)
        {
            return new LoginProfile()
            {
                Id = claims.FindFirst(ClaimTypes.NameIdentifier).Value,
                EMail = claims.FindFirst(ClaimTypes.Email)?.Value,
                Provider = ProviderConstants.AppleId,
            };
        }

        private string GenerateSecret()
        {
            using (var cngKey = CngKey.Import(Convert.FromBase64String(PrivateKey), CngKeyBlobFormat.Pkcs8PrivateBlob))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.CreateJwtSecurityToken(
                    issuer: TeamId,
                    audience: "https://appleid.apple.com",
                    subject: new ClaimsIdentity(new List<Claim> { new Claim("sub", ClientID) }),
                    issuedAt: DateTime.UtcNow,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(5),
                    signingCredentials: new SigningCredentials(new ECDsaSecurityKey(new ECDsaCng(cngKey)), SecurityAlgorithms.EcdsaSha256)
                );
                token.Header.Add("kid", KeyId);

                return handler.WriteToken(token);
            }
        }

        private ClaimsPrincipal ValidateIdToken(string idToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var claims = handler.ValidateToken(idToken, new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = GetApplePublicKeys(),

                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",

                ValidateAudience = true,
                ValidAudience = ClientID,

                ValidateLifetime = true

            }, out var _);

            return claims;
        }

        private IEnumerable<SecurityKey> GetApplePublicKeys()
        {
            var appplePublicKeys = RequestHelper.PerformRequest("https://appleid.apple.com/auth/keys");

            var keys = new List<SecurityKey>();
            foreach (var webKey in JObject.Parse(appplePublicKeys).Value<JArray>("keys"))
            {
                var e = Base64UrlEncoder.DecodeBytes(webKey.Value<string>("e"));
                var n = Base64UrlEncoder.DecodeBytes(webKey.Value<string>("n"));

                var key = new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n })
                {
                    KeyId = webKey.Value<string>("kid")
                };

                keys.Add(key);
            }

            return keys;
        }
    }
}