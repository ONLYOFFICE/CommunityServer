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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace ASC.Core.Tenants
{
    [DataContract]
    [DebuggerDisplay("{Name}")]
    public class TenantQuota : ICloneable
    {
        public static readonly TenantQuota Default = new TenantQuota(Tenant.DEFAULT_TENANT)
            {
                Name = "Default",
                MaxFileSize = 25*1024*1024, // 25Mb
                MaxTotalSize = long.MaxValue,
                ActiveUsers = int.MaxValue,
            };

        [DataMember(Name = "Id", Order = 10)]
        public int Id { get; private set; }

        [DataMember(Name = "Name", Order = 20)]
        public string Name { get; set; }

        [DataMember(Name = "MaxFileSize", Order = 30)]
        public long MaxFileSize { get; set; }

        [DataMember(Name = "MaxTotalSize", Order = 40)]
        public long MaxTotalSize { get; set; }

        [DataMember(Name = "ActiveUsers", Order = 50)]
        public int ActiveUsers { get; set; }

        [DataMember(Name = "Features", Order = 60)]
        public string Features { get; set; }

        [DataMember(Name = "Price", Order = 70)]
        public decimal Price { get; set; }

        [DataMember(Name = "Price2", Order = 80)]
        public decimal Price2 { get; set; }

        [DataMember(Name = "AvangateId", Order = 90)]
        public string AvangateId { get; set; }

        [DataMember(Name = "Visible", Order = 100)]
        public bool Visible { get; set; }

        [DataMember(Name = "Year", Order = 110)]
        public bool Year
        {
            get { return GetFeature("year"); }
            set { SetFeature("year", value); }
        }

        [DataMember(Name = "Year3", Order = 110)]
        public bool Year3
        {
            get { return GetFeature("year3"); }
            set { SetFeature("year3", value); }
        }

        public bool NonProfit
        {
            get { return GetFeature("non-profit"); }
            set { SetFeature("non-profit", value); }
        }

        public bool Trial
        {
            get { return GetFeature("trial"); }
            set { SetFeature("trial", value); }
        }

        public bool Free
        {
            get { return GetFeature("free"); }
            set { SetFeature("free", value); }
        }

        public bool Open
        {
            get { return GetFeature("open"); }
            set { SetFeature("open", value); }
        }

        public bool ControlPanel
        {
            get { return GetFeature("controlpanel"); }
            set { SetFeature("controlpanel", value); }
        }

        public bool Update
        {
            get { return GetFeature("update"); }
            set { SetFeature("update", value); }
        }

        public bool Support
        {
            get { return GetFeature("support"); }
            set { SetFeature("support", value); }
        }

        public bool Audit
        {
            get { return GetFeature("audit"); }
            set { SetFeature("audit", value); }
        }

        public bool DocsEdition
        {
            get { return GetFeature("docs"); }
            set { SetFeature("docs", value); }
        }

        public bool HasDomain
        {
            get { return GetFeature("domain"); }
            set { SetFeature("domain", value); }
        }

        public bool HealthCheck
        {
            get { return GetFeature("healthcheck"); }
            set { SetFeature("healthcheck", value); }
        }

        public bool HasMigration
        {
            get { return GetFeature("migration"); }
            set { SetFeature("migration", value); }
        }

        public bool Ldap
        {
            get { return GetFeature("ldap"); }
            set { SetFeature("ldap", value); }
        }

        public bool Sso
        {
            get { return GetFeature("sso"); }
            set { SetFeature("sso", value); }
        }

        public bool Branding
        {
            get { return GetFeature("branding"); }
            set { SetFeature("branding", value); }
        }

        public bool SSBranding
        {
            get { return GetFeature("ssbranding"); }
            set { SetFeature("ssbranding", value); }
        }

        public bool WhiteLabel
        {
            get { return GetFeature("whitelabel"); }
            set { SetFeature("whitelabel", value); }
        }

        public bool Customization
        {
            get { return GetFeature("customization"); }
            set { SetFeature("customization", value); }
        }

        public bool DiscEncryption
        {
            get { return GetFeature("discencryption"); }
            set { SetFeature("discencryption", value); }
        }

        public bool PrivacyRoom
        {
            get { return GetFeature("privacyroom"); }
            set { SetFeature("privacyroom", value); }
        }

        public int CountPortals
        {
            get
            {
                var features = (Features ?? string.Empty).Split(' ', ',', ';').ToList();
                var portals = features.FirstOrDefault(f => f.StartsWith("portals:"));
                int countPortals;
                if (portals == null || !Int32.TryParse(portals.Replace("portals:", ""), out countPortals) || countPortals <= 0)
                {
                    countPortals = 0;
                }
                return countPortals;
            }
            set
            {
                var features = (Features ?? string.Empty).Split(' ', ',', ';').ToList();
                var portals = features.FirstOrDefault(f => f.StartsWith("portals:"));
                features.Remove(portals);
                if (value > 0)
                {
                    features.Add("portals:" + value);
                }
                Features = string.Join(",", features.ToArray());
            }
        }


        public TenantQuota(int tenant)
        {
            Id = tenant;
        }


        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var q = obj as TenantQuota;
            return q != null && q.Id == Id;
        }


        public bool GetFeature(string feature)
        {
            return !string.IsNullOrEmpty(Features) && Features.Split(' ', ',', ';').Contains(feature);
        }

        internal void SetFeature(string feature, bool set)
        {
            var features = (Features == null
                                ? new string[] { }
                                : Features.Split(' ', ',', ';')).ToList();
            if (set && !features.Contains(feature))
            {
                features.Add(feature);
            }
            else if (!set && features.Contains(feature))
            {
                features.Remove(feature);
            }
            Features = string.Join(",", features.ToArray());
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}