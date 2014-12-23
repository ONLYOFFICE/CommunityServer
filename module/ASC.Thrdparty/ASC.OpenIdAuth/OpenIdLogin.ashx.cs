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
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.SessionState;
using System.Xml.Linq;
using System.Xml.XPath;
using DotNetOpenAuth.ApplicationBlock;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.Extensions.UI;
using DotNetOpenAuth.OpenId.RelyingParty;
using OpenIdAuth.OAuth.Facebook;
using OpenIdAuth.Profile;
using OpenIdAuth.Utils;

namespace OpenIdAuth
{
    /// <summary>
    /// Summary description
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class OpenIdLogin : IHttpHandler, IRequiresSessionState
    {
        private static readonly OpenIdRelyingParty Openid = new OpenIdRelyingParty();

        public void ProcessRequest(HttpContext context)
        {
            var returnUrl = context.Request["returnurl"]??FormsAuthentication.LoginUrl;
            var auth = context.Request["auth"];
            if (!string.IsNullOrEmpty(auth))
            {
                try
                {
                    switch (auth)
                    {
                        case "twitter":
                            DoTwitterLogin(context, returnUrl);
                            break;
                        case "facebook":
                            DoFacebookLogin(context, returnUrl);
                            break;
                        default:
                            DoOpenIdLogin(context, returnUrl);
                            break;
                    }
                }
                catch (ThreadAbortException)
                {
                    //Thats is responce ending
                }
                catch (Exception ex)
                {
                    RedirectToReturnUrl(context, returnUrl, UniversalProfile.FromError(ex));
                }
            }
            else
            {
                //Render xrds
                var xrdsloginuri = new Uri(context.Request.GetUrlRewriter(),
                                           new Uri(context.Request.GetUrlRewriter().AbsolutePath, UriKind.Relative)).ToString() + "?auth=openid&returnurl=" + returnUrl;
                var xrdsimageuri = new Uri(context.Request.GetUrlRewriter(),
                                           new Uri(context.Request.ApplicationPath, UriKind.Relative)).ToString()+"openid.gif";
                XrdsHelper.RenderXrds(context.Response,xrdsloginuri,xrdsimageuri);
            }
        }

        private void DoFacebookLogin(HttpContext context, string returnUrl)
        {
            var client = new FacebookClient
            {
                ClientIdentifier = ConfigurationManager.AppSettings["facebookAppID"],
                ClientSecret = ConfigurationManager.AppSettings["facebookAppSecret"],
            };
            IAuthorizationState authorization = client.ProcessUserAuthorization(null);
            if (authorization == null)
            {
                // Kick off authorization request
                var scope = new List<string>()
                                {
                                    "email,user_about_me",
                                };
                client.RequestUserAuthorization(scope,null,null);
            }
            else
            {
                var request = WebRequest.Create("https://graph.facebook.com/me?access_token=" + Uri.EscapeDataString(authorization.AccessToken));
                using (var response = request.GetResponse())
                {
                    if (response != null)
                        using (var responseStream = response.GetResponseStream())
                        {
                            var graph = FacebookGraph.Deserialize(responseStream);
                            var profile = UniversalProfile.ProfileFromFacebook(graph);
                            RedirectToReturnUrl(context, returnUrl, profile);
                        }
                }
            }
        }

        

        private void DoTwitterLogin(HttpContext context, string returnUrl)
        {
            if (TwitterConsumer.IsTwitterConsumerConfigured)
            {
                var token = context.Request["oauth_token"];
                if (string.IsNullOrEmpty(token))
                {
                    var request = TwitterConsumer.StartSignInWithTwitter(false);
                    request.Send();
                }
                else
                {
                    string screenName;
                    int userId;
                    string accessToken;
                    if (TwitterConsumer.TryFinishSignInWithTwitter(out screenName, out userId, out accessToken))
                    {
                        //Sucess. Get information
                        var info = TwitterConsumer.GetUserInfo(userId, accessToken);
                        var profile = UniversalProfile.ProfileFromTwitter(info);
                        
                        RedirectToReturnUrl(context, returnUrl, profile);
                    }
                    else
                    {
                        RedirectToReturnUrl(context, returnUrl, UniversalProfile.FromError(new Exception("Login failed")));
                    }
                }
            }
        }

        private static void RedirectToReturnUrl(HttpContext context, string returnUrl, UniversalProfile profile)
        {
            if (context.Session != null)
            {
                //Store in session
                context.Response.Redirect(new Uri(returnUrl, UriKind.Absolute).AddProfileSession(profile,context).ToString(), true);
            }
            else if (HttpRuntime.Cache!=null)
            {
                context.Response.Redirect(new Uri(returnUrl, UriKind.Absolute).AddProfileCache(profile).ToString(), true);
            }
            else
            {
                context.Response.Redirect(new Uri(returnUrl, UriKind.Absolute).AddProfile(profile).ToString(), true);
            }
        }

        private void DoOpenIdLogin(HttpContext context, string returnUrl)
        {
            var response = Openid.GetResponse();
            if (response == null)
            {
                Identifier id;
                if (Identifier.TryParse(context.Request["openid_identifier"], out id))
                {
                    try
                    {
                        var request = Openid.CreateRequest(context.Request["openid_identifier"]);
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
                        //fetch.Attributes.AddRequired("http://schema.openid.net/contact/email");//Add two more
                        //fetch.Attributes.AddRequired("http://openid.net/schema/contact/email");
                        fetch.Attributes.AddRequired(WellKnownAttributes.Name.Alias);
                        fetch.Attributes.AddRequired(WellKnownAttributes.Name.First);
                        fetch.Attributes.AddRequired(WellKnownAttributes.Name.Last);
                        fetch.Attributes.AddRequired(WellKnownAttributes.Name.Middle);
                        fetch.Attributes.AddRequired(WellKnownAttributes.Person.Gender);
                        fetch.Attributes.AddRequired(WellKnownAttributes.BirthDate.WholeBirthDate);
                        fetch.Attributes.AddRequired(WellKnownAttributes.Media.Images.Default);

                        request.AddExtension(fetch);
                        request.RedirectToProvider();

                    }
                    catch (ProtocolException ex)
                    {
                        RedirectToReturnUrl(context, returnUrl, UniversalProfile.FromError(ex));
                    }
                }
                else
                {
                    RedirectToReturnUrl(context, returnUrl, UniversalProfile.FromError(new Exception("invalid OpenID identifier")));
                }
            }
            else
            {
                // Stage 3: OpenID Provider sending assertion response
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        var spprofile = response.GetExtension<ClaimsResponse>();
                        var fetchprofile = response.GetExtension <FetchResponse>();
                        var profile = UniversalProfile.ProfileFromOpenId(spprofile, fetchprofile, response.ClaimedIdentifier.ToString());
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            RedirectToReturnUrl(context,returnUrl,profile);
                        }
                        break;
                    case AuthenticationStatus.Canceled:
                        RedirectToReturnUrl(context, returnUrl, UniversalProfile.FromError(new Exception("Canceled at provider")));
                        break;
                    case AuthenticationStatus.Failed:
                        RedirectToReturnUrl(context, returnUrl, UniversalProfile.FromError(response.Exception));
                        break;
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
