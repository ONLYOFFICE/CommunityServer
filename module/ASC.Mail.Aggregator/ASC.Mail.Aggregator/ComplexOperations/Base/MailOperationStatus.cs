using System.Runtime.Serialization;

namespace ASC.Mail.Aggregator.ComplexOperations.Base
{
    [DataContract]
    public class MailOperationStatus
    {
        [DataMember]
        public bool Completed { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public int Percents { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public int OperationType { get; set; }

        [DataMember]
        public string Operation { get; set; }
    }
}
