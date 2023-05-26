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


using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Core;
using ASC.Specific;
using ASC.Web.Community.News.Code;

namespace ASC.Api.Events
{
    [DataContract(Name = "poll", Namespace = "")]
    public class PollWrapper
    {
        public PollWrapper(FeedPoll poll)
        {
            Voted = poll.IsUserVote(SecurityContext.CurrentAccount.ID.ToString());
            if (Voted)
            {
                //Get results
            }
            StartDate = (ApiDateTime)poll.StartDate;
            EndDate = (ApiDateTime)poll.EndDate;
            PollType = poll.PollType;
            Votes = poll.Variants.Select(x => new VoteWrapper() { Id = x.ID, Name = x.Name, Votes = poll.GetVariantVoteCount(x.ID) });
        }

        internal PollWrapper()
        {

        }

        ///<example type="int">0</example>
        ///<order>200</order>
        [DataMember(Order = 200, EmitDefaultValue = true)]
        public FeedPollType PollType { get; set; }

        ///<example>2020-12-07T13:56:02.2859248Z</example>
        ///<order>200</order>
        [DataMember(Order = 200, EmitDefaultValue = false)]
        public ApiDateTime EndDate { get; set; }

        ///<example>2020-12-07T13:56:02.2859248Z</example>
        ///<order>200</order>
        [DataMember(Order = 200, EmitDefaultValue = false)]
        public ApiDateTime StartDate { get; set; }

        ///<example>false</example>
        ///<order></order>
        [DataMember(Order = 200, EmitDefaultValue = true)]
        public bool Voted { get; set; }

        ///<type>ASC.Api.Events.VoteWrapper, ASC.Api.Community</type>
        ///<order>300</order>
        ///<collection>list</collection>
        [DataMember(Order = 300)]
        public IEnumerable<VoteWrapper> Votes { get; set; }

        public static PollWrapper GetSample()
        {
            return new PollWrapper
            {
                EndDate = ApiDateTime.GetSample(),
                PollType = FeedPollType.SimpleAnswer,
                StartDate = ApiDateTime.GetSample(),
                Voted = false,
                Votes = new List<VoteWrapper> { VoteWrapper.GetSample() }
            };
        }
    }
}