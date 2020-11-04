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
using System.Linq;
using System.Web;
using ASC.Common.Security;
using ASC.Web.UserControls.Wiki;
using ASC.Common.Security.Authorizing;

namespace ASC.Web.Community.Wiki.Common
{
    public class WikiObjectsSecurityObject : ISecurityObject
    {
        public IWikiObjectOwner Object { get; private set; }
        public WikiObjectsSecurityObject(IWikiObjectOwner obj)
        {
            Object = obj;
        }

        #region ISecurityObjectId Members

        public Type ObjectType
        {
            get { return this.GetType(); }
        }

        public object SecurityId
        {
            get { return Object.GetObjectId(); }
        }

        #endregion

        #region ISecurityObjectProvider Members

        public IEnumerable<ASC.Common.Security.Authorizing.IRole> GetObjectRoles(ASC.Common.Security.Authorizing.ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            var roles = new List<IRole>();

            if (!Guid.Empty.Equals(Object.OwnerID) && Object.OwnerID.Equals(account.ID))
            {
                roles.Add(ASC.Common.Security.Authorizing.Constants.Owner);
            }

            return roles;
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            throw new NotImplementedException();
        }

        public bool InheritSupported
        {
            get { return false; }
        }

        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        #endregion
    }
}
