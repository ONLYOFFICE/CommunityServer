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
