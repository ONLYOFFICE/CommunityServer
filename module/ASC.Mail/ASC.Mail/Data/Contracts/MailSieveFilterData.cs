using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "Filter")]
    public class MailSieveFilterData
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "position")]
        public int Position { get; set; }

        [DataMember(Name = "enabled")]
        public bool Enabled { get; set; }

        [DataMember(Name = "conditions")]
        public List<MailSieveFilterConditionData> Conditions { get; set; }

        [DataMember(Name = "actions")]
        public List<MailSieveFilterActionData> Actions { get; set; }

        [DataMember(Name = "options")]
        public MailSieveFilterOptionsData Options { get; set; }

        public MailSieveFilterData()
        {
            Actions = new List<MailSieveFilterActionData>();
            Conditions = new List<MailSieveFilterConditionData>();
            Options = new MailSieveFilterOptionsData();
        }
    }
}
