using System;
using System.Globalization;
using System.Runtime.Serialization;
using ASC.Mail.Aggregator.Common;

namespace ASC.Mail.Aggregator.CollectionService.Queue.Data
{
    [DataContract]
    public class TenantData
    {
        [DataMember(Name = "tenant")]
        public int TenantId { get; set; }

        [DataMember(Name = "tariff_type")]
        public Defines.TariffType TariffType { get; set; }

        [DataMember(Name = "expired")]
        public string Expired { get; set; }

        [IgnoreDataMember]
        public DateTime ExpiredDate { get; set; }

        [OnSerializing]
        public void OnSerializing(StreamingContext context)
        {
            Expired = ExpiredDate.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            try
            {
                ExpiredDate = !string.IsNullOrEmpty(Expired)
                    ? DateTime.ParseExact(Expired, "yyyy-MM-dd hh:mm:ss", null, DateTimeStyles.None)
                    : DateTime.UtcNow;
            }
            catch (FormatException)
            {
                ExpiredDate = DateTime.UtcNow;
            }
        }
    }
}
