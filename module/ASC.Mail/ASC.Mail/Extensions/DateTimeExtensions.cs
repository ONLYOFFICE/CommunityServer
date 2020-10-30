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
using ASC.Core.Tenants;

namespace ASC.Mail.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToVerbString(this DateTime dateTime)
        {
            try
            {
                TimeSpan diff = (TenantUtil.DateTimeNow().Date - dateTime.Date);

                if (diff.Days == 0)
                {
                    return dateTime.ToShortTimeString();
                } 
                if (TenantUtil.DateTimeNow().Year == dateTime.Date.Year)
                {
                    return String.Format("{0}", dateTime.ToString("MMMM dd"));
                }
                return String.Format("{0} {1} {2}", dateTime.ToString("dd"), dateTime.ToString("MMMM"), dateTime.ToString("yyyy"));
            }
            catch (Exception)
            {
                return String.Format("{0} {1}", dateTime.ToShortDateString(), dateTime.ToShortTimeString());
            }
        }
    }
}