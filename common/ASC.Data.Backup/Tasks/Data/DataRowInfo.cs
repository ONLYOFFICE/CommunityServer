/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
