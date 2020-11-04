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
using System.Runtime.Serialization;

namespace ASC.Feed.Aggregator
{
    [DataContract]
    public class Feed
    {
        public Feed()
        {
        }

        public Feed(Guid author, DateTime date, bool variate = false)
        {
            AuthorId = author;
            ModifiedBy = author;
            
            CreatedDate = date;
            ModifiedDate = date;

            Action = FeedAction.Created;
            Variate = variate;
        }

        [DataMember]
        public string Item { get; set; }

        [DataMember]
        public string ItemId { get; set; }

        [DataMember]
        public string Id
        {
            get { return string.Format("{0}_{1}", Item, ItemId); }
        }

        [DataMember]
        public Guid AuthorId { get; private set; }

        [DataMember]
        public Guid ModifiedBy { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; private set; }

        [DataMember]
        public DateTime ModifiedDate { get; set; }

        [DataMember]
        public string Product { get; set; }

        [DataMember]
        public string Module { get; set; }

        [DataMember]
        public string ExtraLocation { get; set; }

        [DataMember]
        public string ExtraLocationUrl { get; set; }

        [DataMember]
        public FeedAction Action { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string ItemUrl { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string AdditionalInfo { get; set; }

        [DataMember]
        public string AdditionalInfo2 { get; set; }

        [DataMember]
        public string AdditionalInfo3 { get; set; }

        [DataMember]
        public string AdditionalInfo4 { get; set; }

        [DataMember]
        public bool HasPreview { get; set; }

        [DataMember]
        public bool CanComment { get; set; }

        [DataMember]
        public object Target { get; set; }

        [DataMember]
        public string CommentApiUrl { get; set; }

        [DataMember]
        public IEnumerable<FeedComment> Comments { get; set; }


        // это значит, что новость может обновляться (пр. добавление комментариев);
        // следовательно и права доступа могут устаревать
        public bool Variate { get; private set; }

        public string GroupId { get; set; }

        public string Keywords { get; set; }
    }
}