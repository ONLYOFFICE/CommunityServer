/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Events;
using ASC.Api.Exceptions;
using ASC.Api.Utils;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Notify.Recipients;
using ASC.Web.Community.News;
using ASC.Web.Community.News.Code;
using ASC.Web.Community.News.Code.DAO;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Community.Product;
using ASC.Web.Core.Users;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;

namespace ASC.Api.Community
{
    public partial class CommunityApi
    {
        private IFeedStorage _storage;
        private IFeedStorage FeedStorage
        {
            get
            {
                if (_storage == null)
                {
                    _storage = FeedStorageFactory.Create();
                }
                return _storage;
            }
        }

        ///<summary>
        ///Returns a list of all the portal events with the event titles, dates of creation and update, event texts, and authors.
        ///</summary>
        ///<short>
        ///Get events
        ///</short>
        ///<returns type="ASC.Api.Events.EventWrapper, ASC.Api.Community">List of events</returns>
        ///<category>Events</category>
        ///<path>api/2.0/community/event</path>
        ///<httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("event")]
        public IEnumerable<EventWrapper> GetEvents()
        {
            var feeds = FeedStorage.GetFeeds(FeedType.All, Guid.Empty, (int)_context.Count, (int)_context.StartIndex);
            _context.SetDataPaginated();
            return feeds.Select(x => new EventWrapper(x)).ToSmartList();
        }


        ///<summary>
        ///Creates a new event with the parameters (title, content, type) specified in the request.
        ///</summary>
        ///<short>
        ///Create an event
        ///</short>        
        /// <param type="System.String, System" name="title">Event title</param>
        /// <param type="System.String, System" name="content">Event content</param>
        /// <param type="ASC.Web.Community.News.Code.FeedType, ASC.Web.Community.News.Code" name="type">Event type</param>
        ///<returns type="ASC.Api.Events.EventWrapperFull, ASC.Api.Community">Newly created event</returns>
        ///<path>api/2.0/community/event</path>
        ///<httpMethod>POST</httpMethod>
        ///<category>Events</category>
        [Create("event")]
        public EventWrapperFull CreateEvent(string content, string title, FeedType type)
        {
            CommunitySecurity.DemandPermissions(NewsConst.Action_Add);

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Can't create feed with empty title", "title");

            if (type != FeedType.News && type != FeedType.Order && type != FeedType.Advert && type != FeedType.Poll)
                throw new ArgumentOutOfRangeException(string.Format("Unknown feed type: {0}.", type));

            var feed = new Feed
            {
                Caption = title,
                Text = content,
                Creator = SecurityContext.CurrentAccount.ID.ToString(),
                Date = DateTime.UtcNow,
                FeedType = type
            };

            FeedStorage.SaveFeed(feed, false, type);

            return new EventWrapperFull(feed);
        }

        ///<summary>
        ///Updates the selected event changing the event title, content or/and event type specified in the request.
        ///</summary>
        ///<short>
        ///Update an event
        ///</short>
        /// <param type="System.Int32, System" method="url" name="feedid">Feed ID</param>
        /// <param type="System.String, System" name="title">New event title</param>
        /// <param type="System.String, System" name="content">New event content</param>
        /// <param type="ASC.Web.Community.News.Code.FeedType, ASC.Web.Community.News.Code" name="type">New event type</param>
        ///<returns type="ASC.Api.Events.EventWrapperFull, ASC.Api.Community">List of events</returns>
        ///<category>Events</category>
        ///<path>api/2.0/community/event/{feedid}</path>
        ///<httpMethod>PUT</httpMethod>
        [Update("event/{feedid}")]
        public EventWrapperFull UpdateEvent(int feedid, string content, string title, FeedType type)
        {
            var feed = FeedStorage.GetFeed(feedid).NotFoundIfNull();

            CommunitySecurity.DemandPermissions(feed, NewsConst.Action_Edit);

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Can't update feed with empty title", "title");

            if (type != FeedType.News && type != FeedType.Order && type != FeedType.Advert && type != FeedType.Poll)
                throw new ArgumentOutOfRangeException(string.Format("Unknown feed type: {0}.", type));

            feed.Caption = title;
            feed.Text = content;
            feed.Creator = SecurityContext.CurrentAccount.ID.ToString();

            FeedStorage.SaveFeed(feed, true, type);

            return new EventWrapperFull(feed);
        }

        ///<summary>
        ///Deletes an event with the ID specified in the request.
        ///</summary>
        ///<short>Delete an event</short>
        ///<param type="System.Int32, System" method="url" name="feedid">Feed ID</param>
        ///<returns type="ASC.Api.Events.EventWrapperFull, ASC.Api.Community">Deleted event</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<category>Events</category>
        ///<path>api/2.0/community/event/{feedid}</path>
        ///<httpMethod>DELETE</httpMethod>
        [Delete("event/{feedid}")]
        public EventWrapperFull DeleteEvent(int feedid)
        {
            var feed = FeedStorage.GetFeed(feedid).NotFoundIfNull();

            CommunitySecurity.DemandPermissions(feed, NewsConst.Action_Edit);

            foreach (var comment in FeedStorage.GetFeedComments(feedid))
            {
                CommonControlsConfigurer.FCKUploadsRemoveForItem("news_comments", comment.Id.ToString(CultureInfo.InvariantCulture));
            }

            FeedStorage.RemoveFeed(feed);

            CommonControlsConfigurer.FCKUploadsRemoveForItem("news", feedid.ToString(CultureInfo.InvariantCulture));

            return null;
        }

        ///<summary>
        ///Returns a list of all the events for the current user with the event titles, dates of creation and update, event texts, and author.
        ///</summary>
        ///<short>
        ///Get my events
        ///</short>
        ///<returns type="ASC.Api.Events.EventWrapper, ASC.Api.Community">List of events</returns>
        ///<category>Events</category>
        ///<path>api/2.0/community/event/@self</path>
        ///<httpMethod>GET</httpMethod>
        /// <colletion>list</colletion>
        [Read("event/@self")]
        public IEnumerable<EventWrapper> GetMyEvents()
        {
            var feeds = FeedStorage.GetFeeds(FeedType.All, SecurityContext.CurrentAccount.ID, (int)_context.Count, (int)_context.StartIndex);
            _context.SetDataPaginated();
            return feeds.Select(x => new EventWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of events matching the search query specified in the request with the event titles, dates of creation and update, event types, and authors.
        ///</summary>
        ///<short>
        ///Search events
        ///</short>
        /// <param type="System.String, System" method="url" name="query">Search query</param>
        ///<returns type="ASC.Api.Events.EventWrapper, ASC.Api.Community">List of events</returns>
        ///<category>Events</category>
        ///<path>api/2.0/community/event/@search/{query}</path>
        ///<httpMethod>GET</httpMethod>
        ///<collection>list</collection>
        [Read("event/@search/{query}")]
        public IEnumerable<EventWrapper> SearchEvents(string query)
        {
            var feeds = FeedStorage.SearchFeeds(query, FeedType.All, Guid.Empty, (int)_context.Count, (int)_context.StartIndex);
            _context.SetDataPaginated();
            return feeds.Select(x => new EventWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the detailed information on the event with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Get an event
        ///</short>
        ///<param type="System.Int32, System" method="url" name="feedid">Event ID</param>
        ///<returns type="ASC.Api.Events.EventWrapperFull, ASC.Api.Community">Event information</returns>
        ///<category>Events</category>
        ///<path>api/2.0/community/event/{feedid}</path>
        ///<httpMethod>GET</httpMethod>
        [Read("event/{feedid}")]
        public EventWrapperFull GetEvent(int feedid)
        {
            var feed = FeedStorage.GetFeed(feedid).NotFoundIfNull();
            return new EventWrapperFull(feed);
        }

        ///<summary>
        ///Returns a list of all the comments on the event with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Get event comments
        ///</short>
        ///<param type="System.Int32, System" method="url" name="feedid">Event ID</param>
        ///<returns type="ASC.Api.Events.EventCommentWrapper, ASC.Api.Community">List of comments</returns>
        ///<category>Events</category>
        ///<path>api/2.0/community/event/{feedid}/comment</path>
        ///<httpMethod>GET</httpMethod>
        ///<collection>list</collection>
        [Read("event/{feedid}/comment")]
        public IEnumerable<EventCommentWrapper> GetEventComments(int feedid)
        {
            FeedStorage.GetFeed(feedid).NotFoundIfNull();
            var feedComments = FeedStorage.GetFeedComments(feedid);
            return feedComments.Where(x => !x.Inactive).Select(x => new EventCommentWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Adds a comment to the event with the ID specified in the request. The parent event ID can be also specified if needed.
        ///</summary>
        ///<short>
        ///Add an event comment by feed ID
        ///</short>
        ///<param type="System.Int32, System" method="url" name="feedid">Feed ID</param>
        ///<param type="System.String, System" name="content">Comment text</param>
        ///<param type="System.Int64, System" name="parentId">Comment parent ID</param>
        ///<returns type="ASC.Api.Events.EventCommentWrapper, ASC.Api.Community">Comment</returns>
        /// <example>
        /// <![CDATA[
        /// Sending data in application/json:
        /// 
        /// {
        ///     content:"My comment",
        ///     parentId:"9924256A-739C-462b-AF15-E652A3B1B6EB"
        /// }
        /// 
        /// Sending data in application/x-www-form-urlencoded
        /// content="My comment"&parentId="9924256A-739C-462b-AF15-E652A3B1B6EB"
        /// ]]>
        /// </example>
        /// <remarks>
        /// Send parentId=0 or doesn't send it at all if you want your comment to be on the root level.
        /// </remarks>
        /// <category>Events</category>
        /// <path>api/2.0/community/event/{feedid}/comment</path>
        /// <httpMethod>POST</httpMethod>
        [Create("event/{feedid}/comment")]
        public EventCommentWrapper AddEventComments(int feedid, string content, long parentId)
        {
            if (parentId > 0 && FeedStorage.GetFeedComment(parentId) == null)
                throw new ItemNotFoundException("parent comment not found");

            var feed = FeedStorage.GetFeed(feedid).NotFoundIfNull();

            var comment = new FeedComment(feedid)
            {
                Comment = content,
                Creator = SecurityContext.CurrentAccount.ID.ToString(),
                FeedId = feedid,
                Date = DateTime.UtcNow,
                ParentId = parentId
            };

            FeedStorage.SaveFeedComment(feed, comment);

            return new EventCommentWrapper(comment);
        }

        ///<summary>
        /// Sends a vote for a certain option in a poll-type event with the ID specified in the request.
        ///</summary>
        ///<short>
        /// Vote for an option
        ///</short>
        ///<param type="System.Int32, System" method="url" name="feedid">Event ID</param>
        ///<param type="System.Int64[], System" name="variants">Options</param>
        ///<returns type="ASC.Api.Events.EventWrapperFull, ASC.Api.Community">Event</returns>
        ///<exception cref="ArgumentException">Thrown if not a Poll</exception>
        ///<exception cref="Exception">General error</exception>
        /// <example>
        /// <![CDATA[
        /// Sending data in application/json:
        /// {
        ///     variants:[1,2,3],
        /// }
        ///  Sending data in application/x-www-form-urlencoded
        /// variants=[1,2,3]
        /// ]]>
        /// </example>
        /// <remarks>
        /// If an event is not a poll, then you'll get an error.
        /// </remarks>
        /// <category>Events</category>
        /// <path>api/2.0/community/event/{feedid}/vote</path>
        /// <httpMethod>POST</httpMethod>
        [Create("event/{feedid}/vote")]
        public EventWrapperFull VoteForEvent(int feedid, long[] variants)
        {
            var feed = FeedStorage.GetFeed(feedid).NotFoundIfNull();

            if (feed.FeedType != FeedType.Poll) throw new ArgumentException("Feed is not a poll");
            if (((FeedPoll)feed).IsUserVote(SecurityContext.CurrentAccount.ID.ToString())) throw new ArgumentException("User already voted");

            //Voting
            string error;
            PollVoteHandler.VoteForPoll(variants.ToList(), FeedStorage, feedid, out error);//this method is from 
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);

            return new EventWrapperFull(feed);
        }


        ///<summary>
        /// Subscribes to or unsubscribes from the comments of the event with the ID specified in the request.
        ///</summary>
        ///<short>
        /// Comment subscription
        ///</short>
        ///<param type="System.Boolean, System" name="isSubscribe">Subscribes to the event comments or unsubscribes from them</param>
        ///<param type="System.String, System" method="url" name="feedid">Feed ID</param>
        ///<returns>Boolean value: true means that the user is subscribed to the event comments</returns>
        ///<category>Events</category>
        ///<path>api/2.0/community/event/{feedid}/subscribe</path>
        ///<httpMethod>POST</httpMethod>
        [Create("event/{feedid}/subscribe")]
        public bool SubscribeOnComments(bool isSubscribe, string feedid)
        {
            var subscriptionProvider = NewsNotifySource.Instance.GetSubscriptionProvider();

            var IAmAsRecipient = (IDirectRecipient)NewsNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString());

            if (IAmAsRecipient == null)
            {
                return false;
            }
            if (!isSubscribe)
            {
                subscriptionProvider.Subscribe(NewsConst.NewComment, feedid, IAmAsRecipient);
                return true;
            }
            else
            {
                subscriptionProvider.UnSubscribe(NewsConst.NewComment, feedid, IAmAsRecipient);
                return false;
            }
        }


        /// <summary>
        /// Returns a comment preview with the content specified in the request.
        /// </summary>
        /// <short>Get a comment preview</short>
        /// <section>Comments</section>
        /// <param type="System.String, System" name="commentid">Comment ID</param>
        /// <param type="System.String, System" name="htmltext">Comment text in the HTML format</param>
        /// <returns type="ASC.Web.Studio.UserControls.Common.Comments.CommentInfo, ASC.Web.Studio">Comment information</returns>
        /// <category>Events</category>
        /// <path>api/2.0/community/event/comment/preview</path>
        /// <httpMethod>POST</httpMethod>
        [Create("event/comment/preview")]
        public CommentInfo GetEventCommentPreview(string commentid, string htmltext)
        {
            var storage = FeedStorageFactory.Create();

            var comment = new FeedComment(1)
            {
                Date = TenantUtil.DateTimeNow(),
                Creator = SecurityContext.CurrentAccount.ID.ToString()
            };

            if (!string.IsNullOrEmpty(commentid))
            {
                comment = storage.GetFeedComment(long.Parse(commentid, CultureInfo.CurrentCulture));
            }

            comment.Comment = htmltext;

            var commentInfo = GetCommentInfo(comment);

            commentInfo.IsEditPermissions = false;
            commentInfo.IsResponsePermissions = false;

            return commentInfo;
        }


        /// <summary>
        ///Removes a comment with the ID specified in the request.
        /// </summary>
        /// <short>Remove a comment</short>
        /// <section>Comments</section>
        /// <param type="System.String, System" method="url" name="commentid">Comment ID</param>
        /// <returns>Comment information</returns>
        /// <category>Events</category>
        /// <path>api/2.0/community/event/comment/{commentid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete("event/comment/{commentid}")]
        public string RemoveEventComment(string commentid)
        {
            var storage = FeedStorageFactory.Create();
            var comment = storage.GetFeedComment(long.Parse(commentid, CultureInfo.CurrentCulture));
            if (!CommunitySecurity.CheckPermissions(comment, NewsConst.Action_Edit))
            {
                return null;
            }

            comment.Inactive = true;
            storage.RemoveFeedComment(comment);
            return commentid;
        }


        ///<summary>
        ///Adds a comment to the entity with the ID specified in the request. The parent event ID can be also specified if needed.
        ///</summary>
        ///<short>
        ///Add an event comment by entity ID
        ///</short>
        ///<param type="System.String, System" name="parentcommentid">Comment parent ID</param>
        ///<param type="System.String, System" name="entityid">Entity ID</param>
        ///<param type="System.String, System" name="content">Comment text</param>
        /// <returns type="ASC.Web.Studio.UserControls.Common.Comments.CommentInfo, ASC.Web.Studio">Comment information</returns>
        /// <path>api/2.0/community/event/comment</path>
        /// <httpMethod>POST</httpMethod>
        /// <category>Events</category>
        [Create("event/comment")]
        public CommentInfo AddEventComment(string parentcommentid, string entityid, string content)
        {
            if (String.IsNullOrEmpty(content)) throw new ArgumentException();

            var comment = new FeedComment(long.Parse(entityid))
            {
                Comment = content
            };
            var storage = FeedStorageFactory.Create();
            if (!string.IsNullOrEmpty(parentcommentid))
                comment.ParentId = Convert.ToInt64(parentcommentid);

            var feed = storage.GetFeed(long.Parse(entityid, CultureInfo.CurrentCulture));
            comment = storage.SaveFeedComment(feed, comment);

            return GetCommentInfo(comment);
        }

        ///<summary>
        ///Updates the selected event comment with the content specified in the request.
        ///</summary>
        ///<short>
        ///Update a comment
        ///</short>
        /// <category>Events</category>
        /// <param type="System.String, System" method="url" name="commentid">Comment ID</param>
        /// <param type="System.String, System" name="content">New comment text</param>
        /// <returns>Updated comment</returns>
        /// <path>api/2.0/community/event/comment/{commentid}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("event/comment/{commentid}")]
        public string UpdateComment(string commentid, string content)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentException();

            var storage = FeedStorageFactory.Create();
            var comment = storage.GetFeedComment(long.Parse(commentid, CultureInfo.CurrentCulture));
            if (!CommunitySecurity.CheckPermissions(comment, NewsConst.Action_Edit))
                throw new ArgumentException();

            comment.Comment = content;
            storage.UpdateFeedComment(comment);

            return HtmlUtility.GetFull(content);
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
                CommentBody = HtmlUtility.GetFull(comment.Comment),
                UserFullName = DisplayUserSettings.GetFullUserName(new Guid(comment.Creator)),
                UserProfileLink = CommonLinkUtility.GetUserProfile(comment.Creator),
                UserAvatarPath = UserPhotoManager.GetBigPhotoURL(new Guid(comment.Creator)),
                IsEditPermissions = CommunitySecurity.CheckPermissions(comment, NewsConst.Action_Edit),
                IsResponsePermissions = CommunitySecurity.CheckPermissions(NewsConst.Action_Comment),
                UserPost = CoreContext.UserManager.GetUsers((new Guid(comment.Creator))).Title
            };

            return info;
        }

    }
}
