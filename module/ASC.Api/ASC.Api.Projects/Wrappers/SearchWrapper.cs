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


using System.Runtime.Serialization;
using ASC.Projects.Engine;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "search", Namespace = "")]
    public class SearchWrapper
    {
        [DataMember(Order = 10)]
        public SearchItemWrapper Item { get; set; }

        [DataMember(Order = 14)]
        public SearchItemWrapper Owner { get; set; }


        private SearchWrapper()
        {
        }

        public SearchWrapper(SearchItem searchItem)
        {
            Item = new SearchItemWrapper(searchItem);
            if (searchItem.Container != null)
            {
                Owner = new SearchItemWrapper(searchItem.Container);   
            }
        }


        public static SearchWrapper GetSample()
        {
            return new SearchWrapper
                {
                    Item = SearchItemWrapper.GetSample()
                };
        }
    }
}