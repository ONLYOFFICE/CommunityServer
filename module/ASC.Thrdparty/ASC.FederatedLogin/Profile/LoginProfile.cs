/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Script.Serialization;
using ASC.FederatedLogin.Helpers;
using ASC.Security.Cryptography;
using System.Runtime.Serialization;

namespace ASC.FederatedLogin.Profile
{
    [Serializable]
    public class LoginProfile : ISerializable
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

        public string RealmUrl
        {
            get { return GetField(WellKnownFields.RealmUrl); }
            internal set { SetField(WellKnownFields.RealmUrl, value); }
        }

        public string UniqueId
        {
            get { return string.Format("{0}/{1}", Provider, Id); }
        }

        public string HashId
        {
            get { return HashHelper.MD5(UniqueId); }
        }

        public string Hash
        {
            get { return Common.Utils.Signature.Create(HashId); }
        }

        public string Serialized
        {
            get { return Transport(); }
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


        public LoginProfile GetMinimalProfile()
        {
            var profileNew = new LoginProfile();
            profileNew.Provider = Provider;
            profileNew.Id = Id;
            return profileNew;
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

        public static LoginProfile GetProfile()
        {
            return HttpContext.Current != null?GetProfile(HttpContext.Current.Request):new LoginProfile();
        }

        internal static LoginProfile FromError(Exception e)
        {
            var profile = new LoginProfile {AuthorizationError = e.Message};
            return profile;
        }

        public static LoginProfile GetProfile(HttpRequest request)
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
            var key = HashHelper.MD5(Transport());
            HttpRuntime.Cache.Add(key, this, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(15),
                                  CacheItemPriority.High, null);
            return AppendQueryParam(uri, QuerySessionParamName, key);
        }

        internal Uri AppendSessionProfile(Uri uri, HttpContext context)
        {
            //gen key
            var key = HashHelper.MD5(Transport());
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
            else if (!string.IsNullOrEmpty(queryString[QuerySessionParamName]))
            {
                FromTransport((string)HttpContext.Current.Session[queryString[QuerySessionParamName]]);
            }
            else if (!string.IsNullOrEmpty(queryString[QueryCacheParamName]))
            {
                FromTransport((string)HttpRuntime.Cache[queryString[QueryCacheParamName]]);
            }
        }


        internal string ToSerializedString()
        {
            return string.Join(new string(PairSeparator, 1), _fields.Select(x => string.Join(new string(KeyValueSeparator, 1), new[] { x.Key, x.Value })).ToArray());
        }

        internal static LoginProfile CreateFromSerializedString(string serialized)
        {
            var profile = new LoginProfile();
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
            
            return HttpServerUtility.UrlTokenEncode(InstanceCrypto.Encrypt(Encoding.UTF8.GetBytes(ToSerializedString())));
        }

        internal void FromTransport(string transportstring)
        {
            var serialized = Encoding.UTF8.GetString(InstanceCrypto.Decrypt(HttpServerUtility.UrlTokenDecode(transportstring)));
            FromSerializedString(serialized);
        }
      

        internal LoginProfile()
        {
            
        }

        protected LoginProfile(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            var transformed = (string)info.GetValue(QueryParamName, typeof(string));
            FromTransport(transformed);
        }

        public LoginProfile(string transport)
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

        public string ToJson()
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(this);
        }
    }
}