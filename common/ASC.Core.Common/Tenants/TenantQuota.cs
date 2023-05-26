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
            MaxFileSize = 25 * 1024 * 1024, // 25Mb
            MaxTotalSize = long.MaxValue,
            ActiveUsers = int.MaxValue,
        };

        ///<example type="int">234</example>
        ///<order>10</order>
        [DataMember(Name = "Id", Order = 10)]
        public int Id { get; private set; }

        ///<example>Name</example>
        ///<order>20</order>
        [DataMember(Name = "Name", Order = 20)]
        public string Name { get; set; }

        ///<example type="int">12345</example>
        ///<order>30</order>
        [DataMember(Name = "MaxFileSize", Order = 30)]
        public long MaxFileSize { get; set; }

        ///<example type="int">12345</example>
        ///<order>40</order>
        [DataMember(Name = "MaxTotalSize", Order = 40)]
        public long MaxTotalSize { get; set; }

        ///<example type="int">2</example>
        ///<order>50</order>
        [DataMember(Name = "ActiveUsers", Order = 50)]
        public int ActiveUsers { get; set; }

        ///<example>Features</example>
        ///<order>60</order>
        [DataMember(Name = "Features", Order = 60)]
        public string Features { get; set; }

        ///<example type="double">22.5</example>
        ///<order>70</order>
        [DataMember(Name = "Price", Order = 70)]
        public decimal Price { get; set; }

        ///<example>AvangateId</example>
        ///<order>90</order>
        [DataMember(Name = "AvangateId", Order = 90)]
        public string AvangateId { get; set; }

        ///<example>true</example>
        ///<order>100</order>
        [DataMember(Name = "Visible", Order = 100)]
        public bool Visible { get; set; }

        ///<example>true</example>
        ///<order>110</order>
        [DataMember(Name = "Year", Order = 110)]
        public bool Year
        {
            get { return GetFeature("year"); }
            set { SetFeature("year", value); }
        }

        ///<example>true</example>
        ///<order>110</order>
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

        public bool EnableMailServer
        {
            get { return GetFeature("mailserver"); }
            set { SetFeature("mailserver", value); }
        }

        public bool Custom
        {
            get { return GetFeature("custom"); }
            set { SetFeature("custom", value); }
        }

        public int CountAdmin
        {
            get
            {
                var features = (Features ?? string.Empty).Split(' ', ',', ';').ToList();
                var admin = features.FirstOrDefault(f => f.StartsWith("admin:"));
                int countAdmin;
                if (admin == null || !Int32.TryParse(admin.Replace("admin:", ""), out countAdmin))
                {
                    countAdmin = Int32.MaxValue;
                }
                return countAdmin;
            }
            set
            {
                var features = (Features ?? string.Empty).Split(' ', ',', ';').ToList();
                var admin = features.FirstOrDefault(f => f.StartsWith("admin:"));
                features.Remove(admin);
                if (value > 0)
                {
                    features.Add("admin:" + value);
                }
                Features = string.Join(",", features.ToArray());
            }
        }

        public bool Restore
        {
            get { return GetFeature("restore"); }
            set { SetFeature("restore", value); }
        }

        public bool AutoBackup
        {
            get { return GetFeature("autobackup"); }
            set { SetFeature("autobackup", value); }
        }

        public bool Oauth
        {
            get { return GetFeature("oauth"); }
            set { SetFeature("oauth", value); }
        }

        public bool ContentSearch
        {
            get { return GetFeature("contentsearch"); }
            set { SetFeature("contentsearch", value); }
        }

        public bool ThirdParty
        {
            get { return GetFeature("thirdparty"); }
            set { SetFeature("thirdparty", value); }
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