using System.Collections.Generic;

namespace ASC.Migration.NextcloudWorkspace.Models
{
    public class NCAddressbooks
    {
        public int Id { get; set; }
        public List<NCCards> Cards { get; set; }
    }

    public class NCCards
    {
        public int Id { get; set; }
        public byte[] CardData { get; set; }
    }
}
