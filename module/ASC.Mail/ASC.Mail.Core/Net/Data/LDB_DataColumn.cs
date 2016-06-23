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


namespace ASC.Mail.Data.lsDB
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// 
    /// </summary>
    public class LDB_DataColumn
    {
        #region Members

        private string m_ColumnName = "";
        private int m_ColumSize = -1;
        private LDB_DataType m_DataType = LDB_DataType.String;

        #endregion

        #region Properties

        /// <summary>
        /// Gets LDB data type.
        /// </summary>
        public LDB_DataType DataType
        {
            get { return m_DataType; }
        }

        /// <summary>
        /// Gets column name.
        /// </summary>
        public string ColumnName
        {
            get { return m_ColumnName; }
        }

        /// <summary>
        /// Gets column size. Returns -1 if column is with variable length.
        /// </summary>
        public int ColumnSize
        {
            get { return m_ColumSize; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <param name="dataType">Column data type.</param>
        public LDB_DataColumn(string columnName, LDB_DataType dataType) : this(columnName, dataType, -1) {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <param name="dataType">Column data type.</param>  
        /// <param name="columnSize">Specifies column data size. This is available for String datatype only.</param>  
        public LDB_DataColumn(string columnName, LDB_DataType dataType, int columnSize)
        {
            m_ColumnName = columnName;
            m_DataType = dataType;

            // TODO: check that column name won't exceed 50 bytes

            if (dataType == LDB_DataType.Bool)
            {
                m_ColumSize = 1;
            }
            else if (dataType == LDB_DataType.DateTime)
            {
                m_ColumSize = 13;
            }
            else if (dataType == LDB_DataType.Int)
            {
                m_ColumSize = 4;
            }
            else if (dataType == LDB_DataType.Long)
            {
                m_ColumSize = 8;
            }
            else
            {
                m_ColumSize = columnSize;
            }
        }

        /// <summary>
        /// Internal constructor.
        /// </summary>
        internal LDB_DataColumn() {}

        #endregion

        #region Internal methods

        /// <summary>
        /// Gets string from char(0) terminated text.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns></returns>
        internal static string GetChar0TerminatedString(string text)
        {
            if (text.IndexOf('\0') > -1)
            {
                return text.Substring(0, text.IndexOf('\0'));
            }
            else
            {
                return text;
            }
        }

        /// <summary>
        /// Parses column from byte[] data.
        /// </summary>
        /// <param name="columnData">Column data.</param>
        internal void Parse(byte[] columnData)
        {
            if (columnData.Length != 102)
            {
                throw new Exception("Invalid column data length '" + columnData.Length + "' !");
            }

            //-- total 102 bytes
            // 1 byte   - column type
            // 4 bytes  - column size
            // 45 bytes - reserved
            // 50 bytes - column name
            // 2  bytes - CRLF

            // column type
            m_DataType = (LDB_DataType) columnData[0];
            // column size
            m_ColumSize = (columnData[1] << 24) | (columnData[2] << 16) | (columnData[3] << 8) |
                          (columnData[4] << 0);
            // reserved
            // 45 bytes
            // column name
            byte[] columnName = new byte[50];
            Array.Copy(columnData, 50, columnName, 0, columnName.Length);
            m_ColumnName = GetChar0TerminatedString(Encoding.UTF8.GetString(columnName));

            if (m_DataType == LDB_DataType.Bool)
            {
                m_ColumSize = 1;
            }
            else if (m_DataType == LDB_DataType.DateTime)
            {
                m_ColumSize = 13;
            }
            else if (m_DataType == LDB_DataType.Int)
            {
                m_ColumSize = 4;
            }
            else if (m_DataType == LDB_DataType.Long)
            {
                m_ColumSize = 8;
            }
        }

        /// <summary>
        /// Convert column to byte[] data.
        /// </summary>
        /// <returns></returns>
        internal byte[] ToColumnInfo()
        {
            //-- total 100 + CRLF bytes
            // 1 byte   - column type
            // 4 bytes  - column size
            // 45 bytes - reserved
            // 50 bytes - column name
            // CRLF

            byte[] columnData = new byte[102];
            // column type
            columnData[0] = (byte) m_DataType;
            // column size
            columnData[1] = (byte) ((m_ColumSize & (1 << 24)) >> 24);
            columnData[2] = (byte) ((m_ColumSize & (1 << 16)) >> 16);
            columnData[3] = (byte) ((m_ColumSize & (1 << 8)) >> 8);
            columnData[4] = (byte) ((m_ColumSize & (1 << 0)) >> 0);
            // reserved
            // 45 bytes
            // column name
            byte[] columnName = Encoding.UTF8.GetBytes(m_ColumnName);
            Array.Copy(columnName, 0, columnData, 50, columnName.Length);
            // CRLF
            columnData[100] = (byte) '\r';
            columnData[101] = (byte) '\n';

            return columnData;
        }

        #endregion
    }
}