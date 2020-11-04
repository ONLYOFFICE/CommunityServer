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
using ASC.Web.Community.News.Resources;

namespace ASC.Web.Community.News.Code
{
	class FeedTypeInfo
	{
		public int Id
		{
			get;
			private set;
		}

		public string TypeName
		{
			get;
			private set;
		}

		public string TypeLogoPath
		{
			get;
			private set;
		}

		public FeedTypeInfo(FeedType feedType, string name, string logo)
		{
			Id = (int)feedType;
			TypeName = name;
			TypeLogoPath = logo;
		}

		public static FeedTypeInfo FromFeedType(FeedType feedType)
		{
			switch (feedType)
			{
				case FeedType.News: return new FeedTypeInfo(feedType, NewsResource.FeedTypeNews, "32x_news.png");
				case FeedType.Order: return new FeedTypeInfo(feedType, NewsResource.NewsOrdersTypeName, "32x_order.png");
                case FeedType.Advert: return new FeedTypeInfo(feedType, NewsResource.NewsAnnouncementsTypeName, "32x_advert.png");
                case FeedType.Poll: return new FeedTypeInfo(feedType, NewsResource.FeedTypePoll, "32x_poll.png");
				default: throw new ArgumentOutOfRangeException(string.Format("Unknown feed type: {0}.", feedType));
			}
		}
	}
}
