/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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