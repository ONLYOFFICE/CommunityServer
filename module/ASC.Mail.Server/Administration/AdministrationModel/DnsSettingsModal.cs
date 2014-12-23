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
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Server.Administration.Interfaces;

namespace ASC.Mail.Server.Administration.ServerModel
{
    public class DnsSettingsModal : IDnsSettings
    {
        public int Id { get; private set; }
        public int Tenant { get; private set; }
        public string User { get; private set; }
        public string DomainName { get; private set; }
        public string MxHost { get; private set; }
        public int MxPriority { get; private set; }
        public bool MxVerified { get; private set; }
        public string DkimSelector { get; private set; }
        public string DkimPublicKey { get; private set; }
        public string DkimPrivateKey { get; private set; }
        public bool DkimVerified { get; private set; }
        public string DomainCheckRecordName { get; private set; }
        public string DomainCheckRecord { get; private set; }
        public string SpfRecordName { get; private set; }
        public string SpfRecord { get; private set; }
        public bool SpfVerified { get; private set; }

        public ILogger Logger { get; private set; }

        public DnsSettingsModal(int id, int tenant, string user, string domain_name, string dkim_selector, string dkim_private_key,
                                string dkim_public_key, string domain_check_name, string domain_check_record,
                                string spf_name, string spf_record, string mx_host, int mx_priority, ILogger logger = null)
        {
            if (id < 0)
                throw new ArgumentException("Invalid domain id", "id");

            if (tenant < 0)
                throw new ArgumentException("Invalid tenant id", "tenant");

            if (string.IsNullOrEmpty(user))
                throw new ArgumentException("Invalid user", "user");

            if (string.IsNullOrEmpty(dkim_selector))
                throw new ArgumentException("Invalid dkim selector", "dkim_selector");

            if (string.IsNullOrEmpty(dkim_private_key))
                throw new ArgumentException("Invalid dkim private key", "dkim_private_key");

            if (string.IsNullOrEmpty(dkim_public_key))
                throw new ArgumentException("Invalid dkim public key", "dkim_public_key");

            if (string.IsNullOrEmpty(domain_check_name))
                throw new ArgumentException("Invalid name of domain check record", "domain_check_name");

            if (string.IsNullOrEmpty(domain_check_record))
                throw new ArgumentException("Invalid domain check record", "domain_check_record");

            if (string.IsNullOrEmpty(spf_name))
                throw new ArgumentException("Invalid name of spf record", "spf_name");

            if (string.IsNullOrEmpty(spf_record))
                throw new ArgumentException("Invalid spf record", "spf_record");

            if (string.IsNullOrEmpty(mx_host))
                throw new ArgumentException("Invalid name of spf record", "spf_name");

            if (mx_priority < 0)
                throw new ArgumentException("Invalid mx priority", "mx_priority");

            Id = id;
            Tenant = tenant;
            User = user;
            DomainName = domain_name.ToLowerInvariant();
            DkimSelector = dkim_selector;
            DkimPublicKey = dkim_public_key;
            DkimPrivateKey = dkim_private_key;
            DomainCheckRecordName = domain_check_name;
            DomainCheckRecord = domain_check_record;
            SpfRecordName = spf_name;
            SpfRecord = spf_record;
            MxHost = mx_host;
            MxPriority = mx_priority;
            Logger = logger ?? new NullLogger();
        }

        public bool CheckDnsStatus(string domain_name = "")
        {
            var check_domain_name = string.IsNullOrEmpty(DomainName)
                                 ? domain_name
                                 : DomainName;

            if (string.IsNullOrEmpty(check_domain_name))
                return false;

            MxVerified = DnsChecker.DnsChecker.IsMxSettedUpCorrectForDomain(check_domain_name, MxHost, Logger);

            SpfVerified = DnsChecker.DnsChecker.IsTxtRecordCorrect(check_domain_name, SpfRecord, Logger);

            DkimVerified = DnsChecker.DnsChecker.IsDkimSettedUpCorrectForDomain(check_domain_name, DkimSelector, DkimPublicKey,
                                                                                Logger);
            return MxVerified && SpfVerified && DkimVerified;
        }

        public override int GetHashCode()
        {
            return Id ^ Tenant ^ User.GetHashCode() ^ MxHost.GetHashCode() ^ MxPriority ^ MxVerified.GetHashCode() ^ 
                   DkimSelector.GetHashCode() ^ DkimPublicKey.GetHashCode() ^ DkimPrivateKey.GetHashCode() ^ DkimVerified.GetHashCode() ^ 
                   DomainCheckRecordName.GetHashCode() ^ DomainCheckRecord.GetHashCode() ^
                   SpfRecordName.GetHashCode() ^ SpfRecord.GetHashCode() ^ SpfVerified.GetHashCode() ^ Logger.GetHashCode();
        }
    }
}
