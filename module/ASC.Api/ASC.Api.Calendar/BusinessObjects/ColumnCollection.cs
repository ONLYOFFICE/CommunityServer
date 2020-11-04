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
using ASC.Common.Utils;
using ASC.Web.Core.Calendars;

namespace ASC.Api.Calendar.BusinessObjects
{
    internal class ColumnCollection
    {   
        private List<SelectColumnInfo> _columns = new List<SelectColumnInfo>();

        public class SelectColumnInfo{

            public string Name{get; set;}
            public int Ind{get; set;}

            public T Parse<T>(object[] row)
            {               
                if (typeof(T).Equals(typeof(int)))                
                    return (T)((object)Convert.ToInt32(row[this.Ind]));
                
                if (typeof(T).Equals(typeof(Guid)))
                    return (T)((object)new Guid(Convert.ToString(row[this.Ind])));
                
                if (typeof(T).Equals(typeof(Boolean)))
                    return (T)((object)(Convert.ToBoolean(row[this.Ind])));

                if (typeof(T).Equals(typeof(DateTime)))                
                    return (T)((object)Convert.ToDateTime(row[this.Ind]));

                if (typeof(T).Equals(typeof(RecurrenceRule)))
                {
                    return (T)((object) RecurrenceRule.Parse(Convert.ToString(row[this.Ind])));
                }

                if (typeof(T).Equals(typeof(TimeZoneInfo)))
                {
                    if (IsNull(row))
                        return (T)(object)null;

                    var timeZoneId = Convert.ToString(row[this.Ind]);

                    return (T)((object)TimeZoneConverter.GetTimeZone(timeZoneId));
                }
                
                return (T)(object)(Convert.ToString(row[this.Ind])??"");
            }

            public bool IsNull(object[] row)
            {
                return row[this.Ind] == null || row[this.Ind] == DBNull.Value;
            }
        }

        public SelectColumnInfo RegistryColumn(string selectName)
        {
            var c = new SelectColumnInfo() { Name = selectName, Ind = _columns.Count};
            _columns.Add(c);
            return c;
        }        

        public string[] SelectQuery
        {
            get
            {
                return _columns.Select(c => c.Name).ToArray();
            }
        }
    }
}
