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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ASC.Core;
using ASC.Mail.Core.Engine;
using ASC.Mail.Core.Entities;
using ASC.Mail.Enums;
using ASC.Migration.Core.Models;
using ASC.Migration.GoogleWorkspace.Models.Parse;
using ASC.Migration.Resources;

using FolkerKinzel.VCards;

namespace ASC.Migration.GoogleWorkspace.Models
{
    public class GwsMigratingContacts : MigratingContacts
    {
        public override int ContactsCount => contacts.Count;
        public override string ModuleName => MigrationResource.GoogleModuleNameContacts;

        public override void Parse()
        {
            var vcfPath = Path.Combine(rootFolder, "Contacts", "All Contacts", "All Contacts.vcf");
            if (!File.Exists(vcfPath)) return;

            var vcards = VCard.Load(vcfPath);
            foreach (var vcard in vcards)
            {
                if (vcard.EmailAddresses == null || !vcard.EmailAddresses.Any()) continue; // We can't import contacts without email

                var contact = new GwsContact()
                {
                    Emails = vcard.EmailAddresses.Select(v => v.Value).Distinct().ToList()
                };

                if (vcard.Addresses != null && vcard.Addresses.Any()) contact.Address = vcard.Addresses.First().Value.ToString();
                if (vcard.DisplayNames != null && vcard.DisplayNames.Any()) contact.ContactName = vcard.DisplayNames.First().Value.ToString();
                if (vcard.Notes != null && vcard.Notes.Any()) contact.Description = string.Join("\n", vcard.Notes.Select(v => v.Value));
                if (vcard.PhoneNumbers != null && vcard.PhoneNumbers.Any()) contact.Phones = vcard.PhoneNumbers.Select(v => v.Value).Distinct().ToList();

                contacts.Add(contact);
            }
        }

        public override void Migrate()
        {
            if (!ShouldImport) return;

            var engine = new ContactEngine(CoreContext.TenantManager.GetCurrentTenant().TenantId, user.Guid.ToString());
            foreach (var card in GetContactCards())
            {
                try
                {
                    engine.SaveContactCard(card);
                }
                catch (Exception ex)
                {
                    Log($"Couldn't save contactCard {card.ContactInfo.ContactName}", ex);
                }
            }
        }

        public GwsMigratingContacts(string rootFolder, GwsMigratingUser user, Action<string, Exception> log) : base(log)
        {
            this.rootFolder = rootFolder;
            this.user = user;
        }

        private List<ContactCard> GetContactCards()
        {
            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var userId = user.Guid.ToString();

            var portalContacts = new List<ContactCard>();
            foreach (var gwsContact in contacts)
            {
                var portalContact = new Contact()
                {
                    Type = ContactType.Personal,
                    ContactName = gwsContact.ContactName,
                    Address = gwsContact.Address,
                    Tenant = tenantId,
                    User = userId
                };

                var infos = new List<ContactInfo>();
                if (gwsContact.Emails != null)
                {
                    infos.AddRange(gwsContact
                        .Emails.Select(e => new ContactInfo() { Data = e, Type = (int)ContactInfoType.Email, Tenant = tenantId, User = userId }));
                }
                if (gwsContact.Phones != null)
                {
                    infos.AddRange(gwsContact
                        .Phones.Select(p => new ContactInfo() { Data = p, Type = (int)ContactInfoType.Phone, Tenant = tenantId, User = userId }));
                }

                try
                {
                    portalContacts.Add(new ContactCard(portalContact, infos));
                }
                catch (Exception ex)
                {
                    Log($"Couldn't create contactCard {gwsContact.ContactName}", ex);
                }
            }

            return portalContacts;
        }

        private string rootFolder;

        private List<GwsContact> contacts = new List<GwsContact>();
        private GwsMigratingUser user;
    }
}
