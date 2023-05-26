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
using System.Runtime.Serialization;

namespace ASC.Api.Calendar.Wrappers
{
    [DataContract(Name = "timeZone", Namespace = "")]
    public class TimeZoneWrapper
    {
        private readonly TimeZoneInfo _timeZone;
        public TimeZoneWrapper(TimeZoneInfo timeZone)
        {
            _timeZone = timeZone;
        }

        ///<example name="name">UTC</example>
        ///<order>0</order>
        [DataMember(Name = "name", Order = 0)]
        public string Name
        {
            get
            {
                return Common.Utils.TimeZoneConverter.GetTimeZoneName(_timeZone);
            }
            set { }
        }

        ///<example name="id">UTC</example>
        ///<order>0</order>
        [DataMember(Name = "id", Order = 0)]
        public string Id
        {
            get
            {
                return _timeZone.Id;
            }
            set { }
        }

        ///<example type="int" name="offset">0</example>
        ///<order>0</order>
        [DataMember(Name = "offset", Order = 0)]
        public int Offset
        {
            get
            {
                return (int)_timeZone.GetOffset().TotalMinutes;
            }
            set { }
        }

        public static object GetSample()
        {
            return new { offset = 0, id = "UTC", name = "UTC" };
        }


    }

}
