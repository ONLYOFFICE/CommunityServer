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
using ASC.Core;
using ASC.Core.Users;

#endregion

namespace ASC.Forum
{
    public enum PostTextFormatter
    { 
        BBCode = 0,
        FCKEditor =1
    }

	public class Post : ISecurityObject
    {
        public int ID { get; set; }

        public Guid PosterID { get; set; }

        public UserInfo Poster
        {
            get { return CoreContext.UserManager.GetUsers(PosterID); }
        }

        public Guid EditorID { get; set; }

        public UserInfo Editor
        {
            get { return CoreContext.UserManager.GetUsers(EditorID); }
        }

        public string Subject { get; set; }

        public string Text { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime EditDate { get; set; }

        public int EditCount { get; set; }       

        public bool IsApproved { get; set; }

        public List<Attachment> Attachments { get; set; }

        public PostTextFormatter Formatter { get; set; }

        public int TenantID { get; set; }

        public int TopicID { get; set; }

        public int ParentPostID { get; set; }

        public Topic Topic { get; set; }


        public Post()
        {
            Attachments = new List<Attachment>();
            CreateDate = Core.Tenants.TenantUtil.DateTimeNow();
            EditDate = DateTime.MinValue;            
            EditCount = 0;
            Formatter = PostTextFormatter.BBCode;
            PosterID = SecurityContext.CurrentAccount.ID;
        }

        public Post(string subject, string text)
        {
            Attachments = new List<Attachment>();
            CreateDate = Core.Tenants.TenantUtil.DateTimeNow();
            EditDate = DateTime.MinValue;            
            EditCount = 0;
            Subject = subject;
            Text = text;
            PosterID = SecurityContext.CurrentAccount.ID;
        }

        #region ISecurityObjectId Members

        public object SecurityId
        {
            get { return ID; }
        }

        public Type ObjectType
        {
            get { return GetType(); }
        }

        #endregion

        #region ISecurityObjectProvider Members

        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            var roles = new List<IRole>();
            if (account.ID.Equals(PosterID))
                roles.Add(Common.Security.Authorizing.Constants.Owner);

            return roles;
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            return new Topic {ID = TopicID };
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
