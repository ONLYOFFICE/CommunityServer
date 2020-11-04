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
using System.Collections.Generic;

namespace ASC.Bookmarking.Pojo
{
	public class Bookmark
	{
		public virtual long ID { get; set; }

		public virtual string URL { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual string Name { get; set; }
		public virtual string Description { get; set; }

		public virtual Guid UserCreatorID { get; set; }

		public IList<UserBookmark> UserBookmarks { get; set; }

		public IList<Comment> Comments { get; set; }

		public IList<Tag> Tags { get; set; }

		public Bookmark(string url, DateTime date, string name, string description)
		{
			this.URL = url;
			this.Date = date;
			this.Name = name;
			this.Description = description;

			InitCollections();
		}

		public Bookmark()
		{
			InitCollections();
		}

		private void InitCollections()
		{
			UserBookmarks = new List<UserBookmark>();
			Comments = new List<Comment>();
			Tags = new List<Tag>();
		}
	}
}
