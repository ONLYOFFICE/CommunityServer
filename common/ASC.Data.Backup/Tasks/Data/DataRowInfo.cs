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
using System.Text;

namespace ASC.Data.Backup.Tasks.Data
{
    internal class DataRowInfo
    {
        private readonly List<string> _columnNames = new List<string>();
        private readonly List<object> _values = new List<object>(); 

        public string TableName { get; private set; }

        public IReadOnlyCollection<string> ColumnNames
        {
            get { return _columnNames.AsReadOnly(); }
        }

        public object this[int index]
        {
            get { return _values[index]; }
        }

        public object this[string columnName]
        {
            get { return _values[GetIndex(columnName)]; }
        }

        public DataRowInfo(string tableName)
        {
            TableName = tableName;
        }

        public void SetValue(string columnName, object item)
        {
            var index = GetIndex(columnName);
            if (index == -1)
            {
                _columnNames.Add(columnName);
                _values.Add(item);
            }
            else
            {
                _values[index] = item;
            }
        }

        private int GetIndex(string columnName)
        {
            return _columnNames.FindIndex(name => name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            const int maxStrLength = 150;
            
            var sb = new StringBuilder(maxStrLength);
            
            int i = 0;
            while (i < _values.Count && sb.Length <= maxStrLength)
            {
                var strVal = Convert.ToString(_values[i]);
                sb.AppendFormat("\"{0}\", ", strVal);
                i++;
            }
            
            if (sb.Length > maxStrLength + 2)
            {
                sb.Length = maxStrLength - 3;
                sb.Append("...");
            }
            else if (sb.Length > 0)
            {
                sb.Length = sb.Length - 2;
            }

            return sb.ToString();
        }
    }
}
