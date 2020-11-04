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
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Specific;
using ASC.Web.Community.News.Code;

namespace ASC.Api.Events
{
    [DataContract(Name = "event", Namespace = "")]
    public class EventWrapperFull : EventWrapper
    {
        [DataMember(Order = 100)]
        public string Text { get; set; }



        [DataMember(Order = 200, EmitDefaultValue = false)]
        public PollWrapper Poll { get; set; }

        public EventWrapperFull(ASC.Web.Community.News.Code.Feed feed)
            : base(feed)
        {
            if (feed is FeedPoll)
            {
                //Add poll info
                var poll = feed as FeedPoll;
                Poll = new PollWrapper(poll);
            }
            Text = feed.Text;
        }

        private EventWrapperFull()
        {

        }

        public static new EventWrapperFull GetSample()
        {
            return new EventWrapperFull()
            {
                CreatedBy = EmployeeWraper.GetSample(),
                Created = ApiDateTime.GetSample(),
                Id = 10,
                Type = FeedType.News,
                Title = "Sample news",
                Updated = ApiDateTime.GetSample(),
                Text = "Text of feed",
                Poll = PollWrapper.GetSample()
            };
        }
    }
}