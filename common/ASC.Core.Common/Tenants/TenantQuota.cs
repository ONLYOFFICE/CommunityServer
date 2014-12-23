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

        public bool Trial
        {
            get { return GetFeature("trial"); }
            set { SetFeature("trial", value); }
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

        public bool DocsEdition
        {
            get { return GetFeature("docs"); }
            set { SetFeature("docs", value); }
        }

        [DataMember(Name = "Year", Order = 110)]
        public bool Year
        {
            get { return GetFeature("year"); }
            set { SetFeature("year", value); }
        }

        public bool NonProfit
        {
            get { return GetFeature("non-profit"); }
            set { SetFeature("non-profit", value); }
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

        public bool Gantt
        {
            get { return GetFeature("gantt"); }
            set { SetFeature("gantt", value); }
        }

        public bool Visitor
        {
            get { return GetFeature("visitor"); }
            set { SetFeature("visitor", value); }
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