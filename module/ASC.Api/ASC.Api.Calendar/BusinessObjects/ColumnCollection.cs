/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

                    return (T)((object)TimeZoneInfo.FindSystemTimeZoneById(Convert.ToString(row[this.Ind])));
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
