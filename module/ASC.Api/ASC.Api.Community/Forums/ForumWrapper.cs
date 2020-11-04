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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Forum;

namespace ASC.Api.Forums
{
    [DataContract(Name = "forum", Namespace = "")]
    public class ForumWrapper
    {
        [DataMember(Order = 100)]
        public List<ForumCategoryWrapper> Categories { get; set; }

        public ForumWrapper(IEnumerable<ThreadCategory> categories, IEnumerable<Thread> threads)
        {
            Categories = (from threadCategory in categories where threadCategory.Visible
                         select
                             new ForumCategoryWrapper(threadCategory,
                                                      from thread in threads
                                                      where thread.CategoryID == threadCategory.ID select thread)).ToList();
        }

        private ForumWrapper()
        {

        }

        public static ForumWrapper GetSample()
        {
            return new ForumWrapper
                {
                    Categories = new List<ForumCategoryWrapper> {ForumCategoryWrapper.GetSample()}
                };
        }
    }
}