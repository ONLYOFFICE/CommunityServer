/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
