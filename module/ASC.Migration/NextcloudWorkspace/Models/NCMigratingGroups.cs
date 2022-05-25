using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ASC.Core;
using ASC.Core.Users;
using ASC.Migration.Core.Models;
using ASC.Migration.Resources;

namespace ASC.Migration.NextcloudWorkspace.Models
{
    public class NCMigratingGroups : MigratingGroup
    {
        private string groupName;
        private List<string> userUidList;
        private NCGroup Group;
        private GroupInfo groupinfo;
        public Guid Guid => groupinfo.ID;
        public Dictionary<string, Guid> usersGuidList;
        public MigrationModules Module = new MigrationModules();
        public override List<string> UserUidList => userUidList;
        public override string GroupName => groupName;
        public override string ModuleName => MigrationResource.ModuleNameGroups;
        public NCMigratingGroups(NCGroup group, Action<string, Exception> log) : base(log)
        {
            Group = group;
        }

        public override void Parse()
        {
            groupName = Group.GroupGid;
            groupinfo = new GroupInfo()
            {
                Name = Group.GroupGid
            };
            userUidList = Group.UsersUid;
            Module = new MigrationModules(ModuleName, MigrationResource.OnlyofficeModuleNamePeople);
        }

        public override void Migrate()
        {
            var existingGroups = CoreContext.UserManager.GetGroups().ToList();
            var oldGroup = existingGroups.Find(g => g.Name == groupinfo.Name);
            if (oldGroup != null)
            {
                groupinfo = oldGroup;
            }
            else
            {
                groupinfo = CoreContext.UserManager.SaveGroupInfo(groupinfo);
            }
            foreach (var userGuid in usersGuidList)
            {
                UserInfo user;
                try
                {
                    user = CoreContext.UserManager.GetUsers(userGuid.Value);
                    if (user == Constants.LostUser)
                    {
                        throw new ArgumentNullException();
                    }
                    if(!CoreContext.UserManager.IsUserInGroup(user.ID,groupinfo.ID))
                    {
                        CoreContext.UserManager.AddUserIntoGroup(user.ID, groupinfo.ID);
                    }
                }
                catch (Exception ex)
                {
                    //Think about the text of the error
                    Log($"Couldn't to add user {userGuid.Key} to group {groupName} ", ex);
                }
            }
        }
    }
}
