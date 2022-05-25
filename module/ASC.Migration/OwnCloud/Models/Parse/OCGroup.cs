using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Migration.OwnCloud.Models
{
    public class OCGroup
    {
        public string GroupGid { get; set; }
        public List<string> UsersUid { get; set; }
    }
}
