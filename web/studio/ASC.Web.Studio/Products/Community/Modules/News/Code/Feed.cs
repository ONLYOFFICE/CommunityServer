/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.Web.Community.News.Code
{
    [Serializable]
    public class Feed : ISecurityObject
    {
        public long Id { get; set; }

        public FeedType FeedType { get; set; }

        public string Caption { get; set; }

        public string Text { get; set; }

        public DateTime Date { get; set; }

        public string Creator { get; set; }

        public virtual bool Readed { get; internal set; }


        public Feed()
        {
            FeedType = FeedType.News;
            Creator = SecurityContext.CurrentAccount.ID.ToString();
            Date = TenantUtil.DateTimeNow();
            Readed = false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var feed = obj as Feed;
            return feed != null && Id == feed.Id;
        }

        public override string ToString()
        {
            return Caption;
        }

        public Type ObjectType
        {
            get { return GetType(); }
        }

        public object SecurityId
        {
            get { return Id; }
        }

        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId,
                                                 SecurityCallContext callContext)
        {
            var roles = new List<IRole>();

            if (Equals(account.ID, new Guid(Creator)))
            {
                roles.Add(Constants.Owner);
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
    }
}