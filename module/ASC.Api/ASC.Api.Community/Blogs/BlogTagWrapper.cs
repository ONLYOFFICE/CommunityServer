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
using ASC.Blogs.Core.Domain;

namespace ASC.Api.Blogs
{
    [DataContract(Name = "tag", Namespace = "")]
    public class BlogTagWrapper
    {

        public BlogTagWrapper(TagStat tagStat)
        {
            Name = tagStat.Name;
            Count = tagStat.Count;
        }

        private BlogTagWrapper()
        {
        }

        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 10)]
        public int Count { get; set; }

        public static BlogTagWrapper GetSample()
        {
            return new BlogTagWrapper
                       {
                           Count = 10,
                           Name = "Sample tag"
                       };
        }
    }
}