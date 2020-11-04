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
using ASC.Web.Core.Calendars;

namespace ASC.Api.Calendar.Wrappers
{
   
    [DataContract(Name = "alert", Namespace = "")]
    public class EventAlertWrapper
    {
        [DataMember(Name = "type")]
        public int Type{ get; set; }

        public static EventAlertWrapper ConvertToTypeSurrogated(EventAlertType type)
        {
            return new EventAlertWrapper() { Type = (int)type };
        }

        public static object GetSample()
        {
            return new { type = -1 };
        }
    }
}
