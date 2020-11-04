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


namespace ASC.Mail.Data.lsDB
{
    #region usings

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// lsDB data column collection.
    /// </summary>
    public class LDB_DataColumnCollection //: IEnumerable<LDB_DataColumn>
    {
        #region Members

        private readonly List<LDB_DataColumn> m_pColumns;
        private readonly object m_pOwner;

        #endregion

        #region Properties

        /// <summary>
        /// Gets column from specified index.
        /// </summary>
        public LDB_DataColumn this[int index]
        {
            get { return m_pColumns[index]; }
        }

        /// <summary>
        /// Gets column count in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pColumns.Count; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="owner">Table that owns this collection.</param>
        internal LDB_DataColumnCollection(object owner)
        {
            m_pOwner = owner;

            m_pColumns = new List<LDB_DataColumn>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Ads specified data column to collection.
        /// </summary>
        /// <param name="column"></param>
        public void Add(LDB_DataColumn column)
        {
            if (Contains(column.ColumnName))
            {
                throw new Exception("Data column with specified name '" + column.ColumnName +
                                    "' already exists !");
            }

            if (m_pOwner.GetType() == typeof (DbFile))
            {
                ((DbFile) m_pOwner).AddColumn(column);
            }
            else if (m_pOwner.GetType() == typeof (lsDB_FixedLengthTable))
            {
                ((lsDB_FixedLengthTable) m_pOwner).AddColumn(column);
            }

            m_pColumns.Add(column);
        }

        /// <summary>
        /// Removes specified data column from collection.
        /// </summary>
        /// <param name="columName">Column name which to remove.</param>
        public void Remove(string columName)
        {
            foreach (LDB_DataColumn column in m_pColumns)
            {
                if (column.ColumnName.ToLower() == columName.ToLower())
                {
                    Remove(column);
                    break;
                }
            }
        }

        /// <summary>
        /// Removes specified data column from collection.
        /// </summary>
        /// <param name="column">Data column which to remove.</param>
        public void Remove(LDB_DataColumn column)
        {
            m_pColumns.Remove(column);
        }

        /// <summary>
        ///  Gets specified data column index in collection. Returns -1 if no such column.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public int IndexOf(string columnName)
        {
            for (int i = 0; i < m_pColumns.Count; i++)
            {
                if ((m_pColumns[i]).ColumnName.ToLower() == columnName.ToLower())
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets specified data column index in collection. Returns -1 if no such column.
        /// </summary>
        /// <param name="column">Data column.</param>
        /// <returns></returns>
        public int IndexOf(LDB_DataColumn column)
        {
            return m_pColumns.IndexOf(column);
        }

        /// <summary>
        /// Gets if data column collection contains specified column.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <returns></returns>
        public bool Contains(string columnName)
        {
            foreach (LDB_DataColumn column in m_pColumns)
            {
                if (column.ColumnName.ToLower() == columnName.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets if data column collection contains specified column.
        /// </summary>
        /// <param name="column">Data column.</param>
        /// <returns></returns>
        public bool Contains(LDB_DataColumn column)
        {
            return m_pColumns.Contains(column);
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<LDB_DataColumn> GetEnumerator()
        {
            return m_pColumns.GetEnumerator();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parses and adds data column to the collection.
        /// </summary>
        /// <param name="columnData"></param>
        internal void Parse(byte[] columnData)
        {
            LDB_DataColumn column = new LDB_DataColumn();
            column.Parse(columnData);

            m_pColumns.Add(column);
        }

        #endregion
    }
}