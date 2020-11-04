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
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core.Users;
using ASC.Web.Community.Product;

namespace ASC.Forum
{
	public class ThreadCategory : ISecurityObject
    {
        public virtual int ID { get; set; }

        public virtual string Title { get; set;}

        public virtual string Description { get; set;}

        public virtual int SortOrder { get; set; }

        public virtual DateTime CreateDate { get; set; }

        public virtual Guid PosterID { get; set; }

     

        public virtual int TenantID { get; set; }

        public virtual UserInfo Poster
        {
            get
            {
                return ASC.Core.CoreContext.UserManager.GetUsers(PosterID);
            }
        }

        public ThreadCategory()
        {   
        }

        public virtual bool Visible
        {
            get
            {
                return CommunitySecurity.CheckPermissions(this, Module.Constants.ReadPostsAction);

            }
        }


        #region ISecurityObjectId Members

		/// <inheritdoc/>
        public object SecurityId
        {
            get { return this.ID; }
        }

		/// <inheritdoc/>
        public Type ObjectType
        {
            get { return this.GetType(); }
        }

        #endregion

        #region ISecurityObjectProvider Members

		/// <inheritdoc/>
        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            return new IRole[0];
        }

		/// <inheritdoc/>
        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            throw new NotImplementedException();
        }

		/// <inheritdoc/>
        public bool InheritSupported
        {
            get { return false; }
        }

		/// <inheritdoc/>
        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        #endregion
    }
}
