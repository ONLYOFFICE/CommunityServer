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
    using System.Collections;
    using System.IO;
    using System.Text;

    #endregion

    /// <summary>
    /// lsDB database record.
    /// </summary>
    public class LDB_Record
    {
        #region Members

        private readonly DataPage m_pDataPage;
        private readonly DbFile m_pOwnerDb;
        private int[] m_ColumnValueSize;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or set all data column values.
        /// </summary>
        public object[] Values
        {
            get
            {
                object[] retVal = new object[m_pOwnerDb.Columns.Count];
                for (int i = 0; i < m_pOwnerDb.Columns.Count; i++)
                {
                    retVal[i] = this[i];
                }

                return retVal;
            }

            set { UpdateRecord(value); }
        }

        /// <summary>
        /// Gets or sets specified data column value.
        /// </summary>
        public object this[int columnIndex]
        {
            get
            {
                if (columnIndex < 0)
                {
                    throw new Exception("The columnIndex can't be negative value !");
                }
                if (columnIndex > m_pOwnerDb.Columns.Count)
                {
                    throw new Exception("The columnIndex out of columns count !");
                }

                return GetColumnData(columnIndex);
            }

            set
            {
                if (columnIndex < 0)
                {
                    throw new Exception("The columnIndex can't be negative value !");
                }
                if (columnIndex > m_pOwnerDb.Columns.Count)
                {
                    throw new Exception("The columnIndex out of columns count !");
                }

                // Get current row values
                object[] rowValues = Values;
                // Update specified column value
                rowValues[columnIndex] = value;
                // Update record
                UpdateRecord(rowValues);
            }
        }

        /// <summary>
        /// Gets or sets specified data column value.
        /// </summary>
        public object this[string columnName]
        {
            get
            {
                int index = m_pOwnerDb.Columns.IndexOf(columnName);
                if (index == -1)
                {
                    throw new Exception("Table doesn't contain column '" + columnName + "' !");
                }

                return this[index];
            }

            set
            {
                int index = m_pOwnerDb.Columns.IndexOf(columnName);
                if (index == -1)
                {
                    throw new Exception("Table doesn't contain column '" + columnName + "' !");
                }

                this[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets specified data column value.
        /// </summary>
        public object this[LDB_DataColumn column]
        {
            get
            {
                int index = m_pOwnerDb.Columns.IndexOf(column);
                if (index == -1)
                {
                    throw new Exception("Table doesn't contain column '" + column.ColumnName + "' !");
                }

                return this[index];
            }

            set
            {
                int index = m_pOwnerDb.Columns.IndexOf(column);
                if (index == -1)
                {
                    throw new Exception("Table doesn't contain column '" + column.ColumnName + "' !");
                }

                this[index] = value;
            }
        }

        /// <summary>
        /// Gets data page on what row starts.
        /// </summary>
        internal DataPage DataPage
        {
            get { return m_pDataPage; }
        }

        /// <summary>
        /// Gets data pages held by this row.
        /// </summary>
        internal DataPage[] DataPages
        {
            get
            {
                ArrayList dataPages = new ArrayList();
                DataPage page = m_pDataPage;
                dataPages.Add(page);
                while (page.NextDataPagePointer > 0)
                {
                    page = new DataPage(m_pOwnerDb.DataPageDataAreaSize, m_pOwnerDb, page.NextDataPagePointer);
                    dataPages.Add(page);
                }

                DataPage[] retVal = new DataPage[dataPages.Count];
                dataPages.CopyTo(retVal);

                return retVal;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ownerDb">Table that owns this row.</param>
        /// <param name="rowStartDataPage">Data page on what row starts.</param>
        internal LDB_Record(DbFile ownerDb, DataPage rowStartDataPage)
        {
            m_pOwnerDb = ownerDb;
            m_pDataPage = rowStartDataPage;

            ParseRowInfo();
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Parse row info.
        /// </summary>
        private void ParseRowInfo()
        {
            /* RowInfo structure		
				(4 bytes) * columnCount - holds column data data length
				xx bytes columns values
			*/

            m_ColumnValueSize = new int[m_pOwnerDb.Columns.Count];
            byte[] columnValueSizes = m_pDataPage.ReadData(0, 4*m_pOwnerDb.Columns.Count);
            for (int i = 0; i < m_pOwnerDb.Columns.Count; i++)
            {
                m_ColumnValueSize[i] = ldb_Utils.ByteToInt(columnValueSizes, i*4);
            }
        }

        /// <summary>
        /// Gets specified column data.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns></returns>
        private object GetColumnData(int columnIndex)
        {
            // Get column data start offset
            int columnStartOffset = 4*m_pOwnerDb.Columns.Count;
            for (int i = 0; i < columnIndex; i++)
            {
                columnStartOffset += m_ColumnValueSize[i];
            }

            int dataLength = m_ColumnValueSize[columnIndex];
            int startDataPage = (int) Math.Floor(columnStartOffset/(double) m_pOwnerDb.DataPageDataAreaSize);
            int offsetInStartDataPage = columnStartOffset - (startDataPage*m_pOwnerDb.DataPageDataAreaSize);

            int dataOffset = 0;
            int currentDataPageIndex = 0;
            byte[] columnData = new byte[dataLength];
            DataPage currentDataPage = DataPage;
            while (dataOffset < dataLength)
            {
                // We haven't reached to data page on what data starts, just go to next continuing data page
                if (currentDataPageIndex < startDataPage)
                {
                    // Get next continuing data page
                    currentDataPage = new DataPage(m_pOwnerDb.DataPageDataAreaSize,
                                                   m_pOwnerDb,
                                                   currentDataPage.NextDataPagePointer);
                    currentDataPageIndex++;
                }
                    // We need all this data page data + addtitional data pages data
                else if ((dataLength - dataOffset + offsetInStartDataPage) > currentDataPage.StoredDataLength)
                {
                    currentDataPage.ReadData(columnData,
                                             dataOffset,
                                             m_pOwnerDb.DataPageDataAreaSize - offsetInStartDataPage,
                                             offsetInStartDataPage);
                    dataOffset += m_pOwnerDb.DataPageDataAreaSize - offsetInStartDataPage;

                    // Get next continuing data page
                    currentDataPage = new DataPage(m_pOwnerDb.DataPageDataAreaSize,
                                                   m_pOwnerDb,
                                                   currentDataPage.NextDataPagePointer);
                    currentDataPageIndex++;

                    offsetInStartDataPage = 0;
                }
                    // This data page has all data we need
                else
                {
                    currentDataPage.ReadData(columnData,
                                             dataOffset,
                                             dataLength - dataOffset,
                                             offsetInStartDataPage);
                    dataOffset += dataLength - dataOffset;

                    offsetInStartDataPage = 0;
                }
            }

            // Convert to column data type
            return ConvertFromInternalData(m_pOwnerDb.Columns[columnIndex], columnData);
        }

        /// <summary>
        /// Updates this record values.
        /// </summary>
        /// <param name="rowValues">Row new values.</param>
        private void UpdateRecord(object[] rowValues)
        {
            bool unlock = true;
            // Table is already locked, don't lock it
            if (m_pOwnerDb.TableLocked)
            {
                unlock = false;
            }
            else
            {
                m_pOwnerDb.LockTable(15);
            }

            // Create new record
            byte[] rowData = CreateRecord(m_pOwnerDb, rowValues);

            DataPage[] dataPages = DataPages;
            // Clear old data ?? do we need that			
            //	for(int i=0;i<dataPages.Length;i++){
            //		dataPages[i].Data = new byte[1000];
            //	}

            // We haven't enough room to store row, get needed data pages
            if ((int) Math.Ceiling(rowData.Length/(double) m_pOwnerDb.DataPageDataAreaSize) > dataPages.Length)
            {
                int dataPagesNeeded =
                    (int) Math.Ceiling(rowData.Length/(double) m_pOwnerDb.DataPageDataAreaSize) -
                    dataPages.Length;
                DataPage[] additionalDataPages =
                    m_pOwnerDb.GetDataPages(dataPages[dataPages.Length - 1].Pointer, dataPagesNeeded);

                // Append new data pages to existing data pages chain
                dataPages[dataPages.Length - 1].NextDataPagePointer = additionalDataPages[0].Pointer;

                DataPage[] newVal = new DataPage[dataPages.Length + additionalDataPages.Length];
                Array.Copy(dataPages, 0, newVal, 0, dataPages.Length);
                Array.Copy(additionalDataPages, 0, newVal, dataPages.Length, additionalDataPages.Length);
                dataPages = newVal;
            }

            // Store new record
            DbFile.StoreDataToDataPages(m_pOwnerDb.DataPageDataAreaSize, rowData, dataPages);

            // Update row info
            ParseRowInfo();

            if (unlock)
            {
                m_pOwnerDb.UnlockTable();
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Creates record. Contains record info + record values.
        /// </summary>
        /// <param name="ownerDb">Roecord owner table.</param>
        /// <param name="rowValues">Row values what to store to record.</param>
        /// <returns></returns>
        internal static byte[] CreateRecord(DbFile ownerDb, object[] rowValues)
        {
            if (ownerDb.Columns.Count != rowValues.Length)
            {
                throw new Exception("LDB_Record.CreateRecord m_pOwnerDb.Columns.Count != rowValues.Length !");
            }

            // Convert values to internal store format
            ArrayList rowByteValues = new ArrayList();
            for (int i = 0; i < rowValues.Length; i++)
            {
                rowByteValues.Add(ConvertToInternalData(ownerDb.Columns[i], rowValues[i]));
            }

            /* RowInfo structure			
				(4 bytes) * columnCount - holds column data data length
				xx bytes columns values
			*/

            MemoryStream msRecord = new MemoryStream();
            // Write values sizes
            for (int i = 0; i < rowByteValues.Count; i++)
            {
                msRecord.Write(ldb_Utils.IntToByte(((byte[]) rowByteValues[i]).Length), 0, 4);
            }

            // Write values
            for (int i = 0; i < rowByteValues.Count; i++)
            {
                byte[] val = (byte[]) rowByteValues[i];
                msRecord.Write(val, 0, val.Length);
            }

            return msRecord.ToArray();
        }

        /// <summary>
        /// Converts data to specied column internal store data.
        /// </summary>
        /// <param name="coulmn">Column where to store data.</param>
        /// <param name="val">Data to convert.</param>
        /// <returns></returns>
        internal static byte[] ConvertToInternalData(LDB_DataColumn coulmn, object val)
        {
            if (val == null)
            {
                throw new Exception("Null values aren't supported !");
            }

            if (coulmn.DataType == LDB_DataType.Bool)
            {
                if (val.GetType() != typeof (bool))
                {
                    throw new Exception("Column '" + coulmn.ColumnName +
                                        "' requires datatype of bool, but value contains '" + val.GetType() +
                                        "' !");
                }

                return new[] {Convert.ToByte((bool) val)};
            }
            else if (coulmn.DataType == LDB_DataType.DateTime)
            {
                if (val.GetType() != typeof (DateTime))
                {
                    throw new Exception("Column '" + coulmn.ColumnName +
                                        "' requires datatype of DateTime, but value contains '" +
                                        val.GetType() + "' !");
                }

                /* Data structure
					1 byte day
					1 byte month
					4 byte year (int)
					1 byte hour
					1 byte minute
					1 byte second
				*/

                DateTime d = (DateTime) val;
                byte[] dateBytes = new byte[13];
                // day
                dateBytes[0] = (byte) d.Day;
                // month
                dateBytes[1] = (byte) d.Month;
                // year
                Array.Copy(ldb_Utils.IntToByte(d.Year), 0, dateBytes, 2, 4);
                // hour
                dateBytes[6] = (byte) d.Hour;
                // minute
                dateBytes[7] = (byte) d.Minute;
                // second
                dateBytes[8] = (byte) d.Second;

                return dateBytes;
            }
            else if (coulmn.DataType == LDB_DataType.Long)
            {
                if (val.GetType() != typeof (long))
                {
                    throw new Exception("Column '" + coulmn.ColumnName +
                                        "' requires datatype of Long, but value contains '" + val.GetType() +
                                        "' !");
                }

                return ldb_Utils.LongToByte((long) val);
            }
            else if (coulmn.DataType == LDB_DataType.Int)
            {
                if (val.GetType() != typeof (int))
                {
                    throw new Exception("Column '" + coulmn.ColumnName +
                                        "' requires datatype of Int, but value contains '" + val.GetType() +
                                        "' !");
                }

                return ldb_Utils.IntToByte((int) val);
            }
            else if (coulmn.DataType == LDB_DataType.String)
            {
                if (val.GetType() != typeof (string))
                {
                    throw new Exception("Column '" + coulmn.ColumnName +
                                        "' requires datatype of String, but value contains '" + val.GetType() +
                                        "' !");
                }

                return Encoding.UTF8.GetBytes(val.ToString());
            }
            else
            {
                throw new Exception("Invalid column data type, never must reach here !");
            }
        }

        /// <summary>
        /// Converts internal data to .NET data type.
        /// </summary>
        /// <param name="coulmn">Column what data it is.</param>
        /// <param name="val">Internal data value.</param>
        /// <returns></returns>
        internal static object ConvertFromInternalData(LDB_DataColumn coulmn, byte[] val)
        {
            if (coulmn.DataType == LDB_DataType.Bool)
            {
                return Convert.ToBoolean(val[0]);
            }
            else if (coulmn.DataType == LDB_DataType.DateTime)
            {
                /* Data structure
					1 byte day
					1 byte month
					4 byte year (int)
					1 byte hour
					1 byte minute
					1 byte second
				*/

                byte[] dateBytes = new byte[13];
                // day
                int day = val[0];
                // month
                int month = val[1];
                // year
                int year = ldb_Utils.ByteToInt(val, 2);
                // hour
                int hour = val[6];
                // minute
                int minute = val[7];
                // second
                int second = val[8];

                return new DateTime(year, month, day, hour, minute, second);
            }
            else if (coulmn.DataType == LDB_DataType.Long)
            {
                return ldb_Utils.ByteToLong(val, 0);
            }
            else if (coulmn.DataType == LDB_DataType.Int)
            {
                return ldb_Utils.ByteToInt(val, 0);
            }
            else if (coulmn.DataType == LDB_DataType.String)
            {
                return Encoding.UTF8.GetString(val);
            }
            else
            {
                throw new Exception("Invalid column data type, never must reach here !");
            }
        }

        #endregion
    }
}