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
using System.Globalization;
using System.Net;

namespace ASC.Core.Tenants
{
    [Serializable]
    public class Tenant
    {
        public const int DEFAULT_TENANT = -1;

        public static readonly string HostName = Dns.GetHostName().ToLowerInvariant();

        public Tenant()
        {
            TenantId = DEFAULT_TENANT;
            TimeZone = TimeZoneInfo.Utc;
            Language = CultureInfo.CurrentCulture.Name;
            TrustedDomains = new List<string>();
            TrustedDomainsType = TenantTrustedDomainsType.None;
            CreatedDateTime = DateTime.UtcNow;
            Status = TenantStatus.Active;
            StatusChangeDate = DateTime.UtcNow;
            VersionChanged = DateTime.UtcNow;
            Industry = TenantIndustry.Other;
        }

        public Tenant(string alias)
            : this()
        {
            TenantAlias = alias.ToLowerInvariant();
        }

        public Tenant(int id, string alias)
            : this(alias)
        {
            TenantId = id;
        }

        ///<example type="int">1</example>
        public int TenantId { get; internal set; }

        ///<example>TenantAlias</example>
        public string TenantAlias { get; set; }

        ///<example>MappedDomain</example>
        public string MappedDomain { get; set; }

        ///<example type="int">1</example>
        public int Version { get; set; }

        ///<example>2019-07-26T00:00:00</example>
        public DateTime VersionChanged { get; set; }

        ///<example>TenantDomain</example>
        public string TenantDomain { get { return GetTenantDomain(); } }

        ///<example>HostedRegion</example>
        public string HostedRegion { get; set; }

        ///<example>Name</example>
        public string Name { get; set; }

        ///<example>Language</example>
        public string Language { get; set; }

        ///<example>TimeZone</example>
        public TimeZoneInfo TimeZone { get; set; }

        ///<example>TrustedDomains</example>
        ///<collection>list</collection>
        public List<string> TrustedDomains { get; private set; }

        ///<example type="int">1</example>
        public TenantTrustedDomainsType TrustedDomainsType { get; set; }

        ///<example>38c0f464-f1e7-493e-8d95-dc4ee8ee834a</example>
        public Guid OwnerId { get; set; }

        ///<example>2019-07-26T00:00:00</example>
        public DateTime CreatedDateTime { get; internal set; }

        ///<example>ru-RU</example>
        public CultureInfo GetCulture() { return !string.IsNullOrEmpty(Language) ? CultureInfo.GetCultureInfo(Language) : CultureInfo.CurrentCulture; }

        ///<example>2019-07-26T00:00:00</example>
        public DateTime LastModified { get; set; }

        ///<example type="int">1</example>
        public TenantStatus Status { get; internal set; }

        ///<example>2019-07-26T00:00:00</example>
        public DateTime StatusChangeDate { get; internal set; }

        ///<example>PartnerId</example>
        public string PartnerId { get; set; }

        ///<example>AffiliateId</example>
        public string AffiliateId { get; set; }

        ///<example>Campaign</example>
        public string Campaign { get; set; }

        ///<example>PaymentId</example>
        public string PaymentId { get; set; }

        ///<example type="int">1</example>
        public TenantIndustry Industry { get; set; }

        ///<example>true</example>
        public bool Spam { get; set; }

        ///<example>true</example>
        public bool Calls { get; set; }

        public void SetStatus(TenantStatus status)
        {
            Status = status;
            StatusChangeDate = DateTime.UtcNow;
        }


        public override bool Equals(object obj)
        {
            var t = obj as Tenant;
            return t != null && t.TenantId == TenantId;
        }

        public override int GetHashCode()
        {
            return TenantId;
        }

        public override string ToString()
        {
            return TenantDomain ?? TenantAlias;
        }


        internal string GetTrustedDomains()
        {
            TrustedDomains.RemoveAll(d => string.IsNullOrEmpty(d));
            if (TrustedDomains.Count == 0) return null;
            return string.Join("|", TrustedDomains.ToArray());
        }

        internal void SetTrustedDomains(string trustedDomains)
        {
            if (string.IsNullOrEmpty(trustedDomains))
            {
                TrustedDomains.Clear();
            }
            else
            {
                TrustedDomains.AddRange(trustedDomains.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public String GetTenantDomain(bool allowMappedDomain = true)
        {
            var result = String.Empty;

            var baseHost = TenantUtil.GetBaseDomain(HostedRegion);

            if (string.IsNullOrEmpty(baseHost) && !string.IsNullOrEmpty(HostedRegion))
            {
                baseHost = HostedRegion;
            }

            if (baseHost == "localhost" || TenantAlias == "localhost")
            {
                //single tenant on local host
                TenantAlias = "localhost";
                result = HostName;
            }
            else
            {
                result = string.Format("{0}.{1}", TenantAlias, baseHost).TrimEnd('.').ToLowerInvariant();
            }
            if (!string.IsNullOrEmpty(MappedDomain) && allowMappedDomain)
            {
                if (MappedDomain.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                {
                    MappedDomain = MappedDomain.Substring(7);
                }
                if (MappedDomain.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                {
                    MappedDomain = MappedDomain.Substring(8);
                }
                result = MappedDomain.ToLowerInvariant();
            }

            return result;
        }
    }
}