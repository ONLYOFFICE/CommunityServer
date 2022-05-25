using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core.Models
{
    public abstract class MigratingGroup : ImportableEntity
    {
        public abstract string GroupName { get; }
        public abstract string ModuleName { get; }
        public abstract List<string> UserUidList { get; }
        public virtual MigratingApiGroup ToApiInfo()
        {
            return new MigratingApiGroup()
            {
                GroupName = GroupName,
                ModuleName = ModuleName,
                UserUidList = UserUidList
            };
        }

        protected MigratingGroup(Action<string, Exception> log) : base(log) { }
    }
}
