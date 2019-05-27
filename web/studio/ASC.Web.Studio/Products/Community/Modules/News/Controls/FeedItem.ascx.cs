/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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