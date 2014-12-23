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
using System.Web;
using ASC.FederatedLogin.Profile;
using ASC.Thrdparty;
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