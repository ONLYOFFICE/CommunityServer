using System;
using System.Runtime.Serialization;
using ASC.Mail.Enums.Filter;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "FilterCondition")]
    public class MailSieveFilterConditionData
    {
        [DataMember(IsRequired = true, Name = "key")]
        public ConditionKeyType Key { get; set; }

        [DataMember(IsRequired = true, Name = "operation")]
        public ConditionOperationType Operation { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }
    }
}
