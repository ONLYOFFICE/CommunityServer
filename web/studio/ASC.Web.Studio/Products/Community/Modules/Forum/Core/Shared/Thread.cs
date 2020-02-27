/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Web.Community.Product;

namespace ASC.Forum
{
	public class Thread : ISecurityObject
    {
        public virtual int ID { get; set; }

        public virtual string Title { get; set; }

        public virtual string Description { get; set; }

        public virtual int SortOrder{get; set;}

        public virtual int PostCount { get; set; }

        public virtual int TopicCount { get; set; }

        public virtual bool IsApproved { get; set; }

        public virtual int TenantID { get; set; }

        public virtual int CategoryID { get; set; }

        public virtual int RecentPostID { get; set; }

        public virtual DateTime RecentPostCreateDate { get; set; }

        public virtual DateTime RecentTopicCreateDate { get; set; }

        public virtual Guid RecentPosterID { get; set; }

        public virtual string RecentTopicTitle { get; set; }

        public virtual int RecentTopicPostCount { get; set; }

        public virtual int RecentTopicID { get; set; }

        public Thread()
        {
            IsApproved = true;
            TopicLastUpdates = new List<TopicLastUpdate>();
        }

        public virtual bool Visible 
        {
            get 
            {
                return CommunitySecurity.CheckPermissions(this, Module.Constants.ReadPostsAction);
            }
        }

        internal class TopicLastUpdate
        {
            public int TopicID { get; set; }
            public int RecentPostID { get; set; }
            public DateTime RecentPostCreateDate { get; set; }

        }

        internal List<TopicLastUpdate> TopicLastUpdates { get; set; }

        public bool IsNew()
        {
            if (this.RecentPostID <= 0)
                return false;

            var tvi = ThreadVisitInfo.GetThreadVisitInfo(this.ID);
            if (tvi == null)
                return true;

            if (this.RecentPostCreateDate != DateTime.MinValue
                && tvi.RecentVisitDate != DateTime.MinValue
                && tvi.RecentVisitDate.CompareTo(this.RecentPostCreateDate) >= 0)
            {
                return false;
            }

            if (tvi.TopicViewRecentPostIDs.Count == 0)
                return true;

            foreach (var topicLastUpdate in TopicLastUpdates)
            {
                if (tvi.TopicViewRecentPostIDs.ContainsKey(topicLastUpdate.TopicID))
                {
                    if(tvi.TopicViewRecentPostIDs[topicLastUpdate.TopicID] < topicLastUpdate.RecentPostID)
                        return true;                                    
                }

                else if (tvi.RecentVisitDate != DateTime.MinValue)
                {
                    if(tvi.RecentVisitDate.CompareTo(topicLastUpdate.RecentPostCreateDate) < 0)                
                        return true;                    
                }
                
                else                
                    return true;                    
                
            }

            return false;
        }


        #region ISecurityObjectId Members

		/// <inheritdoc/>
        public object SecurityId
        {
            get { return this.ID; }
        }

		/// <inheritdoc/>
        public System.Type ObjectType
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
            return new ThreadCategory() {ID = this.CategoryID};
        }

		/// <inheritdoc/>
        public bool InheritSupported
        {
            get { return true; }
        }

		/// <inheritdoc/>
        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        #endregion
    }
}
