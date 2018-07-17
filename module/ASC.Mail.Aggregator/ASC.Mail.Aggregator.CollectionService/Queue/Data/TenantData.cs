using System;
using System.Runtime.Serialization;
using ASC.Mail.Aggregator.Common;

namespace ASC.Mail.Aggregator.CollectionService.Queue.Data
{
    [DataContract]
    public class TenantData
    {
        [DataMember(Name = "tenant")]
        public int Tenant { get; set; }

        [DataMember(Name = "tariff_type")]
        public Defines.TariffType TariffType { get; set; }

		[DataMember(Name = "expired")]
        public DateTime Expired { get; set; }
    }
}
