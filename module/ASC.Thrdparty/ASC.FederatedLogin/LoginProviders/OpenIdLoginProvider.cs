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
using System.Web;
using ASC.FederatedLogin.Profile;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace ASC.FederatedLogin.LoginProviders
{
    class OpenIdLoginProvider : ILoginProvider
    {
        private static readonly OpenIdRelyingParty Openid = new OpenIdRelyingParty();


        public LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            var response = Openid.GetResponse();
            if (response == null)
            {
                Identifier id;
                if (Identifier.TryParse(@params["oid"], out id))
                {
                    try
                    {
                        IAuthenticationRequest request;

                        var realmUrlString = String.Empty;

                        if (@params.ContainsKey("realmUrl"))
                            realmUrlString = @params["realmUrl"];

                        if (!String.IsNullOrEmpty(realmUrlString))
                            request = Openid.CreateRequest(id, new Realm(realmUrlString));
                        else
                            request = Openid.CreateRequest(id);

                        request.AddExtension(new ClaimsRequest
                        {
                            Email = DemandLevel.Require,
                            Nickname = DemandLevel.Require,
                            Country = DemandLevel.Request,
                            Gender = DemandLevel.Request,
                            PostalCode = DemandLevel.Request,
                            TimeZone = DemandLevel.Request,
                            FullName = DemandLevel.Request,
                                                        
                                                       
                        });
                        var fetch = new FetchRequest();
                        fetch.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
                        //Duplicating attributes
                        fetch.Attributes.AddRequired("http://schema.openid.net/contact/email");//Add two more
                        fetch.Attributes.AddRequired("http://openid.net/schema/contact/email");
                        fetch.Attributes.AddRequired(WellKnownAttributes.Name.Alias);
                        fetch.Attributes.AddRequired(WellKnownAttributes.Name.First);
                        fetch.Attributes.AddRequired(WellKnownAttributes.Media.Images.Default);
                        fetch.Attributes.AddRequired(WellKnownAttributes.Name.Last);
                        fetch.Attributes.AddRequired(WellKnownAttributes.Name.Middle);
                        fetch.Attributes.AddRequired(WellKnownAttributes.Person.Gender);
                        fetch.Attributes.AddRequired(WellKnownAttributes.BirthDate.WholeBirthDate);
                        request.AddExtension(fetch);
                        request.RedirectToProvider();
                        context.Response.End();//This will throw thread abort

                    }
                    catch (ProtocolException ex)
                    {
                       return LoginProfile.FromError(ex);
                    }
                }
                else
                {
                    return LoginProfile.FromError(new Exception("invalid OpenID identifier"));
                }
            }
            else
            {
                // Stage 3: OpenID Provider sending assertion response
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        var spprofile = response.GetExtension<ClaimsResponse>();
                        var fetchprofile = response.GetExtension<FetchResponse>();

                        var realmUrlString = String.Empty;
                        if (@params.ContainsKey("realmUrl"))
                            realmUrlString = @params["realmUrl"];

                        var profile = ProfileFromOpenId(spprofile, fetchprofile, response.ClaimedIdentifier.ToString(), realmUrlString);
                        return profile;
                    case AuthenticationStatus.Canceled:
                        return LoginProfile.FromError(new Exception("Canceled at provider"));
                    case AuthenticationStatus.Failed:
                        return LoginProfile.FromError(response.Exception);
                }
            }
            return null;
        }

        public LoginProfile GetLoginProfile(string accessToken)
        {
            throw new NotImplementedException();
        }

        public string Scopes { get { return ""; } }
        public string CodeUrl { get { return ""; } }
        public string AccessTokenUrl { get { return ""; } }
        public string RedirectUri { get { return ""; } }
        public string ClientID { get { return ""; } }
        public string ClientSecret { get { return ""; } }
        public bool IsEnabled { get { return GoogleLoginProvider.Instance.IsEnabled; } }

        internal static LoginProfile ProfileFromOpenId(ClaimsResponse spprofile, FetchResponse fetchprofile, string claimedId, string realmUrlString)
        {
            var profile = new LoginProfile
            {
                Link = claimedId,
                Id = claimedId,
                Provider = ProviderConstants.OpenId,
            };
            if (spprofile != null)
            {
                //Fill
                profile.BirthDay = spprofile.BirthDateRaw;
                profile.DisplayName = spprofile.FullName;
                profile.EMail = spprofile.Email;
                profile.Name = spprofile.Nickname;
                profile.Gender = spprofile.Gender.HasValue ? spprofile.Gender.Value.ToString() : "";
                profile.TimeZone = spprofile.TimeZone;
                profile.Locale = spprofile.Language;
            }
            if (fetchprofile != null)
            {
                profile.Name = fetchprofile.GetAttributeValue(WellKnownAttributes.Name.Alias);
                profile.LastName = fetchprofile.GetAttributeValue(WellKnownAttributes.Name.Last);
                profile.FirstName = fetchprofile.GetAttributeValue(WellKnownAttributes.Name.First);
                profile.DisplayName = fetchprofile.GetAttributeValue(WellKnownAttributes.Name.FullName);
                profile.MiddleName = fetchprofile.GetAttributeValue(WellKnownAttributes.Name.Middle);
                profile.Salutation = fetchprofile.GetAttributeValue(WellKnownAttributes.Name.Prefix);
                profile.Avatar = fetchprofile.GetAttributeValue(WellKnownAttributes.Media.Images.Default);
                profile.EMail = fetchprofile.GetAttributeValue(WellKnownAttributes.Contact.Email);
                profile.Gender = fetchprofile.GetAttributeValue(WellKnownAttributes.Person.Gender);
                profile.BirthDay = fetchprofile.GetAttributeValue(WellKnownAttributes.BirthDate.WholeBirthDate);
            }
            profile.RealmUrl = realmUrlString;
            return profile;
        }
    }
}