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

    #endregion

    /// <summary>
    /// Data page.
    /// </summary>
    internal class DataPage
    {
        #region Members

        private readonly byte[] m_Data;
        private readonly int m_DataAreaSize = 1000;
        private readonly long m_OwnerID;
        private readonly DbFile m_pOwnerDB;
        private readonly long m_StartPointer;
        private readonly bool m_Used;
        private long m_NextDataPagePointer;
        private long m_OwnerDataPagePointer;
        private int m_StoredDataLength;

        #endregion

        #region Properties

        /// <summary>
        /// Gets data page size on disk in bytes.
        /// </summary>
        public int DataPageSize
        {
            get { return 33 + DataAreaSize; }
        }

        /// <summary>
        /// Gets this data page address (offset in database file).
        /// </summary>
        public long Pointer
        {
            get { return m_StartPointer; }
        }

        /// <summary>
        /// Gets or sets if data page used or free space.
        /// </summary>
        public bool Used
        {
            get { return m_Used; }

            set
            {
                m_pOwnerDB.SetFilePosition(m_StartPointer + 2);
                m_pOwnerDB.WriteToFile(new[] {Convert.ToByte(value)}, 0, 1);
            }
        }

        /// <summary>
        /// Gets owner object id what owns this data page.
        /// </summary>
        public long OwnerID
        {
            get { return m_OwnerID; }
        }

        /// <summary>
        /// Gets or sets owner data page pointer.
        /// Returns 0 if this is first data page of multiple data pages or only data page.
        /// </summary>
        public long OwnerDataPagePointer
        {
            get { return m_OwnerDataPagePointer; }

            set
            {
                // owner data page pointer
                m_pOwnerDB.SetFilePosition(m_StartPointer + 11);
                m_pOwnerDB.WriteToFile(ldb_Utils.LongToByte(value), 0, 8);

                m_OwnerDataPagePointer = value;
            }
        }

        /// <summary>
        /// Gets or sets pointer to data page what continues this data page.
        /// Returns 0 if data page has enough room for data and there isn't continuing data page.
        /// </summary>
        public long NextDataPagePointer
        {
            get { return m_NextDataPagePointer; }

            set
            {
                // continuing data page pointer
                m_pOwnerDB.SetFilePosition(m_StartPointer + 19);
                m_pOwnerDB.WriteToFile(ldb_Utils.LongToByte(value), 0, 8);

                m_NextDataPagePointer = value;
            }
        }

        /*
		/// <summary>
		/// Gets or sets data that data page holds. Maximum size is this.DataAreaSize. Returns null if no data stored.
		/// </summary>
		public byte[] Data
		{
			get{ 
				byte[] data = new byte[m_StoredDataLength];
				m_pDbFileStream.Position = m_StartPointer + 33;
				m_pDbFileStream.Read(data,0,data.Length);

				return data; 
			}

			set{
				if(value.Length > this.DataAreaSize){
					throw new Exception("Data page can't store more than " + this.DataAreaSize + " bytes, use mutliple data pages !");
				}
		
				// Set stored data length
				m_pDbFileStream.Position = m_StartPointer + 27;
				byte[] dataLength = ldb_Utils.IntToByte(value.Length);
				m_pDbFileStream.Write(dataLength,0,dataLength.Length);
				
                // Store data
				m_pDbFileStream.Position = m_StartPointer + 33;
				m_pDbFileStream.Write(value,0,value.Length);

				m_StoredDataLength = value.Length;
			}
		}
*/

        /// <summary>
        /// Gets how many data data page can store.
        /// </summary>
        public int DataAreaSize
        {
            get { return m_DataAreaSize; }
        }

        /// <summary>
        /// Gets stored data length.
        /// </summary>
        public int StoredDataLength
        {
            get { return m_StoredDataLength; }
        }

        /// <summary>
        /// Gets how much free data space is availabe in data page.
        /// </summary>
        public long SpaceAvailable
        {
            get { return DataAreaSize - m_StoredDataLength; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="dataPageDataAreaSize">Specifies how much data data page can store.</param>
        /// <param name="ownerDB">Owner DB file..</param>
        /// <param name="startOffset">Data page start offset pointer.</param>
        public DataPage(int dataPageDataAreaSize, DbFile ownerDB, long startOffset)
        {
            /* DataPage structure
			   2 bytes                 - CRLF
			   1 byte                  - used (f - unused,u - used)
			   8 byte	               - owner object id
			   8 bytes                 - owner data page pointer
			   8 bytes                 - continuing data page pointer
			   4 bytes                 - stored data length in data area
			   2 bytes                 - CRLF
			   1000 bytes              - data area
			*/

            m_DataAreaSize = dataPageDataAreaSize;
            m_pOwnerDB = ownerDB;
            m_StartPointer = startOffset;

            byte[] dataPageInfo = new byte[33];
            ownerDB.SetFilePosition(startOffset);
            ownerDB.ReadFromFile(dataPageInfo, 0, dataPageInfo.Length);
            m_Data = new byte[dataPageDataAreaSize];
            ownerDB.ReadFromFile(m_Data, 0, dataPageDataAreaSize);

            // CRLF
            if (dataPageInfo[0] != (byte) '\r')
            {
                throw new Exception(
                    "Not right data page startOffset, or invalid data page <CR> is expected but is '" +
                    (int) dataPageInfo[0] + "' !");
            }
            if (dataPageInfo[1] != (byte) '\n')
            {
                throw new Exception(
                    "Not right data page startOffset, or invalid data page <LF> is expected but is '" +
                    (int) dataPageInfo[1] + "' !");
            }

            // used
            if (dataPageInfo[2] == (byte) 'u')
            {
                m_Used = true;
            }
            else
            {
                m_Used = false;
            }

            // owner object id
            m_OwnerID = ldb_Utils.ByteToLong(dataPageInfo, 3);

            // owner data page pointer
            m_OwnerDataPagePointer = ldb_Utils.ByteToLong(dataPageInfo, 11);

            // continuing data page pointer
            m_NextDataPagePointer = ldb_Utils.ByteToLong(dataPageInfo, 19);

            // stored data length in data area
            m_StoredDataLength = ldb_Utils.ByteToInt(dataPageInfo, 27);

            // CRLF
            if (dataPageInfo[31] != (byte) '\r')
            {
                throw new Exception(
                    "Not right data page startOffset, or invalid data page <CR> is expected but is '" +
                    (int) dataPageInfo[31] + "' !");
            }
            if (dataPageInfo[32] != (byte) '\n')
            {
                throw new Exception(
                    "Not right data page startOffset, or invalid data page <LF> is expected but is '" +
                    (int) dataPageInfo[32] + "' !");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates new data page structure.
        /// </summary>
        /// <param name="dataPageDataAreaSize">Specifies how much data can data page store.</param>
        /// <param name="used">Specifies if data page is used or free space. If this value is false, all toher parameters aren't stored.</param>
        /// <param name="ownerID">Owner data object ID.</param>
        /// <param name="ownerDataPagePointer">This data page owner data page pointer. This value can be 0, if no owner.</param>
        /// <param name="nextDataPagePointer">Data page pointer, what continues this data page. This value can be 0 if, data page won't spread to multiple data pages.</param>
        /// <param name="data">Data what data page stores. Maximum length is dataPageDataAreaSize.</param>
        /// <returns></returns>
        public static byte[] CreateDataPage(int dataPageDataAreaSize,
                                            bool used,
                                            long ownerID,
                                            long ownerDataPagePointer,
                                            long nextDataPagePointer,
                                            byte[] data)
        {
            /* DataPage structure
			   2 bytes                    - CRLF
			   1 byte                     - used (f - unused,u - used)
			   8 byte	                  - owner object id
			   8 bytes                    - owner data page pointer
			   8 bytes                    - continuing data page pointer
			   4 bytes                    - stored data length in data area
			   2 bytes                    - CRLF
			   dataPageDataAreaSize bytes - data area
			*/

            if (data.Length > dataPageDataAreaSize)
            {
                throw new Exception("Data page can store only " + dataPageDataAreaSize +
                                    " bytes, data conatins '" + data.Length + "' bytes !");
            }

            byte[] dataPage = new byte[dataPageDataAreaSize + 33];
            // CRLF
            dataPage[0] = (byte) '\r';
            dataPage[1] = (byte) '\n';
            if (used)
            {
                // used
                dataPage[2] = (byte) 'u';
                // owner object id
                Array.Copy(ldb_Utils.LongToByte(ownerID), 0, dataPage, 3, 8);
                // owner data page pointer
                Array.Copy(ldb_Utils.LongToByte(ownerDataPagePointer), 0, dataPage, 11, 8);
                // continuing data page pointer
                Array.Copy(ldb_Utils.LongToByte(nextDataPagePointer), 0, dataPage, 19, 8);
                // stored data length in data area
                Array.Copy(ldb_Utils.IntToByte(data.Length), 0, dataPage, 27, 4);
                // CRLF
                dataPage[31] = (byte) '\r';
                dataPage[32] = (byte) '\n';
                // data area
                Array.Copy(data, 0, dataPage, 33, data.Length);
            }
            else
            {
                // used
                dataPage[2] = (byte) 'f';
                // CRLF
                dataPage[31] = (byte) '\r';
                dataPage[32] = (byte) '\n';
            }

            return dataPage;
        }

        /// <summary>
        /// Reads specified amount data to buffer.
        /// </summary>
        /// <param name="buffer">Buffer where to store data.</param>
        /// <param name="startIndexInBuffer">Start index in buffer where data storing begins. Start index is included.</param>
        /// <param name="length">Number of bytes to read.</param>
        /// <param name="startOffset">Zero based offset of data area.</param>
        /// <returns></returns>
        public void ReadData(byte[] buffer, int startIndexInBuffer, int length, int startOffset)
        {
            if (startOffset < 0)
            {
                throw new Exception("startOffset can't negative value !");
            }
            if ((length + startOffset) > DataAreaSize)
            {
                throw new Exception("startOffset and length are out of range data page data area !");
            }
            if ((length + startOffset) > m_StoredDataLength)
            {
                throw new Exception(
                    "There isn't so much data stored in data page as requested ! Stored data length = " +
                    m_StoredDataLength + "; start offset = " + startOffset + "; length wanted = " + length);
            }

            Array.Copy(m_Data, startOffset, buffer, startIndexInBuffer, length);
        }

        /// <summary>
        /// Reads data page data. Offset byte is included.
        /// </summary>
        /// <param name="startOffset">Zero based offset of data area.</param>
        /// <param name="length">Specifies how much data to read.</param>
        /// <returns></returns>
        public byte[] ReadData(int startOffset, int length)
        {
            if (startOffset < 0)
            {
                throw new Exception("startOffset can't negative value !");
            }
            if ((length + startOffset) > DataAreaSize)
            {
                throw new Exception("startOffset and length are out of range data page data area !");
            }
            if ((length + startOffset) > m_StoredDataLength)
            {
                throw new Exception(
                    "There isn't so much data stored in data page as requested ! Stored data length = " +
                    m_StoredDataLength + "; start offset = " + startOffset + "; length wanted = " + length);
            }

            byte[] data = new byte[length];
            Array.Copy(m_Data, startOffset, data, 0, length);

            return data;
        }

        /// <summary>
        /// Writed data to data page.
        /// </summary>
        /// <param name="data">Data to write.</param>
        public void WriteData(byte[] data)
        {
            if (data.Length > DataAreaSize)
            {
                throw new Exception("Data page can't store more than " + DataAreaSize +
                                    " bytes, use mutliple data pages !");
            }

            // Set stored data length
            m_pOwnerDB.SetFilePosition(m_StartPointer + 27);
            m_pOwnerDB.WriteToFile(ldb_Utils.IntToByte(data.Length), 0, 4);

            // Store data
            m_pOwnerDB.SetFilePosition(m_StartPointer + 33);
            m_pOwnerDB.WriteToFile(data, 0, data.Length);

            m_StoredDataLength = data.Length;
        }

        #endregion
    }
}