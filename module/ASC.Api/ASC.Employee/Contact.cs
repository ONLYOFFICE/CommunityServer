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

namespace ASC.Api.Employee
{
    [DataContract(Name = "contact", Namespace = "")]
    public class Contact
    {
        [DataMember(Order = 1)]
        public string Type { get; set; }

        [DataMember(Order = 2)]
        public string Value { get; set; }

        public Contact()
        {
            //For binder
        }

        public Contact(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public static Contact GetSample()
        {
            return new Contact("GTalk", "my@gmail.com");
        }
    }
}