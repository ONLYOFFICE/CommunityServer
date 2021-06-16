/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

namespace ASC.CRM.Core.Entities
{
    [DataContract]
    public class ListItem : DomainObject
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "color")]
        public string Color { get; set; }

        [DataMember(Name = "sort_order")]
        public int SortOrder { get; set; }

        [DataMember(Name = "additional_params")]
        public string AdditionalParams { get; set; }

        [DataMember(Name = "list_type")]
        public ListType? ListType { get; set; }


        public ListItem()
        {
        }

        public ListItem(string title, string addparams)
        {
            Title = title;
            AdditionalParams = addparams;
        }
    }
}
