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
using System.Text;
using ASC.Web.Core.Subscriptions;
using ASC.Bookmarking.Common;
using ASC.Bookmarking.Resources;
using ASC.Core;
using System.Web;
using ASC.Notify.Recipients;


namespace ASC.Bookmarking.Business.Subscriptions
{
	public class BookmarkingSubscriptionManager : ISubscriptionManager
	{	

		public BookmarkingSubscriptionManager()
		{
			
		}

		#region ISubscriptionManager Members

		#region Subscription Objects
		private List<SubscriptionObject> GetNewBookmarkSubscriptionObjects(Guid productID, Guid moduleOrGroupID, Guid typeID)
		{
			return GetSubscriptionObjects(true, false);
		}

		private List<SubscriptionObject> GetNewCommentSubscriptionObjects(Guid productID, Guid moduleOrGroupID, Guid typeID)
		{
			return GetSubscriptionObjects(false, true);
		}

		private List<SubscriptionObject> GetSubscriptionObjects(bool includeBookmarks, bool includeComments)
		{
			List<SubscriptionObject> subscriptionObjects = new List<SubscriptionObject>();

			IList<string> list = new List<string>();

			var currentAccount = SecurityContext.CurrentAccount;

			if (includeBookmarks)
			{

				list = new List<string>(
					SubscriptionProvider.GetSubscriptions(
						BookmarkingBusinessConstants.NotifyActionNewBookmark,
						new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name), false)
						);
				if (list.Count > 0)
				{
					foreach (string id in list)
					{
						if (!string.IsNullOrEmpty(id))
						{
							subscriptionObjects.Add(new SubscriptionObject()
							{
								ID = id,
								Name = BookmarkingBusinessResources.SubscriptionTypeNewBookmark,
								URL = string.Empty,
                                SubscriptionType = GetSubscriptionTypes()[0]
							});
						}
					}
				}
			}

			if (includeComments)
			{

				list = new List<string>(
					SubscriptionProvider.GetSubscriptions(
						BookmarkingBusinessConstants.NotifyActionNewComment,
						new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name), false)
						);
				if (list.Count > 0)
				{
					var service = BookmarkingService.GetCurrentInstanse();
					foreach (string id in list)
					{
						if (!string.IsNullOrEmpty(id))
						{
							try
							{
								var bookmark = service.GetBookmarkByID(Int32.Parse(id));
								subscriptionObjects.Add(new SubscriptionObject()
								{
									ID = id,
									Name = bookmark.Name,
									URL = VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Bookmarking/BookmarkInfo.aspx") + "?Url=" + HttpUtility.UrlEncode(bookmark.URL),
                                    SubscriptionType = GetSubscriptionTypes()[1]
								});
							}
							catch { }
						}
					}
				}
			}
			return subscriptionObjects;
		}



		public List<SubscriptionObject> GetSubscriptionObjects(Guid subItem)
		{
			return GetSubscriptionObjects(true, true);

		} 
		#endregion

		#region Subscription Types
		public List<SubscriptionType> GetSubscriptionTypes()
		{
            var subscriptionTypes = new List<SubscriptionType>();
            subscriptionTypes.Add(new SubscriptionType()
            {
				ID = BookmarkingBusinessConstants.NotifyActionNewBookmarkID,
                NotifyAction = BookmarkingBusinessConstants.NotifyActionNewBookmark,
                Name = BookmarkingBusinessResources.SubscriptionTypeNewBookmark,
                GetSubscriptionObjects = GetNewBookmarkSubscriptionObjects,
                IsEmptySubscriptionType = IsEmptyNewBookmarkSubscriptionType,
                Single = true
            });

            subscriptionTypes.Add(new SubscriptionType()
            {
				ID = BookmarkingBusinessConstants.NotifyActionNewCommentID,
                NotifyAction = BookmarkingBusinessConstants.NotifyActionNewComment,
                Name = BookmarkingBusinessResources.SubscriptionTypeNewComments,
                GetSubscriptionObjects = GetNewCommentSubscriptionObjects,
                IsEmptySubscriptionType = IsEmptyNewCommentsSubscriptionType
            });

			return subscriptionTypes;
		}

		private bool IsEmptyNewBookmarkSubscriptionType(Guid productID, Guid moduleOrGroupID, Guid typeID)
		{
			var currentAccount = SecurityContext.CurrentAccount;
			var list = new List<string>(
					SubscriptionProvider.GetSubscriptions(
						BookmarkingBusinessConstants.NotifyActionNewBookmark,
						new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name), false)
						);
			return list.Count == 0;			
		}

		private bool IsEmptyNewCommentsSubscriptionType(Guid productID, Guid moduleOrGroupID, Guid typeID)
		{
			var currentAccount = SecurityContext.CurrentAccount;
			var list = new List<string>(
					SubscriptionProvider.GetSubscriptions(
						BookmarkingBusinessConstants.NotifyActionNewComment,
						new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name), false)
						);
			return list.Count == 0;
		}
		#endregion

		public ASC.Notify.Model.ISubscriptionProvider SubscriptionProvider
		{
			get
			{
				return BookmarkingNotifySource.Instance.GetSubscriptionProvider();
			}
		}

		#endregion
	}
}
