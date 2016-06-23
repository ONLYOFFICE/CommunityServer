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


#region Usings

using System;
using System.Collections.Generic;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;

#endregion

namespace ASC.Blogs.Core.Domain
{
    public class CommentStat 
    {
        public Guid PostId;
        public Guid ForUserID;
        public int TotalCount;
        public int UndreadCount;
    }

	public class Comment : ISecurityObject
    {
        #region members

        private Guid id;
	    private Post post;
	    private IList<Comment> commentList = new List<Comment>();

        #endregion

        public Comment() { }
        public Comment(Post post)
        {
            this.post = post;
            //_Blog.AddComment(this);
        }


        public bool IsRoot() { return ParentId == Guid.Empty; }
        
        public List<Comment> SelectChildLevel(List<Comment> from)
        {
            return SelectChildLevel(ID, from);
        }
        
        public static List<Comment> SelectRootLevel(List<Comment> from)
        {
            return SelectChildLevel(Guid.Empty, from);
        }
        
        public static List<Comment> SelectChildLevel(Guid forParentId, List<Comment> from)
        {
            return from.FindAll(comm => comm.ParentId == forParentId);
        }
        
        #region Properties
        
        public Guid PostId { get; set; }
        
        public Guid ParentId { get; set; }
       
        public virtual Guid ID
        {
            get { return id; }
            set { id = value; }
        }

        public virtual Post Post
        {
            get { return post; }
            set { post = value; }
        }

	    public virtual bool Inactive { get; set; }

	    public virtual Guid UserID { get; set; }

	    public virtual string Content { get; set; }

	    public virtual DateTime Datetime { get; set; }

	    public virtual bool IsReaded { get; set; }
        
        public virtual IList<Comment> CommentList
        {
            get { return new List<Comment>(commentList).AsReadOnly(); }
            protected set { commentList = value; }
        }

        #endregion

        #region Methods

        public override int GetHashCode()
        {
            return (GetType().FullName + "|" + id.ToString()).GetHashCode();
        }

        #endregion

        #region ISecurityObjectId Members

        public Type ObjectType
        {
            get { return GetType(); }
        }

        public object SecurityId
        {
            get { return ID; }
        }

        #endregion

        #region ISecurityObjectProvider Members

        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            var roles = new List<IRole>();
            if (Equals(account.ID, UserID))
            {
                roles.Add(Common.Security.Authorizing.Constants.Owner);
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

    public class CommentSecurityObjectProvider : ISecurityObjectProvider
    {
        readonly Guid author = Guid.Empty;

        public CommentSecurityObjectProvider(Comment comment)
        {
            author = comment.UserID;
        }
        public CommentSecurityObjectProvider(Guid author)
        {
            this.author = author;
        }

        #region ISecurityObjectProvider

        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            var roles = new List<IRole>();
            if (Equals(account.ID, author))
                roles.Add(Common.Security.Authorizing.Constants.Owner);
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
