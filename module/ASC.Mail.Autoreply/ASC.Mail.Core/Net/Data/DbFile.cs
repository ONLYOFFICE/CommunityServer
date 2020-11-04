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
    using System.Threading;
    using Net;

    #endregion

    /// <summary>
    /// LumiSoft database file.
    /// </summary>
    public class DbFile : IDisposable
    {
        #region Members

        private readonly LDB_DataColumnCollection m_pColumns;
        private int m_DataPageDataAreaSize = 1000;
        private long m_DatapagesStartOffset = -1;
        private string m_DbFileName = "";

        private long m_FileLength;
        private long m_FilePosition;
        private LDB_Record m_pCurrentRecord;
        private FileStream m_pDbFile;
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
        public LDB_Record CurrentRecord
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

        /// <summary>
        /// Gets how much data data page can store.
        /// </summary>
        public int DataPageDataAreaSize
        {
            get { return m_DataPageDataAreaSize; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DbFile()
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
				8     bytes         - free datapages count
				2     bytes         - CRLF
				4     bytes         - datapage data area size
				2     bytes         - CRLF
				100 x 500 bytes     - 100 columns info store
				2     bytes         - CRLF
				... data pages
			*/

            m_DbFileName = fileName;
            StreamLineReader r = new StreamLineReader(m_pDbFile);

            // TODO: check if LDB file

            // Read version line (50 bytes + CRLF)	
            byte[] version = r.ReadLine();

            // Skip free data pages count
            byte[] freeDataPagesCount = new byte[10];
            m_pDbFile.Read(freeDataPagesCount, 0, freeDataPagesCount.Length);

            // 4 bytes datapage data area size + CRLF
            byte[] dataPageDataAreaSize = new byte[6];
            m_pDbFile.Read(dataPageDataAreaSize, 0, dataPageDataAreaSize.Length);
            m_DataPageDataAreaSize = ldb_Utils.ByteToInt(dataPageDataAreaSize, 0);

            // Read 100 column lines (500 + CRLF bytes each)
            for (int i = 0; i < 100; i++)
            {
                byte[] columnInfo = r.ReadLine();
                if (columnInfo == null)
                {
                    throw new Exception("Invalid columns data area length !");
                }

                if (columnInfo[0] != '\0')
                {
                    m_pColumns.Parse(columnInfo);
                }
            }

            // Header terminator \0
            m_pDbFile.Position++;

            // No we have rows start offset
            m_DatapagesStartOffset = m_pDbFile.Position;

            // Store file length and position
            m_FileLength = m_pDbFile.Length;
            m_FilePosition = m_pDbFile.Position;
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
            }
        }

        /// <summary>
        /// Creates new database file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void Create(string fileName)
        {
            Create(fileName, 1000);
        }

        /// <summary>
        /// Creates new database file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="dataPageDataAreaSize">Specifies how many data can data page store.</param>
        public void Create(string fileName, int dataPageDataAreaSize)
        {
            m_pDbFile = new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

            /* Table structure:
				50    bytes         - version
				2     bytes         - CRLF
				8     bytes         - free datapages count
				2     bytes         - CRLF
				4     bytes         - datapage data area size
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

            // 8 bytes free data pages count + CRLF
            byte[] freeDataPagesCount = new byte[10];
            freeDataPagesCount[8] = (byte) '\r';
            freeDataPagesCount[9] = (byte) '\n';
            m_pDbFile.Write(freeDataPagesCount, 0, freeDataPagesCount.Length);

            // 4 bytes datapage data area size + CRLF
            byte[] dataPageDataAreaSizeB = new byte[6];
            Array.Copy(ldb_Utils.IntToByte(dataPageDataAreaSize), 0, dataPageDataAreaSizeB, 0, 4);
            dataPageDataAreaSizeB[4] = (byte) '\r';
            dataPageDataAreaSizeB[5] = (byte) '\n';
            m_pDbFile.Write(dataPageDataAreaSizeB, 0, dataPageDataAreaSizeB.Length);

            // 100 x 100 + CRLF bytes header lines
            for (int i = 0; i < 100; i++)
            {
                byte[] data = new byte[100];
                m_pDbFile.Write(data, 0, data.Length);
                m_pDbFile.Write(new byte[] {(int) '\r', (int) '\n'}, 0, 2);
            }

            // Headers terminator char(0)
            m_pDbFile.WriteByte((int) '\0');

            // Data pages start pointer
            m_DatapagesStartOffset = m_pDbFile.Position - 1;

            m_DbFileName = fileName;
            m_DataPageDataAreaSize = dataPageDataAreaSize;

            // Store file length and position
            m_FileLength = m_pDbFile.Length;
            m_FilePosition = m_pDbFile.Position;
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

        /*
		/// <summary>
		/// Locks current record.
		/// </summary>
		public void LockRecord()
		{
			if(!this.IsDatabaseOpen){
				throw new Exception("Database isn't open, please open database first !");
			}

		}
*/

        /*
		/// <summary>
		/// Unlocks current record.
		/// </summary>
		public void UnlockRecord()
		{
			if(!this.IsDatabaseOpen){
				throw new Exception("Database isn't open, please open database first !");
			}

		}
*/

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
                nextRowStartOffset = m_DatapagesStartOffset;
            }
            else
            {
                nextRowStartOffset = m_pCurrentRecord.DataPage.Pointer + m_DataPageDataAreaSize + 33;
            }

            while (true)
            {
                if (m_FileLength > nextRowStartOffset)
                {
                    DataPage dataPage = new DataPage(m_DataPageDataAreaSize, this, nextRowStartOffset);

                    // We want datapage with used space
                    if (dataPage.Used && dataPage.OwnerDataPagePointer < 1)
                    {
                        m_pCurrentRecord = new LDB_Record(this, dataPage);
                        break;
                    }
                }
                else
                {
                    return true;
                }

                nextRowStartOffset += m_DataPageDataAreaSize + 33;
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

            // Construct record data
            byte[] record = LDB_Record.CreateRecord(this, values);

            // Get free data pages
            DataPage[] dataPages = GetDataPages(0,
                                                (int)
                                                Math.Ceiling(record.Length/(double) m_DataPageDataAreaSize));

            StoreDataToDataPages(m_DataPageDataAreaSize, record, dataPages);

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

            // Release all data pages hold by this row
            DataPage[] dataPages = m_pCurrentRecord.DataPages;
            for (int i = 0; i < dataPages.Length; i++)
            {
                DataPage p = dataPages[i];

                byte[] dataPage = DataPage.CreateDataPage(m_DataPageDataAreaSize,
                                                          false,
                                                          0,
                                                          0,
                                                          0,
                                                          new byte[m_DataPageDataAreaSize]);
                SetFilePosition(p.Pointer);
                WriteToFile(dataPage, 0, dataPage.Length);
            }

            // Increase free data pages count info in table header
            byte[] freeDataPagesCount = new byte[8];
            SetFilePosition(52);
            ReadFromFile(freeDataPagesCount, 0, freeDataPagesCount.Length);
            freeDataPagesCount =
                ldb_Utils.LongToByte(ldb_Utils.ByteToLong(freeDataPagesCount, 0) + dataPages.Length);
            SetFilePosition(52);
            WriteToFile(freeDataPagesCount, 0, freeDataPagesCount.Length);

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
        /// Moves position to the end of file.
        /// </summary>
        private void GoToFileEnd()
        {
            m_pDbFile.Position = m_pDbFile.Length;
            m_FileLength = m_pDbFile.Length;
            m_FilePosition = m_FileLength;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Stores data to specified data pages.
        /// </summary>
        /// <param name="dataPageDataAreaSize">Data page data area size.</param>
        /// <param name="data">Data to store.</param>
        /// <param name="dataPages">Data pages where to store data.</param>
        internal static void StoreDataToDataPages(int dataPageDataAreaSize, byte[] data, DataPage[] dataPages)
        {
            if ((int) Math.Ceiling(data.Length/(double) dataPageDataAreaSize) > dataPages.Length)
            {
                throw new Exception("There isn't enough data pages to store data ! Data needs '" +
                                    (int) Math.Ceiling(data.Length/(double) dataPageDataAreaSize) +
                                    "' , but available '" + dataPages.Length + "'.");
            }

            //--- Store data to data page(s) -----------------------//
            long position = 0;
            for (int i = 0; i < dataPages.Length; i++)
            {
                if ((data.Length - position) > dataPageDataAreaSize)
                {
                    byte[] d = new byte[dataPageDataAreaSize];
                    Array.Copy(data, position, d, 0, d.Length);
                    dataPages[i].WriteData(d);
                    position += dataPageDataAreaSize;
                }
                else
                {
                    byte[] d = new byte[data.Length - position];
                    Array.Copy(data, position, d, 0, d.Length);
                    dataPages[i].WriteData(d);
                }
            }
            //------------------------------------------------------//	
        }

        /// <summary>
        /// Gets specified number of free data pages. If free data pages won't exist, creates new ones.
        /// Data pages are marked as used and OwnerDataPagePointer and NextDataPagePointer is set as needed.
        /// </summary>
        /// <param name="ownerDataPagePointer">Owner data page pointer that own first requested data page. If no owner then this value is 0.</param>
        /// <param name="count">Number of data pages wanted.</param>
        internal DataPage[] GetDataPages(long ownerDataPagePointer, int count)
        {
            if (!TableLocked)
            {
                throw new Exception("Table must be locked to acess GetDataPages() method !");
            }

            ArrayList freeDataPages = new ArrayList();

            // Get free data pages count from table header
            byte[] freeDataPagesCount = new byte[8];
            SetFilePosition(52);
            ReadFromFile(freeDataPagesCount, 0, freeDataPagesCount.Length);
            long nFreeDataPages = ldb_Utils.ByteToLong(freeDataPagesCount, 0);

            // We have plenty free data pages and enough for count requested, find requested count free data pages
            if (nFreeDataPages > 1000 && nFreeDataPages > count)
            {
                long nextDataPagePointer = m_DatapagesStartOffset + 1;
                while (freeDataPages.Count < count)
                {
                    DataPage dataPage = new DataPage(m_DataPageDataAreaSize, this, nextDataPagePointer);
                    if (!dataPage.Used)
                    {
                        dataPage.Used = true;
                        freeDataPages.Add(dataPage);
                    }

                    nextDataPagePointer += m_DataPageDataAreaSize + 33;
                }

                // Decrease free data pages count in table header
                SetFilePosition(52);
                ReadFromFile(ldb_Utils.LongToByte(nFreeDataPages - count), 0, 8);
            }
                // Just create new data pages
            else
            {
                for (int i = 0; i < count; i++)
                {
                    byte[] dataPage = DataPage.CreateDataPage(m_DataPageDataAreaSize,
                                                              true,
                                                              0,
                                                              0,
                                                              0,
                                                              new byte[m_DataPageDataAreaSize]);
                    GoToFileEnd();
                    long dataPageStartPointer = GetFilePosition();
                    WriteToFile(dataPage, 0, dataPage.Length);

                    freeDataPages.Add(new DataPage(m_DataPageDataAreaSize, this, dataPageStartPointer));
                }
            }

            // Relate data pages (chain)
            for (int i = 0; i < freeDataPages.Count; i++)
            {
                // First data page
                if (i == 0)
                {
                    // Owner data page poitner specified for first data page
                    if (ownerDataPagePointer > 0)
                    {
                        ((DataPage) freeDataPages[i]).OwnerDataPagePointer = ownerDataPagePointer;
                    }

                    // There is continuing data page
                    if (freeDataPages.Count > 1)
                    {
                        ((DataPage) freeDataPages[i]).NextDataPagePointer =
                            ((DataPage) freeDataPages[i + 1]).Pointer;
                    }
                }
                    // Last data page
                else if (i == (freeDataPages.Count - 1))
                {
                    ((DataPage) freeDataPages[i]).OwnerDataPagePointer =
                        ((DataPage) freeDataPages[i - 1]).Pointer;
                }
                    // Middle range data page
                else
                {
                    ((DataPage) freeDataPages[i]).OwnerDataPagePointer =
                        ((DataPage) freeDataPages[i - 1]).Pointer;
                    ((DataPage) freeDataPages[i]).NextDataPagePointer =
                        ((DataPage) freeDataPages[i + 1]).Pointer;
                }
            }

            DataPage[] retVal = new DataPage[freeDataPages.Count];
            freeDataPages.CopyTo(retVal);

            return retVal;
        }

        /// <summary>
        /// Adds column to db file.
        /// </summary>
        /// <param name="column"></param>
        internal void AddColumn(LDB_DataColumn column)
        {
            // Find free column data area

            // Set position over version, free data pages count and data page data area size
            m_pDbFile.Position = 68;

            long freeColumnPosition = -1;
            StreamLineReader r = new StreamLineReader(m_pDbFile);
            // Loop all columns data areas, see it there any free left
            for (int i = 0; i < 100; i++)
            {
                byte[] columnInfo = r.ReadLine();
                if (columnInfo == null)
                {
                    throw new Exception("Invalid columns data area length !");
                }

                // We found unused column data area
                if (columnInfo[0] == '\0')
                {
                    freeColumnPosition = m_pDbFile.Position;
                    break;
                }
            }
            m_FilePosition = m_pDbFile.Position;

            if (freeColumnPosition != -1)
            {
                // TODO: If there is data ???

                // Move to row start
                SetFilePosition(GetFilePosition() - 102);

                // Store column
                byte[] columnData = column.ToColumnInfo();
                WriteToFile(columnData, 0, columnData.Length);
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
        /// <param name="data">Buffer where to store readed data..</param>
        /// <param name="offset">Offset in array to where to start storing readed data.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns></returns>
        internal int ReadFromFile(byte[] data, int offset, int count)
        {
            int readed = m_pDbFile.Read(data, offset, count);
            m_FilePosition += readed;

            return readed;
        }

        /// <summary>
        /// Writes data to file.
        /// </summary>
        /// <param name="data">Data to write.</param>
        /// <param name="offset">Offset in array from where to start writing data.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <returns></returns>
        internal void WriteToFile(byte[] data, int offset, int count)
        {
            m_pDbFile.Write(data, offset, count);
            m_FilePosition += count;
        }

        /// <summary>
        /// Gets current position in file.
        /// </summary>
        /// <returns></returns>
        internal long GetFilePosition()
        {
            return m_FilePosition;
        }

        /// <summary>
        /// Sets file position.
        /// </summary>
        /// <param name="position">Position in file.</param>
        internal void SetFilePosition(long position)
        {
            if (m_FilePosition != position)
            {
                m_pDbFile.Position = position;
                m_FilePosition = position;
            }
        }

        #endregion
    }
}