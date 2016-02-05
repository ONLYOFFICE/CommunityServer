using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASC.Api.Mail.DataContracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "Tag")]
    public class MailTagData
    {
        [CollectionDataContract(Namespace = "", ItemName = "address")]
        public class AddressesList<TItem> : List<TItem>
        {
            public AddressesList()
            {
            }

            public AddressesList(IEnumerable<TItem> items)
                : base(items)
            {
            }
        }

        [DataMember(IsRequired = true, Name = "id")]
        public int Id { get; set; }

        [DataMember(IsRequired = true, Name = "name")]
        public string Name { get; set; }

        [DataMember(IsRequired = true, Name = "style")]
        public string Style { get; set; }

        [DataMember(IsRequired = true, Name = "addresses")]
        public AddressesList<string> Addresses { get; set; }

        [DataMember(Name = "lettersCount")]
        public int LettersCount { get; set; }
    }
}
