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
                profileLink.Text = CoreContext.UserManager.GetUsers(new Guid(Feed.Creator)).RenderCustomProfileLink("", "linkMedium");
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
                var feed = storage.GetFeed(Convert.ToInt64(id, CultureInfo.CurrentCulture));
                storage.RemoveFeed(feed);

                CommonControlsConfigurer.FCKUploadsRemoveForItem("news", id);

				resp.rs1 = id;
				resp.rs2 = NewsResource.FeedDeleted;
			}
			return resp;
		}
	}
}