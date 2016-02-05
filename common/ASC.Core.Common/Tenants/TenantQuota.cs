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

        public bool HasBackup
        {
            get { return GetFeature("backup"); }
            set { SetFeature("backup", value); }
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

        public bool Sms
        {
            get { return GetFeature("sms"); }
            set { SetFeature("sms", value); }
        }

        public bool Voip
        {
            get { return GetFeature("voip"); }
            set { SetFeature("voip", value); }
        }

        public bool Visitor
        {
            get { return GetFeature("visitor"); }
            set { SetFeature("visitor", value); }
        }

        public bool WhiteLabel
        {
            get { return GetFeature("whitelabel"); }
            set { SetFeature("whitelabel", value); }
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
            var features = (Features ?? string.Empty).Split(' ', ',', ';').ToList();
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