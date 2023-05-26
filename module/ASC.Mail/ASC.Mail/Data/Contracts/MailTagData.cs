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


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "Tag")]
    public class MailTagData
    {
        [CollectionDataContract(Namespace = "", ItemName = "address")]
        public class AddressesList<TItem> : List<TItem>
        {
            public AddressesList()
            {
            }

            public AddressesList(IEnumerable<TItem> items)
                : base(items)
            {
            }
        }

        ///<example type="int" name="id">1234</example>
        [DataMember(IsRequired = true, Name = "id")]
        public int Id { get; set; }

        ///<example name="name">name</example>
        [DataMember(IsRequired = true, Name = "name")]
        public string Name { get; set; }

        ///<example name="style">style</example>
        [DataMember(IsRequired = true, Name = "style")]
        public string Style { get; set; }

        ///<example name="addresses">addresses</example>
        ///<collection>list</collection>
        [DataMember(IsRequired = true, Name = "addresses")]
        public AddressesList<string> Addresses { get; set; }

        ///<example type="int" name="lettersCount">1234</example>
        [DataMember(Name = "lettersCount")]
        public int LettersCount { get; set; }
    }
}
