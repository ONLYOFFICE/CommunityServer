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
