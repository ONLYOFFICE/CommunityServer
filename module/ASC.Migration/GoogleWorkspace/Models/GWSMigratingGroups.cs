/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ASC.Core;
using ASC.Core.Users;
using ASC.Migration.Core.Models;
using ASC.Migration.Resources;

namespace ASC.Migration.GoogleWorkspace.Models
{
    public class GWSMigratingGroups : MigratingGroup
    {
        private string groupName;
        private List<string> userUidList;
        private string rootFolder;
        private GroupInfo groupinfo;
        public Guid Guid => groupinfo.ID;
        public MigrationModules Module = new MigrationModules();
        public override List<string> UserUidList => userUidList;
        public override string GroupName => groupName;
        public override string ModuleName => MigrationResource.ModuleNameGroups;
        public GWSMigratingGroups(string rootFolder, Action<string, Exception> log) : base(log)
        {
            this.rootFolder = rootFolder;
        }

        public override void Parse()
        {
            userUidList = new List<string>();
            var groupsFolder = Path.Combine(rootFolder, "Groups");
            var groupInfo = Path.Combine(groupsFolder, "info.csv");
            using (StreamReader sr = new StreamReader(groupInfo))
            {
                string line = sr.ReadLine();
                line = sr.ReadLine();
                if(line!=null)
                {
                    groupName = line.Split(',')[9];
                    if(!string.IsNullOrWhiteSpace(groupName))
                    {
                        groupinfo = new GroupInfo()
                        {
                            Name = groupName
                        };
                    }
                }
            }
            if(!string.IsNullOrWhiteSpace(groupinfo.Name))
            {
                var groupMembers = Path.Combine(groupsFolder, "members.csv");
                using (StreamReader sr = new StreamReader(groupMembers))
                {
                    string line = sr.ReadLine();
                    while ((line = sr.ReadLine()) != null)
                    {
                        var b = line.Split(',');
                        userUidList.Add(line.Split(',')[1]);
                    }
                }
            }
            if(userUidList.Count > 0)
            {
                Module = new MigrationModules(ModuleName, MigrationResource.OnlyofficeModuleNamePeople);
            }
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
            foreach (var userEmail in userUidList)
            {
                UserInfo user;
                try
                {
                    user = CoreContext.UserManager.GetUserByEmail(userEmail);
                    if(user == Constants.LostUser)
                    {
                        throw new ArgumentNullException();
                    }
                    if (!CoreContext.UserManager.IsUserInGroup(user.ID, groupinfo.ID))
                    {
                        CoreContext.UserManager.AddUserIntoGroup(user.ID, groupinfo.ID);
                    }
                }
                catch(Exception ex)
                {
                    //Think about the text of the error
                    Log($"Couldn't to add user {userEmail} to group {groupName} {Path.Combine(rootFolder, "Groups")} ", ex);
                }
            }
        }
    }
}
