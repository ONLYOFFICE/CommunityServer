using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASC.Api.Mail.DataContracts
{
    [Serializable]
    public class ContactInfo
    {
        [DataMember(IsRequired = false, Name = "id")]
        public int Id { get; set; }

        [DataMember(IsRequired = true, Name = "value")]
        public string Value { get; set; }

        [DataMember(IsRequired = true, Name = "isPrimary")]
        public bool IsPrimary { get; set; }
    }

    [Serializable]
    [DataContract(Namespace = "", Name = "Contact")]
    public class MailContactData
    {
        [CollectionDataContract(Namespace = "", ItemName = "email")]
        public class EmailsList<TItem> : List<TItem>
        {
            public EmailsList()
            {
            }

            public EmailsList(IEnumerable<TItem> items)
                : base(items)
            {
            }
        }

        [CollectionDataContract(Namespace = "", ItemName = "phone")]
        public class PhoneNumgersList<TItem> : List<TItem>
        {
            public PhoneNumgersList()
            {
            }

            public PhoneNumgersList(IEnumerable<TItem> items)
                : base(items)
            {
            }
        }

        [DataMember(IsRequired = false, Name = "id")]
        public int ContactId { get; set; }

        [DataMember(IsRequired = true, Name = "name")]
        public string Name { get; set; }

        [DataMember(IsRequired = true, Name = "description")]
        public string Description { get; set; }

        [DataMember(IsRequired = true, Name = "emails")]
        public EmailsList<ContactInfo> Emails { get; set; }

        [DataMember(IsRequired = true, Name = "phones")]
        public PhoneNumgersList<ContactInfo> PhoneNumbers { get; set; }

        [DataMember(IsRequired = false, Name = "type")]
        public int Type { get; set; }

        [DataMember(IsRequired = true, Name = "smallFotoUrl")]
        public string SmallFotoUrl { get; set; }

        [DataMember(IsRequired = true, Name = "mediumFotoUrl")]
        public string MediumFotoUrl { get; set; }
    }
}
