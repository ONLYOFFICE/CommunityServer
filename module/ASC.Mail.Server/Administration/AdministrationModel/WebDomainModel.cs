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

        protected WebDomainModel(int id, int tenant, string name, bool is_verified, MailServerBase server)
            : base(name)
        {
            if (id < 0)
                throw new ArgumentException("Negative domain teamlab id", "id");

            if (tenant < 0)
                throw new ArgumentException("Negative domain teamlab tenant id", "tenant");

            Id = id;
            Tenant = tenant;
            Server = server;
            IsVerified = is_verified;
        }

        public IDnsSettings AddDns(int dns_id, IMailServerFactory factory)
        {
            IDnsSettings dns_records;
            using (var db_context_with_tran = TeamlabDnsDal.CreateMailDbContext(true))
            {
                var dns_dto = TeamlabDnsDal.LinkDnsToDomain(dns_id, Id, db_context_with_tran.DbManager);
                var txt_name = Server.SetupInfo.DnsPresets.CurrentOrigin;

                dns_records = factory.CreateDnsSettings(dns_dto.id, dns_dto.tenant, dns_dto.user, Name, dns_dto.dkim_selector,
                                                        dns_dto.dkim_private_key, dns_dto.dkim_public_key, txt_name,
                                                        dns_dto.domain_chek, txt_name, dns_dto.spf,
                                                        Server.SetupInfo.DnsPresets.MxHost, Server.SetupInfo.DnsPresets.MxPriority, Server.Logger);

                var dkim_base = new DkimRecordBase(dns_records.DkimSelector, dns_records.DkimPrivateKey,
                                                   dns_records.DkimPublicKey);

                _AddDkim(dkim_base);

                db_context_with_tran.CommitTransaction();
            }

            return dns_records;
        }

        protected abstract void _AddDkim(DkimRecordBase dkim_to_add);

        public IDnsSettings GetDns(IMailServerFactory factory)
        {
            var dns_dto = TeamlabDnsDal.GetDomainDnsRecords(Id);

            if (dns_dto == null) return null;

            var txt_name = Server.SetupInfo.DnsPresets.CurrentOrigin;
            var dns = factory.CreateDnsSettings(dns_dto.id, dns_dto.tenant, dns_dto.user, Name, dns_dto.dkim_selector,
                                                dns_dto.dkim_private_key, dns_dto.dkim_public_key, txt_name,
                                                dns_dto.domain_chek, txt_name, dns_dto.spf,
                                                Server.SetupInfo.DnsPresets.MxHost, Server.SetupInfo.DnsPresets.MxPriority, Server.Logger);

            return dns;
        }

        public void SetVerified(bool is_verified)
        {
            TeamlabDomainDal.SetDomainVerified(Id, is_verified);
            IsVerified = is_verified;
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
