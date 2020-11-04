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

namespace ASC.Files.Core.Security
{
    [DataContract(Name = "fileShare", Namespace = "")]
    public enum FileShare
    {
        [EnumMember(Value = "0")]
        None,

        [EnumMember(Value = "1")]
        ReadWrite,

        [EnumMember(Value = "2")]
        Read,

        [EnumMember(Value = "3")]
        Restrict,

        [EnumMember(Value = "4")]
        Varies,

        [EnumMember(Value = "5")]
        Review,

        [EnumMember(Value = "6")]
        Comment,

        [EnumMember(Value = "7")]
        FillForms,

        [EnumMember(Value = "8")]
        CustomFilter,
    }
}