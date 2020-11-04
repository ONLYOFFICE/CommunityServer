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
using System.Diagnostics;

namespace ASC.Files.Core
{
    [DataContract(Name = "sorted_by_type", Namespace = "")]
    public enum SortedByType
    {
        [EnumMember] DateAndTime,

        [EnumMember] AZ,

        [EnumMember] Size,

        [EnumMember] Author,

        [EnumMember] Type,

        [EnumMember] New,

        [EnumMember] DateAndTimeCreation

    }

    [DataContract(Name = "orderBy", IsReference = true, Namespace = "")]
    [DebuggerDisplay("{SortedBy} {IsAsc}")]
    public class OrderBy
    {
        [DataMember(Name = "is_asc")]
        public bool IsAsc { get; set; }

        [DataMember(Name = "property")]
        public SortedByType SortedBy { get; set; }

        public OrderBy(SortedByType sortedByType, bool isAsc)
        {
            SortedBy = sortedByType;
            IsAsc = isAsc;
        }
    }
}