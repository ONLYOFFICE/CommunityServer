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
