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
using ASC.Projects.Core.Domain;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "milestone", Namespace = "")]
    public class SimpleMilestoneWrapper
    {
        [DataMember(Order = 60)]
        public int Id { get; set; }

        [DataMember(Order = 61)]
        public string Title { get; set; }

        [DataMember(Order = 62)]
        public ApiDateTime Deadline { get; set; }


        public SimpleMilestoneWrapper()
        {
        }

        public SimpleMilestoneWrapper(Milestone milestone)
        {
            Id = milestone.ID;
            Title = milestone.Title;
            Deadline = (ApiDateTime)milestone.DeadLine;
        }


        public static SimpleMilestoneWrapper GetSample()
        {
            return new SimpleMilestoneWrapper
                {
                    Id = 123,
                    Title = "Milestone",
                    Deadline = (ApiDateTime)DateTime.Now,
                };
        }
    }
}