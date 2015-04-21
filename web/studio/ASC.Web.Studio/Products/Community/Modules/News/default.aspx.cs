/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Globalization;
using System.Text;
using System.Web;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility.HtmlUtility;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Community.News.Code;
using ASC.Web.Community.News.Code.DAO;
using ASC.Web.Community.News.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using FeedNS = ASC.Web.Community.News.Code;

namespace ASC.Web.Community.News
{
    [AjaxNamespace("Default")]
    public partial class Default : MainPage
    {
        //private BBCodeParser.Parser postParser = new BBCodeParser.Parser(CommonControlsConfigurer.SimpleTextConfig);

        private RequestInfo info;

        public RequestInfo Info
        {
            get { return info ?? (info = new RequestInfo(Request)); }
        }

        public int PageNumber
        {
            get { return ViewState["PageNumber"] != null ? Convert.ToInt32(ViewState["PageNumber"]) : 0; }
            set { ViewState["PageNumber"] = value; }
        }

        public int PageSize
        {
            get { return ViewState["PageSize"] != null ? Convert.ToInt32(ViewState["PageSize"]) : 20; }
            set { ViewState["PageSize"] = value; }
        }

        public long FeedsCount { get; set; }

        protected string EventTitle { get; set; }
        protected string StatusSubscribe { get; set; }

        protected Uri FeedItemUrlWithParam
        {
            get { return new Uri("~/products/community/modules/news/editnews.aspx?docID=" + Info.UserIdAttribute, UriKind.Relative); }

        }

        protected void Page_Load(object sender, EventArgs e)
        {            
            Utility.RegisterTypeForAjax(typeof(Default), Page);
            commentList.Visible = CommunitySecurity.CheckPermissions(NewsConst.Action_Comment);

            pgNavigator.EntryCount = 1;
            pgNavigator.EntryCountOnPage = 1;

            if (IsPostBack) return;

            var storage = FeedStorageFactory.Create();
            if (!string.IsNullOrEmpty(Request["docID"]))
            {
                long docID;
                if (long.TryParse(Request["docID"], out docID))
                {
                    //Show panel
                    ContentView.Visible = false;
                    FeedView.Visible = true;

                    var feed = storage.GetFeed(docID);
                    if (feed != null)
                    {

                        if (!feed.Readed)
                        {
                            storage.ReadFeed(feed.Id, SecurityContext.CurrentAccount.ID.ToString());
                        }
                        FeedViewCtrl.Feed = feed;
                        hdnField.Value = feed.Id.ToString(CultureInfo.CurrentCulture);
                        InitCommentsView(storage, feed);
                        FeedView.DataBind();
                        EventTitle = feed.Caption;
                        var subscriptionProvider = NewsNotifySource.Instance.GetSubscriptionProvider();
                        var amAsRecipient = (IDirectRecipient)NewsNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString());
                        var isSubsribedOnComments = subscriptionProvider.IsSubscribed(NewsConst.NewComment, amAsRecipient, feed.Id.ToString());

                        var SubscribeTopicLink = string.Format(CultureInfo.CurrentCulture,
                                                               string.Format(CultureInfo.CurrentCulture,
                                                                             "<a id=\"statusSubscribe\" class=\"follow-status " +
                                                                             (isSubsribedOnComments ? "subscribed" : "unsubscribed") +
                                                                             "\" title=\"{0}\" href=\"#\" onclick=\"SubscribeOnComments('{1}');\"></a>",
                                                                             (isSubsribedOnComments ? NewsResource.UnsubscribeFromNewComments : NewsResource.SubscribeOnNewComments), feed.Id));

                        SubscribeLinkBlock.Text = SubscribeTopicLink;

                        Title = HeaderStringHelper.GetPageTitle((Master as NewsMaster).CurrentPageCaption ?? feed.Caption);
                    }
                    else
                    {
                        Response.Redirect(VirtualPathUtility.ToAbsolute("~/products/community/modules/news/"));
                        ContentView.Visible = true;
                        FeedView.Visible = false;
                        FeedRepeater.Visible = true;
                    }
                }
            }
            else
            {
                PageNumber = string.IsNullOrEmpty(Request["page"]) ? 1 : Convert.ToInt32(Request["page"]);
                PageSize = string.IsNullOrEmpty(Request["size"]) ? 20 : Convert.ToInt32(Request["size"]);
                LoadData();
            }
            InitScripts();
        }

        private void InitScripts()
        {
            var jsResource = new StringBuilder();
            jsResource.Append("jq('#tableForNavigation select').val(" + PageSize + ").change(function(evt) {changeCountOfRows(this.value);}).tlCombobox();");
            Page.RegisterInlineScript(jsResource.ToString(), true);
        }

        private void InitCommentsView(IFeedStorage storage, FeedNS.Feed feed)
        {
            IList<CommentInfo> comments = BuildCommentsList(storage.GetFeedComments(feed.Id));

            //AppendChildsComments(ref comments, storage.GetFeedComments(feed.Id));

            ConfigureComments(commentList, feed);
            commentList.Items = comments;
            commentList.CommentsCountTitle = GetCommentsCount(comments).ToString(CultureInfo.CurrentCulture);
            commentList.TotalCount = GetCommentsCount(comments);
        }

        private static void ConfigureComments(CommentsList commentList, FeedNS.Feed feed)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);
            commentList.Simple = false;
            commentList.BehaviorID = "_commentsObj";
            commentList.FckDomainName = "news_comments";

            commentList.JavaScriptAddCommentFunctionName = "Default.AddComment";
            commentList.JavaScriptPreviewCommentFunctionName = "Default.GetPreview";
            commentList.JavaScriptRemoveCommentFunctionName = "Default.RemoveComment";
            commentList.JavaScriptUpdateCommentFunctionName = "Default.UpdateComment";
            commentList.JavaScriptLoadBBcodeCommentFunctionName = "Default.LoadCommentText";

            commentList.ObjectID = feed != null ? feed.Id.ToString(CultureInfo.CurrentCulture) : "";
        }

        private static int GetCommentsCount(ICollection<CommentInfo> comments)
        {
            var count = comments.Count;
            foreach (var info in comments)
            {
                count += GetCommentsCount(info.CommentList);
            }
            return count;
        }

        public static string GetHtmlImgUserAvatar(Guid userId)
        {
            var imgPath = UserPhotoManager.GetBigPhotoURL(userId);
            if (imgPath != null) return "<img class=\"userMiniPhoto\"  src=\"" + imgPath + "\"/>";
            return string.Empty;
        }

        private static CommentInfo GetCommentInfo(FeedComment comment)
        {
            var info = new CommentInfo
                {
                    CommentID = comment.Id.ToString(CultureInfo.CurrentCulture),
                    UserID = new Guid(comment.Creator),
                    TimeStamp = comment.Date,
                    TimeStampStr = comment.Date.Ago(),
                    IsRead = true,
                    Inactive = comment.Inactive,
                    CommentBody = comment.Comment,
                    UserFullName = DisplayUserSettings.GetFullUserName(new Guid(comment.Creator)),
                    UserAvatar = GetHtmlImgUserAvatar(new Guid(comment.Creator)),
                    IsEditPermissions = CommunitySecurity.CheckPermissions(comment, NewsConst.Action_Edit),
                    IsResponsePermissions = CommunitySecurity.CheckPermissions(NewsConst.Action_Comment),
                    UserPost = CoreContext.UserManager.GetUsers((new Guid(comment.Creator))).Title
                };

            return info;
        }

        #region Ajax functions for comments management

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string RemoveComment(string commentId, string pid)
        {
            var storage = FeedStorageFactory.Create();
            var comment = storage.GetFeedComment(long.Parse(commentId, CultureInfo.CurrentCulture));
            if (!CommunitySecurity.CheckPermissions(comment, NewsConst.Action_Edit))
            {
                return null;
            }

            comment.Inactive = true;
            storage.RemoveFeedComment(comment);
            return commentId;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse AddComment(string parentCommentId, string newsId, string text, string pid)
        {
            var resp = new AjaxResponse { rs1 = parentCommentId };

            var comment = new FeedComment(long.Parse(newsId));
            comment.Comment = text;
            var storage = FeedStorageFactory.Create();
            if (!string.IsNullOrEmpty(parentCommentId))
                comment.ParentId = Convert.ToInt64(parentCommentId);

            var feed = storage.GetFeed(long.Parse(newsId, CultureInfo.CurrentCulture));
            comment = storage.SaveFeedComment(feed, comment);

            var commentInfo = GetCommentInfo(comment);
            var defComment = new CommentsList();
            ConfigureComments(defComment, feed);

            var visibleCommentsCount = 0;
            storage.GetFeedComments(feed.Id).ForEach((cmm) => { visibleCommentsCount += (cmm.Inactive ? 0 : 1); });

            resp.rs2 = CommentsHelper.GetOneCommentHtmlWithContainer(
                defComment, commentInfo, comment.IsRoot(), visibleCommentsCount%2 == 1);

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse UpdateComment(string commentId, string text, string pid)
        {
            var resp = new AjaxResponse();
            if (string.IsNullOrEmpty(text)) return resp;

            var storage = FeedStorageFactory.Create();
            var comment = storage.GetFeedComment(long.Parse(commentId, CultureInfo.CurrentCulture));
            if (!CommunitySecurity.CheckPermissions(comment, NewsConst.Action_Edit))
            {
                return resp;
            }

            comment.Comment = text;
            storage.UpdateFeedComment(comment);

            resp.rs1 = commentId;
            resp.rs2 = HtmlUtility.GetFull(text);

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string LoadCommentText(string commentId, string pid)
        {
            var storage = FeedStorageFactory.Create();
            var comment = storage.GetFeedComment(long.Parse(commentId, CultureInfo.CurrentCulture));
            return comment.Comment;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string GetPreview(string text, string commentID)
        {
            var storage = FeedStorageFactory.Create();

            var comment = new FeedComment(1)
                {
                    Date = TenantUtil.DateTimeNow(),
                    Creator = SecurityContext.CurrentAccount.ID.ToString()
                };

            if (!string.IsNullOrEmpty(commentID))
            {
                comment = storage.GetFeedComment(long.Parse(commentID, CultureInfo.CurrentCulture));
            }

            comment.Comment = text;

            var commentInfo = GetCommentInfo(comment);

            commentInfo.IsEditPermissions = false;
            commentInfo.IsResponsePermissions = false;

            var defComment = new CommentsList();
            ConfigureComments(defComment, null);

            return CommentsHelper.GetOneCommentHtmlWithContainer(
                defComment, commentInfo, true, false);
        }

        #endregion

        private static List<CommentInfo> BuildCommentsList(List<FeedComment> loaded)
        {
            return BuildCommentsList(loaded, 0);
        }

        private static List<CommentInfo> BuildCommentsList(List<FeedComment> loaded, long parentId)
        {
            var result = new List<CommentInfo>();
            foreach (var comment in FeedComment.SelectChildLevel(parentId, loaded))
            {
                var info = GetCommentInfo(comment);
                info.CommentList = BuildCommentsList(loaded, comment.Id);

                result.Add(info);
            }
            return result;
        }

        private void LoadData()
        {
            var storage = FeedStorageFactory.Create();
            var feedType = FeedType.All;

            if (!string.IsNullOrEmpty(Request["type"]))
            {
                feedType = (FeedType)Enum.Parse(typeof(FeedType), Request["type"], true);
                var feedTypeInfo = FeedTypeInfo.FromFeedType(feedType);
                Title = HeaderStringHelper.GetPageTitle((Master as NewsMaster).CurrentPageCaption ?? feedTypeInfo.TypeName);
            }
            else
            {
                Title = HeaderStringHelper.GetPageTitle((Master as NewsMaster).CurrentPageCaption ?? NewsResource.NewsBreadCrumbs);
            }

            var feedsCount = !string.IsNullOrEmpty(Request["search"]) ? storage.SearchFeedsCount(Request["search"], feedType, Info.UserId) : storage.GetFeedsCount(feedType, Info.UserId);

            FeedsCount = feedsCount;

            if (feedsCount == 0)
            {
                FeedRepeater.Visible = false;
                MessageShow.Visible = true;

                string buttonLink;
                string buttonName;
                var emptyScreenControl = new EmptyScreenControl { Describe = NewsResource.EmptyScreenText };

                switch (feedType)
                {
                    case FeedType.News:
                        emptyScreenControl.ImgSrc = WebImageSupplier.GetAbsoluteWebPath("150x_news.png", NewsConst.ModuleId);
                        emptyScreenControl.Header = NewsResource.EmptyScreenNewsCaption;
                        buttonLink = FeedUrls.EditNewsUrl;
                        buttonName = NewsResource.EmptyScreenNewsLink;
                        break;
                    case FeedType.Order:
                        emptyScreenControl.ImgSrc = WebImageSupplier.GetAbsoluteWebPath("150x_order.png", NewsConst.ModuleId);
                        emptyScreenControl.Header = NewsResource.EmptyScreenOrdersCaption;
                        buttonLink = FeedUrls.EditOrderUrl;
                        buttonName = NewsResource.EmptyScreenOrderLink;
                        break;
                    case FeedType.Advert:
                        emptyScreenControl.ImgSrc = WebImageSupplier.GetAbsoluteWebPath("150x_advert.png", NewsConst.ModuleId);
                        emptyScreenControl.Header = NewsResource.EmptyScreenAdvertsCaption;
                        buttonLink = FeedUrls.EditAdvertUrl;
                        buttonName = NewsResource.EmptyScreenAdvertLink;
                        break;
                    case FeedType.Poll:
                        emptyScreenControl.ImgSrc = WebImageSupplier.GetAbsoluteWebPath("150x_poll.png", NewsConst.ModuleId);
                        emptyScreenControl.Header = NewsResource.EmptyScreenPollsCaption;
                        buttonLink = FeedUrls.EditPollUrl;
                        buttonName = NewsResource.EmptyScreenPollLink;
                        break;
                    default:
                        emptyScreenControl.ImgSrc = WebImageSupplier.GetAbsoluteWebPath("150x_newslogo.png", NewsConst.ModuleId);
                        emptyScreenControl.Header = NewsResource.EmptyScreenEventsCaption;
                        buttonLink = FeedUrls.EditNewsUrl;
                        buttonName = NewsResource.EmptyScreenEventLink;
                        break;
                }

                if (CommunitySecurity.CheckPermissions(NewsConst.Action_Add) && String.IsNullOrEmpty(Request["uid"]) && String.IsNullOrEmpty(Request["search"]))
                    emptyScreenControl.ButtonHTML = String.Format("<a class='link underline blue plus' href='{0}'>{1}</a>", buttonLink, buttonName);


                MessageShow.Controls.Add(emptyScreenControl);
            }
            else
            {
                var pageSize = PageSize;
                var pageCount = (int)(feedsCount/pageSize + 1);
                if (pageCount < PageNumber) PageNumber = pageCount;

                var feeds = !string.IsNullOrEmpty(Request["search"]) ?
                                storage.SearchFeeds(Request["search"], feedType, Info.UserId, pageSize, (PageNumber - 1)*pageSize) :
                                storage.GetFeeds(feedType, Info.UserId, pageSize, (PageNumber - 1)*pageSize);

                pgNavigator.EntryCountOnPage = pageSize;
                pgNavigator.EntryCount = 0 < pageCount ? (int)feedsCount : pageSize;
                pgNavigator.CurrentPageNumber = PageNumber;

                pgNavigator.ParamName = "page";
                if (!string.IsNullOrEmpty(Request["search"]))
                {
                    pgNavigator.PageUrl = string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}?search={1}&size={2}",
                        VirtualPathUtility.ToAbsolute("~/products/community/modules/news/"),
                        Request["search"],
                        pageSize
                        );
                }
                else
                {
                    pgNavigator.PageUrl = string.IsNullOrEmpty(Request["type"]) ?
                                              string.Format(
                                                  CultureInfo.CurrentCulture,
                                                  "{0}?{1}&size={2}",
                                                  VirtualPathUtility.ToAbsolute("~/products/community/modules/news/"),
                                                  (string.IsNullOrEmpty(Info.UserIdAttribute) ? string.Empty : "?" + Info.UserIdAttribute.Substring(1)),
                                                  pageSize
                                                  ) :
                                              string.Format(
                                                  CultureInfo.CurrentCulture,
                                                  "{0}?type={1}{2}&size={3}",
                                                  VirtualPathUtility.ToAbsolute("~/products/community/modules/news/"),
                                                  Request["type"],
                                                  Info.UserIdAttribute,
                                                  pageSize);
                }
                FeedRepeater.DataSource = feeds;
                FeedRepeater.DataBind();
            }
        }
    }
}