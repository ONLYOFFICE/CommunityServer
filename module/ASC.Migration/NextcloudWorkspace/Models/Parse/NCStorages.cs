using System.Collections.Generic;

namespace ASC.Migration.NextcloudWorkspace.Models
{
    public class NCStorages
    {
        public int NumericId { get; set; }
        public string Id { get; set; }
        public List<NCFileCache> FileCache { get; set; }
    }

    public class NCFileCache
    {
        public int FileId { get; set; }
        public string Path { get; set; }
        public List<NCShare> Share { get; set; }
    }

    public class NCShare
    {
        public int Id { get; set; }
        public string ShareWith { get; set; }
        public int Premissions { get; set; }
    }
}
