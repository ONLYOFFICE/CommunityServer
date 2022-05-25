using System.Collections.Generic;

namespace ASC.Migration.OwnCloud.Models
{
    public class OCAddressbooks
    {
        public int Id { get; set; }
        public List<OCCards> Cards { get; set; }
    }

    public class OCCards
    {
        public int Id { get; set; }
        public byte[] CardData { get; set; }
    }
}
