/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Users;

namespace ASC.Web.Core.Calendars
{
    public class SharingOptions : ICloneable
    {
        [DataContract(Name = "PublicItem")]
        public class PublicItem
        {
            public Guid Id { get; set; }
            public bool IsGroup { get; set; }
        }

        public bool SharedForAll { get; set; }

        public List<PublicItem> PublicItems { get; set; }

        public SharingOptions()
        {
            this.PublicItems = new List<PublicItem>();
        }

        public bool PublicForItem(Guid itemId)
        {
            if (SharedForAll)
                return true;

            if(PublicItems.Exists(i=> i.Id.Equals(itemId)))
                return true;

            var u = CoreContext.UserManager.GetUsers(itemId);
            if(u!=null && u.ID!= ASC.Core.Users.Constants.LostUser.ID)
            {
                var userGroups = new List<GroupInfo>(CoreContext.UserManager.GetUserGroups(itemId));
                userGroups.AddRange(CoreContext.UserManager.GetUserGroups(itemId, Constants.SysGroupCategoryId));
                return userGroups.Exists(g => PublicItems.Exists(i => i.Id.Equals(g.ID)));
            }

            return false;
        }

        #region ICloneable Members

        public object Clone()
        {
            var o = new SharingOptions();
            o.SharedForAll = this.SharedForAll;
            foreach (var i in this.PublicItems)            
                o.PublicItems.Add(new PublicItem() { Id = i.Id, IsGroup = i.IsGroup });
            
            return o;
        }

        #endregion
    }

   
}
