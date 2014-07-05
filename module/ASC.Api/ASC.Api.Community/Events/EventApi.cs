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
using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Events;
using ASC.Api.Exceptions;
using ASC.Api.Utils;
using ASC.Core;
using ASC.Web.Community.News;
using ASC.Web.Community.News.Code;
using ASC.Web.Community.News.Code.DAO;
using ASC.Web.Community.Product;

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
        ///Returns the list of all events on the portal with the event titles, date of creation and update, event text and author
        ///</summary>
        ///<short>
        ///All events
        ///</short>
        ///<returns>list of events</returns>
        ///<category>Events</category>
        [Read("event")]
        public IEnumerable<EventWrapper> GetEvents()
        {
            var feeds = FeedStorage.GetFeeds(FeedType.All, Guid.Empty, (int)_context.Count, (int)_context.StartIndex);
            _context.SetDataPaginated();
            return feeds.Select(x => new EventWrapper(x)).ToSmartList();
        }


        ///<summary>
        ///Creates a new event with the parameters (title, content, type) specified in the request
        ///</summary>
        ///<short>
        ///Create event
        ///</short>        
        /// <param name="title">Title</param>
        /// <param name="content">Content</param>
        /// <param name="type">Type. One of  (News|Order|Advert|Poll)</param>
        ///<returns>New created event</returns>
        ///<category>Events</category>
        [Create("event")]
        public EventWrapperFull CreateEvent(string content, string title, FeedType type)
        {
            CommunitySecurity.DemandPermissions(NewsConst.Action_Add);

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Can't create feed with empty title", "title");

            var feed = new Web.Community.News.Code.Feed
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
        ///Updates the selected event changing the event title, content or/and event type specified
        ///</summary>
        ///<short>
        ///Update event
        ///</short>
        /// <param name="feedid">Feed ID</param>
        /// <param name="title">Title</param>
        /// <param name="content">Content</param>
        /// <param name="type">Type</param>
        ///<returns>List of events</returns>
        ///<category>Events</category>
        [Update("event/{feedid}")]
        public EventWrapperFull UpdateEvent(int feedid, string content, string title, FeedType type)
        {
            var feed = FeedStorage.GetFeed(feedid).NotFoundIfNull();

            CommunitySecurity.DemandPermissions(feed, NewsConst.Action_Edit);

            feed.Caption = title;
            feed.Text = content;
            feed.Creator = SecurityContext.CurrentAccount.ID.ToString();

            FeedStorage.SaveFeed(feed, true, type);
            
            return new EventWrapperFull(feed);
        }
        ///<summary>
        ///Returns the list of all events for the current user with the event titles, date of creation and update, event text and author
        ///</summary>
        ///<short>
        ///My events
        ///</short>
        ///<returns>List of events</returns>
        ///<category>Events</category>
        [Read("event/@self")]
        public IEnumerable<EventWrapper> GetMyEvents()
        {
            var feeds = FeedStorage.GetFeeds(FeedType.All, SecurityContext.CurrentAccount.ID, (int)_context.Count, (int)_context.StartIndex);
            _context.SetDataPaginated();
            return feeds.Select(x => new EventWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of events matching the search query with the event title, date of creation and update, event type and author
        ///</summary>
        ///<short>
        ///Search
        ///</short>
        /// <param name="query">search query</param>
        ///<returns>List of events</returns>
        ///<category>Events</category>
        [Read("event/@search/{query}")]
        public IEnumerable<EventWrapper> SearchEvents(string query)
        {
            var feeds = FeedStorage.SearchFeeds(query, FeedType.All, Guid.Empty, (int)_context.Count, (int)_context.StartIndex);
            _context.SetDataPaginated();
            return feeds.Select(x => new EventWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the detailed information about the event with the specified ID
        ///</summary>
        ///<short>
        ///Specific event
        ///</short>
        ///<param name="feedid">Event ID</param>
        ///<returns>Event</returns>
        ///<category>Events</category>
        [Read("event/{feedid}")]
        public EventWrapperFull GetEvent(int feedid)
        {
            var feed = FeedStorage.GetFeed(feedid).NotFoundIfNull();
            return new EventWrapperFull(feed);
        }

        ///<summary>
        ///Returns the detailed information about the comments on the event with the specified ID
        ///</summary>
        ///<short>
        ///Get comments
        ///</short>
        ///<param name="feedid">Event id</param>
        ///<returns>List of comments</returns>
        ///<category>Events</category>
        [Read("event/{feedid}/comment")]
        public IEnumerable<EventCommentWrapper> GetEventComments(int feedid)
        {
            FeedStorage.GetFeed(feedid).NotFoundIfNull();
            var feedComments = FeedStorage.GetFeedComments(feedid);
            return feedComments.Where(x => !x.Inactive).Select(x => new EventCommentWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Adds a comment to the event with the specified ID. The parent event ID can be also specified if needed.
        ///</summary>
        ///<short>
        ///Add comment
        ///</short>
        ///<param name="feedid">Event ID</param>
        ///<param name="content">Comment content</param>
        ///<param name="parentId">Comment parent ID</param>
        ///<returns>Comments list</returns>
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
        /// Send parentId=0 or don't send it at all if you want your comment to be on the root level
        /// </remarks>
        /// <category>Events</category>
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
        /// Sends a vote to a certain option in a poll-type event with the ID specified
        ///</summary>
        ///<short>
        /// Vote for event
        ///</short>
        ///<param name="feedid">Event ID</param>
        ///<param name="variants">Variants</param>
        ///<returns>Event</returns>
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
        /// If event is not a poll, then you'll get an error
        /// </remarks>
        /// <category>Events</category>
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
    }
}
