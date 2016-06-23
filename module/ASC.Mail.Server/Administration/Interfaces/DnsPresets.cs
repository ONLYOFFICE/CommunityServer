/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

            public virtual Builder SetMx(string mxHostObj, int mxPriorityObj)
            {
                mx_priority = mxPriorityObj;
                mx_host = mxHostObj;
                return this;
            }

            public virtual Builder SetDkimSelector(string dkimSelectorObj)
            {
                dkim_selector = dkimSelectorObj;
                return this;
            }

            public virtual Builder SetSpfValue(string spfValueObj)
            {
                spf_value = spfValueObj;
                return this;
            }

            public virtual Builder SetDomainCheckPrefix(string domainCheckPrefixObj)
            {
                domain_check_prefix = domainCheckPrefixObj;
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
