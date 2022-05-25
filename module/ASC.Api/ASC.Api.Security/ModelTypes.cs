using System.Collections.Generic;

namespace ASC.Api.Security
{
    public class ModelTypes
    {
        public IEnumerable<string> Actions { get; set; }
        public IEnumerable<string> ActionTypes { get; set; }
        public IEnumerable<string> ProductTypes { get; set; }
        public IEnumerable<string> ModuleTypes { get; set; }
        public IEnumerable<string> EntryTypes { get; set; }
    }
}
