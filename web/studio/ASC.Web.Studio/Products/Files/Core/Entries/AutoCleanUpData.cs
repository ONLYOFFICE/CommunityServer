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


using System;
using System.Runtime.Serialization;

using ASC.Core.Tenants;

namespace ASC.Files.Core
{
    public enum DateToAutoCleanUp
    {
        OneWeek = 1,
        TwoWeeks,
        OneMonth,
        TwoMonths,
        ThreeMonths
    }

    [Serializable]
    [DataContract]
    public class AutoCleanUpData
    {
        [DataMember(Name = "IsAutoCleanUp")]
        public bool IsAutoCleanUp { get; set; }

        [DataMember(Name = "Gap")]
        public DateToAutoCleanUp Gap { get; set; }
    }

    public static class FileDateTime
    {
        public static DateTime GetModifiedOnWithAutoCleanUp(DateTime modifiedOn, DateToAutoCleanUp date, bool utc = false)
        {
            var dateTime = modifiedOn;
            switch (date)
            {
                case DateToAutoCleanUp.OneWeek: dateTime = dateTime.AddDays(7); break;
                case DateToAutoCleanUp.TwoWeeks: dateTime = dateTime.AddDays(14); break;
                case DateToAutoCleanUp.OneMonth: dateTime = dateTime.AddMonths(1); break;
                case DateToAutoCleanUp.TwoMonths: dateTime = dateTime.AddMonths(2); break;
                case DateToAutoCleanUp.ThreeMonths: dateTime = dateTime.AddMonths(3); break;
                default: break;
            }
            return utc ? TenantUtil.DateTimeToUtc(dateTime) : dateTime;
        }
    }
}