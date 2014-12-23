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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.Combiner.Utils;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using OpenIdAuth.OAuth.Facebook;
using OpenIdAuth.Utils;
using System.Runtime.Serialization;

namespace OpenIdAuth.Profile
{
    public static class UniversalProfileExtensions
    {
        public static Uri AddProfile(this Uri uri, UniversalProfile profile)
        {
            return profile.AppendProfile(uri);
        }
        public static Uri AddProfileSession(this Uri uri, UniversalProfile profile, HttpContext context)
        {
            return profile.AppendSessionProfile(uri, context);
        }

        public static Uri AddProfileCache(this Uri uri, UniversalProfile profile)
        {
            return profile.AppendCacheProfile(uri);
        }

        public static UniversalProfile GetProfile(this Uri uri)
        {
            var profile = new UniversalProfile();
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            if (!string.IsNullOrEmpty(queryString[UniversalProfile.QuerySessionParamName]) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                return (UniversalProfile)HttpContext.Current.Session[queryString[UniversalProfile.QuerySessionParamName]];
            }
            if (!string.IsNullOrEmpty(queryString[UniversalProfile.QueryParamName]))
            {
                profile.ParseFromUrl(uri);
            }
            if (!string.IsNullOrEmpty(queryString[UniversalProfile.QueryCacheParamName]))
            {
                return (UniversalProfile)HttpRuntime.Cache.Get(queryString[UniversalProfile.QuerySessionParamName]);
            }
            return profile;
        }

        public static bool HasProfile(this Uri uri)
        {
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            return !string.IsNullOrEmpty(queryString[UniversalProfile.QueryParamName]) || !string.IsNullOrEmpty(queryString[UniversalProfile.QuerySessionParamName]) || !string.IsNullOrEmpty(queryString[UniversalProfile.QueryCacheParamName]);
        }


    }

    public class WellKnownFields
    {
        //Constants
        public const string Id = "Id";
        public const string Link = "L";
        public const string Name = "N";
        public const string DisplayName = "D";
        public const string Email = "E";
        public const string Avatar = "A";
        public const string Gender = "G";
        public const string FirstName = "FN";
        public const string LastName = "LN";
        public const string MiddleName = "MN";
        public const string Salutation = "S";
        public const string BirthDay = "BD";
        public const string Locale = "LOC";
        public const string Timezone = "TZ";
        public const string Auth = "AR";
        public const string AuthError = "AeR";
        public const string Provider = "APr";
    }

    [Serializable]
    public class UniversalProfile:ISerializable
    {
        private IDictionary<string, string> _fields = new Dictionary<string, string>();

        public string Id
        {
            get { return GetField(WellKnownFields.Id); }
            internal set { SetField(WellKnownFields.Id, value); }
        }

        public string Link
        {
            get { return GetField(WellKnownFields.Link); }
            internal set { SetField(WellKnownFields.Link, value); }
        }

        public string Name
        {
            get { return GetField(WellKnownFields.Name); }
            internal set { SetField(WellKnownFields.Name, value); }
        }

        public string DisplayName
        {
            get { return GetField(WellKnownFields.DisplayName); }
            internal set { SetField(WellKnownFields.DisplayName, value); }
        }

        public string EMail
        {
            get { return GetField(WellKnownFields.Email); }
            internal set { SetField(WellKnownFields.Email, value); }
        }

        public string Avatar
        {
            get { return GetField(WellKnownFields.Avatar); }
            internal set { SetField(WellKnownFields.Avatar, value); }
        }

        public string Gender
        {
            get { return GetField(WellKnownFields.Gender); }
            internal set { SetField(WellKnownFields.Gender, value); }
        }


        public string FirstName
        {
            get { return GetField(WellKnownFields.FirstName); }
            internal set { SetField(WellKnownFields.FirstName, value); }
        }

        public string LastName
        {
            get { return GetField(WellKnownFields.LastName); }
            internal set { SetField(WellKnownFields.LastName, value); }
        }

        public string MiddleName
        {
            get { return GetField(WellKnownFields.MiddleName); }
            internal set { SetField(WellKnownFields.MiddleName, value); }
        }

        public string Salutation
        {
            get { return GetField(WellKnownFields.Salutation); }
            internal set { SetField(WellKnownFields.Salutation, value); }
        }

        public string BirthDay
        {
            get { return GetField(WellKnownFields.BirthDay); }
            internal set { SetField(WellKnownFields.BirthDay, value); }
        }

        public string Locale
        {
            get { return GetField(WellKnownFields.Locale); }
            internal set { SetField(WellKnownFields.Locale, value); }
        }

        public string TimeZone
        {
            get { return GetField(WellKnownFields.Timezone); }
            internal set { SetField(WellKnownFields.Timezone, value); }
        }

        public string AuthorizationResult
        {
            get { return GetField(WellKnownFields.Auth); }
            internal set { SetField(WellKnownFields.Auth, value); }
        }

        public string AuthorizationError
        {
            get { return GetField(WellKnownFields.AuthError); }
            internal set { SetField(WellKnownFields.AuthError, value); }
        }

        public string Provider
        {
            get { return GetField(WellKnownFields.Provider); }
            internal set { SetField(WellKnownFields.Provider, value); }
        }

        public string UniqueId
        {
            get { return string.Format("{0}/{1}", Provider, Id); }
        }

        public string HashId
        {
            get { return Hash.MD5(UniqueId); }
        }

        public string UserDisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(DisplayName))
                {
                    return DisplayName;
                }
                var combinedName = string.Join(" ",
                                   new[] { FirstName, MiddleName, LastName }.Where(
                                       x => !string.IsNullOrEmpty(x)).ToArray());
                if (string.IsNullOrEmpty(combinedName))
                {
                    combinedName = Name;
                }
                return combinedName;
            }
        }

        public bool IsFailed
        {
            get { return !string.IsNullOrEmpty(AuthorizationError); }
        }

        public bool IsAuthorized
        {
            get { return !IsFailed; }
        }

        internal string GetField(string name)
        {
            return _fields.ContainsKey(name) ? _fields[name] : string.Empty;
        }



        internal void SetField(string name, string value)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (!string.IsNullOrEmpty(value))
            {
                if (_fields.ContainsKey(name))
                {
                    _fields[name] = value;
                }
                else
                {
                    _fields.Add(name, value);
                }
            }
            else
            {
                if (_fields.ContainsKey(name))
                {
                    _fields.Remove(name);
                }
            }
        }

        public const string QueryParamName = "up";
        public const string QuerySessionParamName = "sup";
        public const string QueryCacheParamName = "cup";
        private const char KeyValueSeparator = '→';
        private const char PairSeparator = '·';

        internal Uri AppendProfile(Uri uri)
        {
            var value = Transport();
            return AppendQueryParam(uri, QueryParamName, value);
        }

        public static bool HasProfile()
        {
            return HttpContext.Current != null && HasProfile(HttpContext.Current.Request);
        }

        public static bool HasProfile(HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            return request.Url.HasProfile();
        }

        public static UniversalProfile GetProfile()
        {
            return HttpContext.Current != null?GetProfile(HttpContext.Current.Request):new UniversalProfile();
        }

        internal static UniversalProfile FromError(Exception e)
        {
            var profile = new UniversalProfile {AuthorizationError = e.Message};
            return profile;
        }

        public static UniversalProfile GetProfile(HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            return request.Url.GetProfile();
        }

        private static Uri AppendQueryParam(Uri uri, string keyvalue, string value)
        {
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            if (!string.IsNullOrEmpty(queryString[keyvalue]))
            {
                queryString[keyvalue] = value;
            }
            else
            {
                queryString.Add(keyvalue, value);
            }
            var query = new StringBuilder();
            foreach (var key in queryString.AllKeys)
            {
                query.AppendFormat("{0}={1}&", key,
                                   queryString[key]);
            }
            var builder = new UriBuilder(uri) { Query = query.ToString() };
            return builder.Uri;
        }

        internal Uri AppendCacheProfile(Uri uri)
        {
            //gen key
            var key = Hash.MD5(Transport());
            HttpRuntime.Cache.Add(key, this, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(15),
                                  CacheItemPriority.High, null);
            return AppendQueryParam(uri, QuerySessionParamName, key);
        }

        internal Uri AppendSessionProfile(Uri uri, HttpContext context)
        {
            //gen key
            var key = Hash.MD5(Transport());
            context.Session[key] = this;
            return AppendQueryParam(uri, QuerySessionParamName, key);
        }


        internal void ParseFromUrl(Uri uri)
        {
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            if (!string.IsNullOrEmpty(queryString[QueryParamName]))
            {
                FromTransport(queryString[QueryParamName]);
            }
        }


        internal string ToSerializedString()
        {
            return string.Join(new string(PairSeparator, 1), _fields.Select(x => string.Join(new string(KeyValueSeparator, 1), new[] { x.Key, x.Value })).ToArray());
        }

        internal static UniversalProfile CreateFromSerializedString(string serialized)
        {
            var profile = new UniversalProfile();
            profile.FromSerializedString(serialized);
            return profile;
        }

        internal void FromSerializedString(string serialized)
        {
            if (serialized == null) throw new ArgumentNullException("serialized");
            _fields = serialized.Split(PairSeparator).ToDictionary((x) => x.Split(KeyValueSeparator)[0], (y) => y.Split(KeyValueSeparator)[1]);
        }

        internal string Transport()
        {
            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(ToSerializedString()));
        }

        internal void FromTransport(string transportstring)
        {
            var serialized = Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(transportstring));
            FromSerializedString(serialized);
        }

        internal static UniversalProfile ProfileFromTwitter(XDocument info)
        {
            XPathNavigator nav = info.CreateNavigator();
            var profile = new UniversalProfile
            {
                Name = nav.SelectNodeValue("//screen_name"),
                DisplayName = nav.SelectNodeValue("//name"),
                Avatar = nav.SelectNodeValue("//profile_image_url"),
                TimeZone = nav.SelectNodeValue("//time_zone"),
                Locale = nav.SelectNodeValue("//lang"),
                Id = nav.SelectNodeValue("//id"),
                Link = nav.SelectNodeValue("//url"),
                Provider = "twitter"
            };
            return profile;
        }

        internal static UniversalProfile ProfileFromFacebook(FacebookGraph graph)
        {
            var profile = new UniversalProfile
            {
                BirthDay = graph.Birthday,
                Link = graph.Link.ToString(),
                FirstName = graph.FirstName,
                LastName = graph.LastName,
                Gender = graph.Gender,
                DisplayName = graph.FirstName + graph.LastName,
                EMail = graph.Email,
                Id = graph.Id.ToString(),
                TimeZone = graph.Timezone,
                Locale = graph.Locale,
                Provider = "facebook"
            };

            return profile;
        }

        internal static UniversalProfile ProfileFromOpenId(ClaimsResponse spprofile, FetchResponse fetchprofile, string claimedId)
        {
            var profile = new UniversalProfile
                              {
                                  Link = claimedId,
                                  Id = claimedId,
                                  Provider = "openid",
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
            return profile;
        }

        internal UniversalProfile()
        {
            
        }

        protected UniversalProfile(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            var transformed = (string)info.GetValue(QueryParamName, typeof(string));
            FromTransport(transformed);
        }

        public UniversalProfile(string transport)
        {
            FromTransport(transport);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue(QueryParamName, Transport());
        }

        
        
    }
}