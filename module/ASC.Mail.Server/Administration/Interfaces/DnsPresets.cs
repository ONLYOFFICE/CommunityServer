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

namespace ASC.Mail.Server.Administration.Interfaces
{
    public class DnsPresets
    {
        public int MxPriority { get; private set; }
        public string MxHost { get; private set; }
        public string DkimSelector { get; private set; }
        public string SpfValue { get; private set; }
        public string DomainCheckPrefix { get; private set; }
        public string CurrentOrigin { get { return "@"; } }

        public class Builder
        {
            internal int mx_priority;
            internal string mx_host;
            internal string dkim_selector;
            internal string spf_value;
            internal string domain_check_prefix;

            public virtual Builder SetMX(string mx_host_obj, int mx_priority_obj)
            {
                mx_priority = mx_priority_obj;
                mx_host = mx_host_obj;
                return this;
            }

            public virtual Builder SetDKIMSelector(string dkim_selector_obj)
            {
                dkim_selector = dkim_selector_obj;
                return this;
            }

            public virtual Builder SetSpfValue(string spf_value_obj)
            {
                spf_value = spf_value_obj;
                return this;
            }

            public virtual Builder SetDomainCheckPrefix(string domain_check_prefix_obj)
            {
                domain_check_prefix = domain_check_prefix_obj;
                return this;
            }

            public DnsPresets Build()
            {
                return new DnsPresets(this);
            }
        }

        private DnsPresets(Builder builder)
        {
            if (builder.mx_priority < 0)
                throw new ArgumentException("Invalid mx priority", "builder");

            if (string.IsNullOrEmpty(builder.mx_host))
                throw new ArgumentException("Invalid mx host", "builder");

            if (string.IsNullOrEmpty(builder.dkim_selector))
                throw new ArgumentException("Invalid dkim selector", "builder");

            if (string.IsNullOrEmpty(builder.spf_value))
                throw new ArgumentException("Invalid spf value", "builder");

            if (string.IsNullOrEmpty(builder.domain_check_prefix))
                throw new ArgumentException("Invalid domain check prefix", "builder");

            MxPriority = builder.mx_priority;
            MxHost = builder.mx_host;
            DkimSelector = builder.dkim_selector;
            SpfValue = builder.spf_value;
            DomainCheckPrefix = builder.domain_check_prefix;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            var other = (DnsPresets)obj;

            return MxPriority == other.MxPriority &&
               MxHost == other.MxHost &&
               DkimSelector == other.DkimSelector &&
               SpfValue == other.SpfValue &&
               DomainCheckPrefix == other.DomainCheckPrefix;
        }

        public override int GetHashCode()
        {
            return MxPriority ^ MxHost.GetHashCode() ^ DkimSelector.GetHashCode() ^ SpfValue.GetHashCode() ^ DomainCheckPrefix.GetHashCode();
        }

    }
}
