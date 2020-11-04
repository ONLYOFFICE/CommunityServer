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
using System.Text;
using System.Runtime.Serialization;

namespace ASC.Api.Calendar.Wrappers
{
    [DataContract(Name = "permissions", Namespace = "")]
    public class Permissions
    {
        [DataMember(Name = "users")]
        public List<UserParams> UserParams { get; set; }

        public Permissions()
        {
            this.UserParams = new List<UserParams>();
        }

        public static object GetSample()
        {
            return new { users = new List<object>(){ ASC.Api.Calendar.Wrappers.UserParams.GetSample() } };
        }
    }

    [DataContract(Name = "permissions", Namespace = "")]
    public class CalendarPermissions : Permissions
    {
        [DataMember(Name = "data")]
        public PublicItemCollection Data { get; set; }

        public new static object GetSample()
        {
            return new { data = PublicItemCollection.GetSample() };
        }
    }

    [DataContract(Name = "userparams", Namespace = "")]
    public class UserParams
    {
        [DataMember(Name="objectId")]
        public Guid Id{get; set;}

        [DataMember(Name="name")]
        public string Name{get; set;}

        public static object GetSample()
        {
            return new { objectId = "2fdfe577-3c26-4736-9df9-b5a683bb8520", name = "Valery Zykov" };
        }
    }
}
