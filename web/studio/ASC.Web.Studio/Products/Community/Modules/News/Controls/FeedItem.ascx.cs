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

using System;
using System.Globalization;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Community.News.Code;
using ASC.Web.Community.News.Code.DAO;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Community.News.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using FeedNS = ASC.Web.Community.News.Code;

namespace ASC.Web.Community.News.Controls
{
	[AjaxNamespace("FeedItem")]
	public partial class FeedItem : UserControl
	{
		public FeedNS.Feed Feed { get; set; }
		public bool IsEditVisible { get; set; }
		public string FeedLink { get; set; }
		public Uri RemoveUrlWithParam { get; set; }
		public Uri EditUrlWithParam { get; set; }
		public string FeedType { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public DateTime ExpirationTime { get; set; }
		public int PollVotes { get; set; }

		protected Guid RequestedUserId
		{
			get
			{
				Guid result = Guid.Empty;
				try
				{
					result = new Guid(Request["uid"]);
				}
				catch { }

				return result;
			}
		}

		protected string UserIdAttribute
		{
			get
			{
				if (!RequestedUserId.Equals(Guid.Empty))
				{
					return string.Format(CultureInfo.CurrentCulture, "?uid={0}", RequestedUserId);
				}
				return string.Empty;
			}

		}


		protected void Page_Load(object sender, EventArgs e)
		{
			Utility.RegisterTypeForAjax(typeof(FeedItem), this.Page);
		}

		public override void DataBind()
		{
			base.DataBind();

			if (Feed != null)
			{
				Date.Text = Feed.Date.ToShortDateString();
				NewsLink.NavigateUrl = FeedLink;
				NewsLink.Text = Feed.Caption.HtmlEncode();
                Type.Text = FeedTypeInfo.FromFeedType(Feed.FeedType).TypeName;
                profileLink.Text = CoreContext.UserManager.GetUsers(new Guid(Feed.Creator)).RenderCustomProfileLink(CommunityProduct.ID, "", "linkMedium");
			}
		}

		private string GetEditUrl()
		{
			return (Feed is FeedPoll ? FeedUrls.EditPollUrl : FeedUrls.EditNewsUrl) + UserIdAttribute;
		}

		[AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public AjaxResponse Remove(string id)
		{
			AjaxResponse resp = new AjaxResponse();
			resp.rs1 = "0";
			if (!string.IsNullOrEmpty(id))
			{
                CommunitySecurity.DemandPermissions(NewsConst.Action_Edit);

				var storage = FeedStorageFactory.Create();
				storage.RemoveFeed(Convert.ToInt64(id, CultureInfo.CurrentCulture));

                CommonControlsConfigurer.FCKUploadsRemoveForItem("news", id);

				resp.rs1 = id;
				resp.rs2 = NewsResource.FeedDeleted;
			}
			return resp;
		}
	}
}