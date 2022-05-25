using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ASC.Core;
using ASC.Mail.Core.Engine;
using ASC.Mail.Core.Entities;
using ASC.Mail.Enums;
using ASC.Migration.Core.Models;
using ASC.Migration.Resources;

using FolkerKinzel.VCards;

namespace ASC.Migration.NextcloudWorkspace.Models.Parse
{
    public class NCMigratingContacts : MigratingContacts
    {
        public override int ContactsCount => contacts.Count;
        public override string ModuleName => MigrationResource.NextcloudModuleNameContacts;

        private List<NCContact> contacts = new List<NCContact>();
        private NCMigratingUser user;
        private NCAddressbooks addressbooks;

        public NCMigratingContacts(NCMigratingUser user, NCAddressbooks addressbooks, Action<string, Exception> log) : base(log)
        {
            this.user = user;
            this.addressbooks = addressbooks;
        }

        public override void Parse()
        {
            if(this.addressbooks != null)
            {
                if (this.addressbooks.Cards != null)
                {
                    foreach (var vcardByte in this.addressbooks.Cards)
                    {
                        var vcard = VCard.Parse(Encoding.Default.GetString(vcardByte.CardData));
                        foreach (var item in vcard)
                        {
                            if (item.EmailAddresses == null || !item.EmailAddresses.Any()) continue;

                            var contact = new NCContact()
                            {
                                Emails = item.EmailAddresses.Select(v => v.Value).Distinct().ToList()
                            };

                            if (item.Addresses != null && item.Addresses.Any()) contact.Address = item.Addresses.First().Value.ToString();
                            if (item.DisplayNames != null && item.DisplayNames.Any()) contact.ContactName = item.DisplayNames.First().Value.ToString();
                            if (item.Notes != null && item.Notes.Any()) contact.Description = string.Join("\n", item.Notes.Select(v => v.Value));
                            if (item.PhoneNumbers != null && item.PhoneNumbers.Any()) contact.Phones = item.PhoneNumbers.Select(v => v.Value).Distinct().ToList();

                            contacts.Add(contact);
                        }
                    }
                }
            }
        }

        public override void Migrate()
        {
            if (!ShouldImport) return;

            var engine = new ContactEngine(CoreContext.TenantManager.GetCurrentTenant().TenantId, user.Guid.ToString());
            var cards = GetContactCards();
            foreach (var card in cards)
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

    }
}
