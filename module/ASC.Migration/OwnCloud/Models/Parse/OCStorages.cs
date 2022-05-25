using System.Collections.Generic;

namespace ASC.Migration.OwnCloud.Models
{
    public class OCStorages
    {
        public int NumericId { get; set; }
        public string Id { get; set; }
        public List<OCFileCache> FileCache { get; set; }
    }

    public class OCFileCache
    {
        public int FileId { get; set; }
        public string Path { get; set; }
        public List<OCShare> Share { get; set; }
    }

    public class OCShare
    {
        public int Id { get; set; }
        public string ShareWith { get; set; }
        public int Premissions { get; set; }
    }
}
