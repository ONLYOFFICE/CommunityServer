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
using ASC.Core.Users;
using System.Collections.Generic;

namespace ASC.Feed
{
    public class FeedMinUser
    {
        public UserInfo UserInfo { get; set; }
    }

    public class FeedMin
    {
        public string Id { get; set; }

        public Guid AuthorId { get; set; }

        public FeedMinUser Author { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string Product { get; set; }

        public string Item { get; set; }

        public string Title { get; set; }

        public string ItemUrl { get; set; }

        public string Description { get; set; }

        public string AdditionalInfo { get; set; }

        public string AdditionalInfo2 { get; set; }

        public string Module { get; set; }

        public string ExtraLocation { get; set; }

        public IEnumerable<FeedComment> Comments { get; set; }

        public class FeedComment
        {
            public Guid AuthorId { get; set; }

            public FeedMinUser Author { get; set; }

            public string Description { get; set; }

            public DateTime Date { get; set; }


            public FeedMin ToFeedMin()
            {
                return new FeedMin { Author = Author, Title = Description, CreatedDate = Date, ModifiedDate = Date };
            }
        }
    }
}