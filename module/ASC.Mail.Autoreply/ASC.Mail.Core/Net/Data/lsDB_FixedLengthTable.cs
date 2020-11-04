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
    using System.IO;
    using System.Threading;

    #endregion

    /// <summary>
    /// Table what all columns are with fixed length.
    /// </summary>
    public class lsDB_FixedLengthTable : IDisposable
    {
        #region Members

        private readonly LDB_DataColumnCollection m_pColumns;
        private long m_ColumnsStartOffset = -1;
        private string m_DbFileName = "";
        private long m_FileLength;
        private long m_FilePosition;
        private lsDB_FixedLengthRecord m_pCurrentRecord;
        private FileStream m_pDbFile;
        private byte[] m_RowDataBuffer;
        private int m_RowLength = -1;
        private long m_RowsStartOffset = -1;
        private bool m_TableLocked;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if there is active database file.
        /// </summary>
        public bool IsDatabaseOpen
        {
            get { return m_pDbFile != null; }
        }

        /// <summary>
        /// Gets open database file name. Throws exception if database isn't open.
        /// </summary>
        public string FileName
        {
            get
            {
                if (!IsDatabaseOpen)
                {
                    throw new Exception("Database isn't open, please open database first !");
                }

                return m_DbFileName;
            }
        }

        /// <summary>
        /// Gets table columns. Throws exception if database isn't open.
        /// </summary>
        public LDB_DataColumnCollection Columns
        {
            get
            {
                if (!IsDatabaseOpen)
                {
                    throw new Exception("Database isn't open, please open database first !");
                }

                return m_pColumns;
            }
        }

        /// <summary>
        /// Gets current record. Returns null if there isn't current record.
        /// </summary>
        public lsDB_FixedLengthRecord CurrentRecord
        {
            get { return m_pCurrentRecord; }
        }

        /// <summary>
        /// Gets table is locked.
        /// </summary>
        public bool TableLocked
        {
            get { return m_TableLocked; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public lsDB_FixedLengthTable()
        {
            m_pColumns = new LDB_DataColumnCollection(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Opens specified data file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void Open(string fileName)
        {
            Open(fileName, 0);
        }

        /// <summary>
        /// Opens specified data file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="waitTime">If data base file is exclusively locked, then how many seconds to wait file to unlock before raising a error.</param>
        public void Open(string fileName, int waitTime)
        {
            DateTime lockExpireTime = DateTime.Now.AddSeconds(waitTime);
            while (true)
            {
                try
                {
                    m_pDbFile = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                    break;
                }
                catch (IOException x)
                {
                    if (!File.Exists(fileName))
                    {
                        throw new Exception("Specified database file '" + fileName + "' does not exists !");
                    }

                    // Make this because to get rid of "The variable 'x' is declared but never used"
                    string dummy = x.Message;

                    Thread.Sleep(15);

                    // Lock wait time timed out
                    if (DateTime.Now > lockExpireTime)
                    {
                        throw new Exception("Database file is locked and lock wait time expired !");
                    }
                }
            }

            /* Table structure:
				50    bytes         - version
				2     bytes         - CRLF
                4     bytes         - Free rows count
                2     bytes         - CRLF
				100 x 500 bytes     - 100 columns info store
				2     bytes         - CRLF
				... data pages
			*/

            m_DbFileName = fileName;

            // TODO: check if LDB file

            // Read version line (50 bytes + CRLF)	
            byte[] version = new byte[52];
            ReadFromFile(0, version, 0, version.Length);

            // Read free rows count
            byte[] freeRows = new byte[6];
            ReadFromFile(0, freeRows, 0, freeRows.Length);

            long currentColumnOffset = 58;
            // Read 100 column lines (500 + CRLF bytes each)
            for (int i = 0; i < 100; i++)
            {
                byte[] columnInfo = new byte[102];
                if (ReadFromFile(currentColumnOffset, columnInfo, 0, columnInfo.Length) != columnInfo.Length)
                {
                    throw new Exception("Invalid columns data area length !");
                }

                if (columnInfo[0] != '\0')
                {
                    m_pColumns.Parse(columnInfo);
                }

                currentColumnOffset += 102;
            }

            // Header terminator \0
            m_pDbFile.Position++;

            // No we have rows start offset
            m_RowsStartOffset = m_pDbFile.Position;

            // Store file length and position
            m_FileLength = m_pDbFile.Length;
            m_FilePosition = m_pDbFile.Position;

            // Calculate row length
            m_RowLength = 1 + 2;
            for (int i = 0; i < m_pColumns.Count; i++)
            {
                m_RowLength += m_pColumns[i].ColumnSize;
            }

            m_RowDataBuffer = new byte[m_RowLength];
        }

        /// <summary>
        /// Closes database file.
        /// </summary>
        public void Close()
        {
            if (m_pDbFile != null)
            {
                m_pDbFile.Close();
                m_pDbFile = null;
                m_DbFileName = "";
                m_FileLength = 0;
                m_FilePosition = 0;
                m_RowLength = 0;
                m_ColumnsStartOffset = 0;
                m_RowsStartOffset = 0;
                m_TableLocked = false;
                m_RowDataBuffer = null;
                m_pCurrentRecord = null;
            }
        }

        /// <summary>
        /// Creates new database file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void Create(string fileName)
        {
            m_pDbFile = new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

            /* Table structure:
				50    bytes         - version
				2     bytes         - CRLF
                4     bytes         - Free rows count
                2     bytes         - CRLF
				100 x 500 bytes     - 100 columns info store
				2     bytes         - CRLF
				... data pages
			*/

            // Version 50 + CRLF bytes
            byte[] versionData = new byte[52];
            versionData[0] = (byte) '1';
            versionData[1] = (byte) '.';
            versionData[2] = (byte) '0';
            versionData[50] = (byte) '\r';
            versionData[51] = (byte) '\n';
            m_pDbFile.Write(versionData, 0, versionData.Length);

            // Free rows count
            byte[] freeRows = new byte[6];
            freeRows[4] = (byte) '\r';
            freeRows[5] = (byte) '\n';
            m_pDbFile.Write(freeRows, 0, freeRows.Length);

            m_ColumnsStartOffset = m_pDbFile.Position;

            // 100 x 100 + CRLF bytes header lines
            for (int i = 0; i < 100; i++)
            {
                byte[] data = new byte[100];
                m_pDbFile.Write(data, 0, data.Length);
                m_pDbFile.Write(new byte[] {(int) '\r', (int) '\n'}, 0, 2);
            }

            // Headers terminator char(0)
            m_pDbFile.WriteByte((int) '\0');

            // Rows start pointer
            m_RowsStartOffset = m_pDbFile.Position - 1;

            m_DbFileName = fileName;

            // Store file length and position
            m_FileLength = m_pDbFile.Length;
            m_FilePosition = m_pDbFile.Position;

            // Calculate row length
            m_RowLength = 1 + 2;
            for (int i = 0; i < m_pColumns.Count; i++)
            {
                m_RowLength += m_pColumns[i].ColumnSize;
            }

            m_RowDataBuffer = new byte[m_RowLength];
        }

        /// <summary>
        /// Locks table.
        /// </summary>
        /// <param name="waitTime">If table is locked, then how many sconds to wait table to unlock, before teturning error.</param>
        public void LockTable(int waitTime)
        {
            if (!IsDatabaseOpen)
            {
                throw new Exception("Database isn't open, please open database first !");
            }
            // Table is locked already, just skip locking
            if (m_TableLocked)
            {
                return;
            }

            DateTime lockExpireTime = DateTime.Now.AddSeconds(waitTime);
            while (true)
            {
                try
                {
                    // We just lock first byte
                    m_pDbFile.Lock(0, 1);
                    m_TableLocked = true;

                    break;
                }
                    // Catch the IOException generated if the 
                    // specified part of the file is locked.
                catch (IOException x)
                {
                    // Make this because to get rid of "The variable 'x' is declared but never used"
                    string dummy = x.Message;

                    Thread.Sleep(15);

                    // Lock wait time timed out
                    if (DateTime.Now > lockExpireTime)
                    {
                        throw new Exception("Table is locked and lock wait time expired !");
                    }
                }
            }
        }

        /// <summary>
        /// Unlock table.
        /// </summary>
        public void UnlockTable()
        {
            if (!IsDatabaseOpen)
            {
                throw new Exception("Database isn't open, please open database first !");
            }

            if (m_TableLocked)
            {
                // We just unlock first byte
                m_pDbFile.Unlock(0, 1);
            }
        }

        /// <summary>
        /// Moves to first record.
        /// </summary>
        public void MoveFirstRecord()
        {
            m_pCurrentRecord = null;
            //    NextRecord();
        }

        /// <summary>
        /// Gets next record. Returns true if end of file reached and there are no more records.
        /// </summary>
        /// <returns>Returns true if end of file reached and there are no more records.</returns>
        public bool NextRecord()
        {
            if (!IsDatabaseOpen)
            {
                throw new Exception("Database isn't open, please open database first !");
            }

            //--- Find next record ---------------------------------------------------//
            long nextRowStartOffset = 0;
            if (m_pCurrentRecord == null)
            {
                nextRowStartOffset = m_RowsStartOffset;
            }
            else
            {
                nextRowStartOffset = m_pCurrentRecord.Pointer + m_RowLength;
            }

            while (true)
            {
                if (m_FileLength > nextRowStartOffset)
                {
                    ReadFromFile(nextRowStartOffset, m_RowDataBuffer, 0, m_RowLength);

                    // We want used row
                    if (m_RowDataBuffer[0] == 'u')
                    {
                        if (m_pCurrentRecord == null)
                        {
                            m_pCurrentRecord = new lsDB_FixedLengthRecord(this,
                                                                          nextRowStartOffset,
                                                                          m_RowDataBuffer);
                        }
                        else
                        {
                            m_pCurrentRecord.ReuseRecord(this, nextRowStartOffset, m_RowDataBuffer);
                        }
                        break;
                    }
                }
                else
                {
                    return true;
                }

                nextRowStartOffset += m_RowLength;
            }
            //-------------------------------------------------------------------------//

            return false;
        }

        /// <summary>
        /// Appends new record to table.
        /// </summary>
        public void AppendRecord(object[] values)
        {
            if (!IsDatabaseOpen)
            {
                throw new Exception("Database isn't open, please open database first !");
            }
            if (m_pColumns.Count != values.Length)
            {
                throw new Exception("Each column must have corresponding value !");
            }

            bool unlock = true;
            // Table is already locked, don't lock it
            if (TableLocked)
            {
                unlock = false;
            }
            else
            {
                LockTable(15);
            }

            /* Fixed record structure:
                1 byte     - specified is row is used or free space
                             u - used
                             f - free space
                x bytes    - columns data
                2 bytes    - CRLF
            */

            int rowLength = 1 + 2;
            for (int i = 0; i < m_pColumns.Count; i++)
            {
                rowLength += m_pColumns[i].ColumnSize;
            }

            int position = 1;
            byte[] record = new byte[rowLength];
            record[0] = (int) 'u';
            record[rowLength - 2] = (int) '\r';
            record[rowLength - 1] = (int) '\n';
            for (int i = 0; i < values.Length; i++)
            {
                byte[] columnData = LDB_Record.ConvertToInternalData(m_pColumns[i], values[i]);
                // Check that column won't exceed maximum length.
                if (columnData.Length > m_pColumns[i].ColumnSize)
                {
                    throw new Exception("Column '" + m_pColumns[i] + "' exceeds maximum value length !");
                }

                Array.Copy(columnData, 0, record, position, columnData.Length);
                position += columnData.Length;
            }

            // Find free row
            byte[] freeRowsBuffer = new byte[4];
            ReadFromFile(52, freeRowsBuffer, 0, freeRowsBuffer.Length);
            int freeRows = ldb_Utils.ByteToInt(freeRowsBuffer, 0);
            // There are plenty free rows, find first

            if (freeRows > 100)
            {
                //--- Find free record ---------------------------------------------------//
                long nextRowStartOffset = m_RowsStartOffset;
                long rowOffset = 0;

                byte[] rowData = new byte[m_RowLength];
                while (true)
                {
                    ReadFromFile(nextRowStartOffset, rowData, 0, m_RowLength);

                    // We want used row
                    if (rowData[0] == 'f')
                    {
                        rowOffset = nextRowStartOffset;
                        break;
                    }

                    nextRowStartOffset += m_RowLength;
                }
                //-------------------------------------------------------------------------//

                // Write new record to file
                WriteToFile(rowOffset, record, 0, record.Length);

                // Update free rows count
                WriteToFile(52, ldb_Utils.IntToByte(freeRows - 1), 0, 4);
            }
                // There are few empty rows, just append it
            else
            {
                AppendToFile(record, 0, record.Length);
            }

            if (unlock)
            {
                UnlockTable();
            }
        }

        /// <summary>
        /// Deletes current record.
        /// </summary>
        public void DeleteCurrentRecord()
        {
            if (!IsDatabaseOpen)
            {
                throw new Exception("Database isn't open, please open database first !");
            }
            if (m_pCurrentRecord == null)
            {
                throw new Exception("There is no current record !");
            }

            bool unlock = true;
            // Table is already locked, don't lock it
            if (TableLocked)
            {
                unlock = false;
            }
            else
            {
                LockTable(15);
            }

            byte[] data = new byte[m_RowLength];
            data[0] = (byte) 'f';
            data[m_RowLength - 2] = (byte) '\r';
            data[m_RowLength - 1] = (byte) '\n';
            WriteToFile(m_pCurrentRecord.Pointer, data, 0, data.Length);

            // Update free rows count
            byte[] freeRowsBuffer = new byte[4];
            ReadFromFile(52, freeRowsBuffer, 0, freeRowsBuffer.Length);
            int freeRows = ldb_Utils.ByteToInt(freeRowsBuffer, 0);
            WriteToFile(52, ldb_Utils.IntToByte(freeRows + 1), 0, 4);

            if (unlock)
            {
                UnlockTable();
            }

            // Activate next record **** Change it ???
            NextRecord();
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Sets file position.
        /// </summary>
        /// <param name="position">Position in file.</param>
        private void SetFilePosition(long position)
        {
            if (m_FilePosition != position)
            {
                m_pDbFile.Position = position;
                m_FilePosition = position;
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Adds column to db file.
        /// </summary>
        /// <param name="column"></param>
        internal void AddColumn(LDB_DataColumn column)
        {
            if (column.ColumnSize < 1)
            {
                throw new Exception("Invalid column size '" + column.ColumnSize + "' for column '" +
                                    column.ColumnName + "' !");
            }

            // Find free column data area

            long currentColumnOffset = m_ColumnsStartOffset;
            long freeColumnPosition = -1;
            // Loop all columns data areas, see it there any free left
            for (int i = 0; i < 100; i++)
            {
                byte[] columnInfo = new byte[102];
                if (ReadFromFile(currentColumnOffset, columnInfo, 0, columnInfo.Length) != columnInfo.Length)
                {
                    throw new Exception("Invalid columns data area length !");
                }

                // We found unused column data area
                if (columnInfo[0] == '\0')
                {
                    freeColumnPosition = currentColumnOffset - 102;
                    break;
                }

                currentColumnOffset += 102;
            }

            if (freeColumnPosition != -1)
            {
                // TODO: If there is data ???

                // Store column
                byte[] columnData = column.ToColumnInfo();
                WriteToFile(currentColumnOffset, columnData, 0, columnData.Length);
            }
            else
            {
                throw new Exception("Couldn't find free column space ! ");
            }
        }

        /// <summary>
        /// Removes specified column from database file.
        /// </summary>
        /// <param name="column"></param>
        internal void RemoveColumn(LDB_DataColumn column)
        {
            throw new Exception("TODO:");
        }

        /// <summary>
        /// Reads data from file.
        /// </summary>
        /// <param name="readOffset">Offset in database file from where to start reading data.</param>
        /// <param name="data">Buffer where to store readed data.</param>
        /// <param name="offset">Offset in array to where to start storing readed data.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns></returns>
        internal int ReadFromFile(long readOffset, byte[] data, int offset, int count)
        {
            SetFilePosition(readOffset);

            int readed = m_pDbFile.Read(data, offset, count);
            m_FilePosition += readed;

            return readed;
        }

        /// <summary>
        /// Writes data to file.
        /// </summary>
        /// <param name="writeOffset">Offset in database file from where to start writing data.</param>
        /// <param name="data">Data to write.</param>
        /// <param name="offset">Offset in array from where to start writing data.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <returns></returns>
        internal void WriteToFile(long writeOffset, byte[] data, int offset, int count)
        {
            SetFilePosition(writeOffset);

            m_pDbFile.Write(data, offset, count);
            m_FilePosition += count;
        }

        /// <summary>
        /// Appends specified data at the end of file.
        /// </summary>
        /// <param name="data">Data to write.</param>
        /// <param name="offset">Offset in array from where to start writing data.</param>
        /// <param name="count">Number of bytes to write.</param>
        internal void AppendToFile(byte[] data, int offset, int count)
        {
            m_pDbFile.Position = m_pDbFile.Length;

            m_pDbFile.Write(data, offset, count);

            m_FileLength = m_pDbFile.Length;
            m_FilePosition = m_pDbFile.Position;
        }

        /// <summary>
        /// Gets current position in file.
        /// </summary>
        /// <returns></returns>
        internal long GetFilePosition()
        {
            return m_FilePosition;
        }

        #endregion
    }
}