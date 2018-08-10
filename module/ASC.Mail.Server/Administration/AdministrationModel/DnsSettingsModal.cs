/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Common.Utils;
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

        public DnsSettingsModal(int id, int tenant, string user, string domainName, string dkimSelector, string dkimPrivateKey,
                                string dkimPublicKey, string domainCheckName, string domainCheckRecord,
                                string spfName, string spfRecord, string mxHost, int mxPriority, ILogger logger = null)
        {
            if (id < 0)
                throw new ArgumentException("Invalid domain id", "id");


            if (string.IsNullOrEmpty(user))
                throw new ArgumentException("Invalid user", "user");

            if (string.IsNullOrEmpty(dkimSelector))
                throw new ArgumentException("Invalid dkim selector", "dkimSelector");

            if (string.IsNullOrEmpty(dkimPrivateKey))
                throw new ArgumentException("Invalid dkim private key", "dkimPrivateKey");

            if (string.IsNullOrEmpty(dkimPublicKey))
                throw new ArgumentException("Invalid dkim public key", "dkimPublicKey");

            if (string.IsNullOrEmpty(domainCheckName))
                throw new ArgumentException("Invalid name of domain check record", "domainCheckName");

            if (string.IsNullOrEmpty(domainCheckRecord))
                throw new ArgumentException("Invalid domain check record", "domainCheckRecord");

            if (string.IsNullOrEmpty(spfName))
                throw new ArgumentException("Invalid name of spf record", "spfName");

            if (string.IsNullOrEmpty(spfRecord))
                throw new ArgumentException("Invalid spf record", "spfRecord");

            if (string.IsNullOrEmpty(mxHost))
                throw new ArgumentException("Invalid name of spf record", "spfName");

            if (mxPriority < 0)
                throw new ArgumentException("Invalid mx priority", "mxPriority");

            Id = id;
            Tenant = tenant;
            User = user;
            DomainName = domainName.ToLowerInvariant();
            DkimSelector = dkimSelector;
            DkimPublicKey = dkimPublicKey;
            DkimPrivateKey = dkimPrivateKey;
            DomainCheckRecordName = domainCheckName;
            DomainCheckRecord = domainCheckRecord;
            SpfRecordName = spfName;
            SpfRecord = spfRecord;
            MxHost = mxHost;
            MxPriority = mxPriority;
            Logger = logger ?? new NullLogger();
        }

        public bool CheckDnsStatus(string domainName = "")
        {
            var checkDomainName = string.IsNullOrEmpty(DomainName)
                                 ? domainName
                                 : DomainName;

            if (string.IsNullOrEmpty(checkDomainName))
                return false;

            var dnsLookup = new DnsLookup();

            MxVerified = dnsLookup.IsDomainMxRecordExists(checkDomainName, MxHost);

            SpfVerified = dnsLookup.IsDomainTxtRecordExists(checkDomainName, SpfRecord);

            DkimVerified = dnsLookup.IsDomainDkimRecordExists(checkDomainName, DkimSelector, DkimPublicKey);

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
