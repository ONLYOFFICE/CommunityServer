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


#region usings

using System;
using System.Collections.Generic;

#endregion

namespace ASC.Common.Security.Authorizing
{
    public class AzObjectSecurityProviderHelper
    {
        private readonly SecurityCallContext callContext;
        private readonly bool currObjIdAsProvider;
        private ISecurityObjectId currObjId;
        private ISecurityObjectProvider currSecObjProvider;

        public AzObjectSecurityProviderHelper(ISecurityObjectId objectId, ISecurityObjectProvider secObjProvider)
        {
            if (objectId == null) throw new ArgumentNullException("objectId");
            currObjIdAsProvider = false;
            currObjId = objectId;
            currSecObjProvider = secObjProvider;
            if (currSecObjProvider == null && currObjId is ISecurityObjectProvider)
            {
                currObjIdAsProvider = true;
                currSecObjProvider = (ISecurityObjectProvider) currObjId;
            }
            callContext = new SecurityCallContext();
        }

        public ISecurityObjectId CurrentObjectId
        {
            get { return currObjId; }
        }

        public bool ObjectRolesSupported
        {
            get { return currSecObjProvider != null && currSecObjProvider.ObjectRolesSupported; }
        }

        public IEnumerable<IRole> GetObjectRoles(ISubject account)
        {
            IEnumerable<IRole> roles = currSecObjProvider.GetObjectRoles(account, currObjId, callContext);
            foreach (IRole role in roles)
            {
                if (!callContext.RolesList.Contains(role)) callContext.RolesList.Add(role);
            }
            return roles;
        }

        public bool NextInherit()
        {
            if (currSecObjProvider == null || !currSecObjProvider.InheritSupported) return false;
            currObjId = currSecObjProvider.InheritFrom(currObjId);
            if (currObjId == null) return false;
            if (currObjIdAsProvider)
            {
                currSecObjProvider = currObjId as ISecurityObjectProvider;
            }
            callContext.ObjectsStack.Insert(0, CurrentObjectId);
            return currSecObjProvider != null;
        }
    }
}