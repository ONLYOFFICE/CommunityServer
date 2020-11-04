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


        public int TenantId { get; internal set; }

        public string TenantAlias { get; set; }

        public string MappedDomain { get; set; }

        public int Version { get; set; }

        public DateTime VersionChanged { get; set; }

        public string TenantDomain { get { return GetTenantDomain(); } }

        public string HostedRegion { get; set; }

        public string Name { get; set; }

        public string Language { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public List<string> TrustedDomains { get; private set; }

        public TenantTrustedDomainsType TrustedDomainsType { get; set; }

        public Guid OwnerId { get; set; }

        public DateTime CreatedDateTime { get; internal set; }

        public CultureInfo GetCulture() { return !string.IsNullOrEmpty(Language) ? CultureInfo.GetCultureInfo(Language) : CultureInfo.CurrentCulture; }

        public DateTime LastModified { get; set; }

        public TenantStatus Status { get; internal set; }

        public DateTime StatusChangeDate { get; internal set; }

        public string PartnerId { get; set; }

        public string AffiliateId { get; set; }

        public string Campaign { get; set; }

        public string PaymentId { get; set; }

        public TenantIndustry Industry { get; set; }

        public bool Spam { get; set; }

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