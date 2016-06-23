/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
