using System;
using System.Runtime.Serialization;
using ASC.Mail.Enums.Filter;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "FilterAction")]
    public class MailSieveFilterActionData : IEquatable<MailSieveFilterActionData>
    {
        [DataMember(IsRequired = true, Name = "action")]
        public ActionType Action { get; set; }

        [DataMember(IsRequired = false, Name = "data")]
        public string Data { get; set; }

        public bool Equals(MailSieveFilterActionData other)
        {
            if (other == null) return false;

            return Action == other.Action && Data.Equals(other.Data);
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode() ^ (Data ?? "").GetHashCode();
        }
    }
}
