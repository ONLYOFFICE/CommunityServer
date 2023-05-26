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

using ASC.Api.Calendar.BusinessObjects;
using ASC.Files.Core.Security;

namespace ASC.Api.Calendar.Attachments
{
    public class SecurityAdapterProvider : IFileSecurityProvider
    {
        public IFileSecurity GetFileSecurity(string data)
        {
            return new SecurityAdapter(data);
        }

        public Dictionary<object, IFileSecurity> GetFileSecurity(Dictionary<string, string> data)
        {
            var result = new Dictionary<object, IFileSecurity>();
            using (var provider = new DataProvider())
            {
                foreach (var item in data)
                {
                    if (int.TryParse(item.Value, out int id))
                    {
                        var eventObj = provider.GetEventById(id);
                        if(eventObj != null)
                        {
                            var calendarObj = provider.GetCalendarById(Convert.ToInt32(eventObj.CalendarId));
                            result.Add(item.Key, new SecurityAdapter(eventObj, calendarObj));
                            continue;
                        }
                    }
                    result.Add(item.Key, new SecurityAdapter());
                }
            }
            return result;
        }
    }
}
