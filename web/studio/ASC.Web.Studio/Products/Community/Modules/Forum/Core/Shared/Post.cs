/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
