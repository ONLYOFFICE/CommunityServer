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
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.Dal;

namespace ASC.Mail.Server.Administration.ServerModel
{
    public abstract class WebDomainModel : WebDomainBase, IWebDomain
    {
        public int Id { get; private set; }
        public int Tenant { get; private set; }
        public bool IsVerified { get; private set; }

        public MailServerBase Server { get; private set; }

        private DnsDal _dnsDal;
        private DnsDal TeamlabDnsDal
        {
            get { return _dnsDal ?? (_dnsDal = new DnsDal(Tenant, Server.User)); }
        }

        private WebDomainDal _domainDal;
        private WebDomainDal TeamlabDomainDal
        {
            get { return _domainDal ?? (_domainDal = new WebDomainDal(Tenant)); }
        }

        protected WebDomainModel(int id, int tenant, string name, bool isVerified, MailServerBase server)
            : base(name)
        {
            if (id < 0)
                throw new ArgumentException("Negative domain teamlab id", "id");

            Id = id;
            Tenant = tenant;
            Server = server;
            IsVerified = isVerified;
        }

        public IDnsSettings AddDns(int dnsId, IMailServerFactory factory)
        {
            IDnsSettings dnsRecords;
            using (var dbContextWithTran = TeamlabDnsDal.CreateMailDbContext(true))
            {
                var dnsDto = TeamlabDnsDal.LinkDnsToDomain(dnsId, Id, dbContextWithTran.DbManager);
                var txtName = Server.SetupInfo.DnsPresets.CurrentOrigin;

                dnsRecords = factory.CreateDnsSettings(dnsDto.id, dnsDto.tenant, dnsDto.user, Name, dnsDto.dkim_selector,
                                                        dnsDto.dkim_private_key, dnsDto.dkim_public_key, txtName,
                                                        dnsDto.domain_chek, txtName, dnsDto.spf,
                                                        Server.SetupInfo.DnsPresets.MxHost, Server.SetupInfo.DnsPresets.MxPriority, Server.Logger);

                var dkimBase = new DkimRecordBase(dnsRecords.DkimSelector, dnsRecords.DkimPrivateKey,
                                                   dnsRecords.DkimPublicKey);

                _AddDkim(dkimBase);

                dbContextWithTran.CommitTransaction();
            }

            return dnsRecords;
        }

        protected abstract void _AddDkim(DkimRecordBase dkimToAdd);

        public IDnsSettings GetDns(IMailServerFactory factory)
        {
            var dnsDto = TeamlabDnsDal.GetDomainDnsRecords(Id);

            if (dnsDto == null) return null;

            var txtName = Server.SetupInfo.DnsPresets.CurrentOrigin;
            var dns = factory.CreateDnsSettings(dnsDto.id, dnsDto.tenant, dnsDto.user, Name, dnsDto.dkim_selector,
                                                dnsDto.dkim_private_key, dnsDto.dkim_public_key, txtName,
                                                dnsDto.domain_chek, txtName, dnsDto.spf,
                                                Server.SetupInfo.DnsPresets.MxHost, Server.SetupInfo.DnsPresets.MxPriority, Server.Logger);

            return dns;
        }

        public void SetVerified(bool isVerified)
        {
            TeamlabDomainDal.SetDomainVerified(Id, isVerified);
            IsVerified = isVerified;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            var other = (WebDomainModel)obj;

            return Id == other.Id && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return DateCreated.GetHashCode() ^ Name.GetHashCode() ^ Id;
        }

    }
}
