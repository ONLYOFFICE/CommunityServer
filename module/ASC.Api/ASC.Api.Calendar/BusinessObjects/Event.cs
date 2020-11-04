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
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Api.Calendar.Wrappers;
using ASC.Web.Core.Calendars;
using ASC.Core.Tenants;

namespace ASC.Api.Calendar.BusinessObjects
{    
    [AllDayLongUTCAttribute]
    public class Event : BaseEvent, ISecurityObject
    {
        public Event()
        {            
        }

        public int TenantId { get; set; }

        #region ISecurityObjectId Members

        public object SecurityId
        {
            get { return this.Id; }
        }

        /// <inheritdoc/>
        public Type ObjectType
        {
            get { return typeof(Event); }
        }

        #endregion

        #region ISecurityObjectProvider Members

        public IEnumerable<ASC.Common.Security.Authorizing.IRole> GetObjectRoles(ASC.Common.Security.Authorizing.ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            List<IRole> roles = new List<IRole>();
            if (account.ID.Equals(this.OwnerId))
                roles.Add(ASC.Common.Security.Authorizing.Constants.Owner);

            return roles;
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            int calId;
            if (int.TryParse(this.CalendarId, out calId))
                return new Calendar() { Id = this.CalendarId };

            return null;
        }

        public bool InheritSupported
        {
            get { return true; }
        }

        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        #endregion
       
    }

}
