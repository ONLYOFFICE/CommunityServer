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


using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASC.Api.Calendar.Wrappers
{
    [DataContract(Name = "sharingOption", Namespace = "")]
    public class AccessOption
    {
        [DataMember(Name = "id", Order = 10)]
        public string Id { get; set; }

        [DataMember(Name = "name", Order = 20)]
        public string Name { get; set; }

        [DataMember(Name = "defaultAction", Order = 30)]
        public bool Default { get; set; }

        [DataMember(Name = "defaultStyle", Order = 40)]
        public string DefaultStyle { get; set; }

        public static AccessOption ReadOption
        {
            get { return new AccessOption() { Id = "read", Default = true, DefaultStyle = "read", Name= Resources.CalendarApiResource.ReadOption }; }
        }

        public static AccessOption FullAccessOption
        {
            get { return new AccessOption() { Id = "full_access", DefaultStyle = "full", Name = Resources.CalendarApiResource.FullAccessOption }; }
        }

        public static AccessOption OwnerOption
        {
            get { return new AccessOption() { Id = "owner", Name = Resources.CalendarApiResource.OwnerOption }; }
        }


        public static List<AccessOption> CalendarStandartOptions {
            get {
                 return new List<AccessOption>(){ReadOption, FullAccessOption};
            }
        }

        public static object GetSample()
        {
            return new { id = "read", name = "Read only", defaultAction = true };
        }
    }
}
