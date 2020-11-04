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


namespace ASC.Mail.Net.IO
{
    #region usings

    using System;
    using System.IO;
    using System.Text;
    using Log;

    #endregion

    /// <summary>
    /// This delegate represents callback method for BeginReadLine.
    /// </summary>
    /// <param name="e">Method data.</param>
    public delegate void ReadLineCallback(ReadLine_EventArgs e);

    /// <summary>
    /// This delegate represents callback method for BeginReadToEnd,BeginReadHeader,BeginReadPeriodTerminated.
    /// </summary>
    /// <param name="e">Method data.</param>
    public delegate void ReadToStreamCallback(ReadToStream_EventArgs e);

    /// <summary>
    /// This delegate represents callback method for BeginWrite.
    /// </summary>
    /// <param name="e">Method data.</param>
    public delegate void WriteCallback(Write_EventArgs e);

    /// <summary>
    /// This delegate represents callback method for BeginWrite,BeginWritePeriodTerminated.
    /// </summary>
    /// <param name="e">Method data.</param>
    public delegate void WriteStreamCallback(WriteStream_EventArgs e);

    /// <summary>
    /// Stream wrapper class, provides many usefull read and write methods for stream.
    /// </summary>
    [Obsolete("Use SmartStream instead.")]
    public class StreamHelper
    {
        #region Nested type: _ToStreamReader

        /// <summary>
        /// Asynchronous to stream reader implementation.
        /// </summary>
        private class _ToStreamReader
        {
            #region Members

            private readonly SizeExceededAction m_ExceededAction = SizeExceededAction.ThrowException;
            private readonly int m_MaxSize = Workaround.Definitions.MaxStreamLineLength;
            private readonly byte[] m_pBuffer;
            private readonly BufferedStream m_pBufferedStream;
            private readonly ReadToStreamCallback m_pCallback;
            private readonly byte[] m_pLineBuffer;
            private readonly Stream m_pStoreStream;
            private readonly StreamHelper m_pStreamHelper;
            private readonly object m_pTag;
            private int m_CountInBuffer;
            private int m_CountToRead;
            private bool m_IsLineSizeExceeded;
            private int m_TotalReadedCount;

            #endregion

            #region Constructor

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="streamHelper">Reference to StreamHelper.</param>
            /// <param name="storeStream">Stream where to store readed data.</param>
            /// <param name="maxSize">Maximum number of bytes to read.</param>
            /// <param name="exceededAction">Specifies how this method behaves when maximum size exceeded.</param>
            /// <param name="callback">Callback what will be called if asynchronous reading compltes.</param>
            /// <param name="tag">User data.</param>
            public _ToStreamReader(StreamHelper streamHelper,
                                   Stream storeStream,
                                   int maxSize,
                                   SizeExceededAction exceededAction,
                                   ReadToStreamCallback callback,
                                   object tag)
            {
                m_pStreamHelper = streamHelper;
                m_pStoreStream = storeStream;
                m_pBufferedStream = new BufferedStream(m_pStoreStream, Workaround.Definitions.MaxStreamLineLength);
                m_MaxSize = maxSize;
                m_ExceededAction = exceededAction;
                m_pCallback = callback;
                m_pTag = tag;
                m_pBuffer = new byte[Workaround.Definitions.MaxStreamLineLength];
                m_pLineBuffer = new byte[4096];
            }

            #endregion

            #region Methods

            /// <summary>
            /// Starts reading specified amount of data.
            /// </summary>
            /// <param name="count">Number of bytes to read from source stream and store to store stream.</param>
            public void BeginRead(int count)
            {
                m_CountToRead = count;

                DoRead();
            }

            /// <summary>
            /// Starts reading period terminated data.
            /// </summary>
            public void BeginReadPeriodTerminated()
            {
                try
                {
                    m_pStreamHelper.BeginReadLineInternal(m_pLineBuffer,
                                                          m_ExceededAction,
                                                          null,
                                                          OnReadPeriodTerminated_ReadLine_Completed,
                                                          false,
                                                          false);
                }
                catch (Exception x)
                {
                    ReadPeriodTerminatedCompleted(x);
                }
            }

            /// <summary>
            /// Starts reading header data.
            /// </summary>
            public void BeginReadHeader()
            {
                try
                {
                    m_pStreamHelper.BeginReadLineInternal(m_pLineBuffer,
                                                          m_ExceededAction,
                                                          null,
                                                          OnReadHeader_ReadLine_Completed,
                                                          false,
                                                          false);
                }
                catch (Exception x)
                {
                    ReadHeaderCompleted(x);
                }
            }

            /// <summary>
            /// Starts reading all source stream data.
            /// </summary>
            public void BeginReadAll()
            {
                DoReadAll();
            }

            #endregion

            #region Utility methods

            /// <summary>
            /// Processes all buffer data and gets new buffer if active buffer consumed.
            /// </summary>
            private void DoRead()
            {
                /* Note: We do own read buffering here, because we cand do buffering here even if
                 *       buffering not enabled for StreamHelper. All this because we read all data anyway. 
                 *       But we need to use StreamHelper read buffer first if there is any data in that buffer.
                */

                try
                {
                    // We have data in buffer, consume it.
                    if (m_CountInBuffer > 0)
                    {
                        m_TotalReadedCount += m_CountInBuffer;

                        // Write readed data to store stream.
                        m_pStoreStream.Write(m_pBuffer, 0, m_CountInBuffer);
                        m_CountInBuffer = 0;
                    }

                    // We have readed all data that was requested.
                    if (m_TotalReadedCount == m_CountToRead)
                    {
                        OnRead_Completed(null);
                    }
                    else
                    {
                        // There is some data in StreamHelper read buffer, we need to consume it first.
                        if (m_pStreamHelper.m_ReadBufferOffset < m_pStreamHelper.m_ReadBufferEndPos)
                        {
                            int countReadedFromBuffer =
                                Math.Min(
                                    m_pStreamHelper.m_ReadBufferEndPos - m_pStreamHelper.m_ReadBufferOffset,
                                    m_CountToRead - m_TotalReadedCount);
                            Array.Copy(m_pStreamHelper.m_pReadBuffer,
                                       m_pStreamHelper.m_ReadBufferOffset,
                                       m_pBuffer,
                                       0,
                                       countReadedFromBuffer);
                            m_pStreamHelper.m_ReadBufferOffset += countReadedFromBuffer;
                        }
                            // Start reading new (local)buffer data block.
                        else
                        {
                            m_pStreamHelper.Stream.BeginRead(m_pBuffer,
                                                             0,
                                                             Math.Min(m_pBuffer.Length,
                                                                      m_CountToRead - m_TotalReadedCount),
                                                             OnRead_ReadBuffer_Completed,
                                                             null);
                        }
                    }
                }
                catch (Exception x)
                {
                    OnRead_Completed(x);
                }
            }

            /// <summary>
            /// Is called when asynchrounous data buffer block reading has completed.
            /// </summary>
            /// <param name="result"></param>
            private void OnRead_ReadBuffer_Completed(IAsyncResult result)
            {
                try
                {
                    m_CountInBuffer = m_pStreamHelper.Stream.EndRead(result);

                    // We reached end of stream, no more data.
                    if (m_CountInBuffer == 0)
                    {
                        OnRead_Completed(new IncompleteDataException());
                    }
                    else
                    {
                        // Continue reading.
                        DoRead();
                    }
                }
                catch (Exception x)
                {
                    OnRead_Completed(x);
                }
            }

            /// <summary>
            /// Is called when ReadHeader has completed.
            /// </summary>
            /// <param name="x">Exception happened during read or null if operation was successfull.</param>
            private void OnRead_Completed(Exception x)
            {
                // Release read lock.
                m_pStreamHelper.m_IsReadActive = false;

                // Log
                if (m_pStreamHelper.Logger != null)
                {
                    m_pStreamHelper.Logger.AddRead(m_TotalReadedCount, null);
                }

                if (m_pCallback != null)
                {
                    m_pCallback(new ReadToStream_EventArgs(x, m_pStoreStream, m_TotalReadedCount, m_pTag));
                }
            }

            /// <summary>
            /// Is called when asynchrounous line reading has completed.
            /// </summary>
            /// <param name="e">Callback data.</param>
            private void OnReadPeriodTerminated_ReadLine_Completed(ReadLine_EventArgs e)
            {
                try
                {
                    m_TotalReadedCount += e.ReadedCount;

                    // We got error.
                    if (e.Exception != null)
                    {
                        try
                        {
                            m_pBufferedStream.Flush();
                            m_pStoreStream.Flush();
                        }
                        catch
                        {
                            // Just skip excpetions here, otherwise we may hide original exception, 
                            // if exceptions is thrown by flush.
                        }

                        // Maximum line size exceeded, but junk data wanted.
                        if (m_ExceededAction == SizeExceededAction.JunkAndThrowException &&
                            e.Exception is LineSizeExceededException)
                        {
                            m_IsLineSizeExceeded = true;

                            // Start reading next data buffer block.
                            m_pStreamHelper.BeginReadLineInternal(m_pLineBuffer,
                                                                  m_ExceededAction,
                                                                  null,
                                                                  OnReadPeriodTerminated_ReadLine_Completed,
                                                                  false,
                                                                  false);
                        }
                            // Unknown exception or ThrowException, so we are done.
                        else
                        {
                            ReadPeriodTerminatedCompleted(e.Exception);
                        }
                    }
                        // We reached end of stream before got period terminator.
                    else if (e.ReadedCount == 0)
                    {
                        ReadPeriodTerminatedCompleted(
                            new IncompleteDataException(
                                "Source stream was reached end of stream and data is not period terminated !"));
                    }
                        // We got terminator, so we are done now.
                    else if (e.Count == 1 && e.LineBuffer[0] == '.')
                    {
                        m_pBufferedStream.Flush();
                        m_pStoreStream.Flush();

                        // LineSizeExceeded.
                        if (m_IsLineSizeExceeded)
                        {
                            ReadPeriodTerminatedCompleted(new LineSizeExceededException());
                        }
                            // DataSizeExceeded.
                        else if (m_TotalReadedCount > m_MaxSize)
                        {
                            ReadPeriodTerminatedCompleted(new DataSizeExceededException());
                        }
                            // Completed successfuly.
                        else
                        {
                            ReadPeriodTerminatedCompleted(null);
                        }
                    }
                        // Just append line to store stream and get next line.
                    else
                    {
                        // Maximum allowed data size exceeded.
                        if (m_TotalReadedCount > m_MaxSize)
                        {
                            if (m_ExceededAction == SizeExceededAction.ThrowException)
                            {
                                ReadPeriodTerminatedCompleted(new DataSizeExceededException());
                                return;
                            }
                            // Junk data.
                            //else{
                            //}
                        }
                        else
                        {
                            // If line starts with period, first period is removed.
                            if (e.LineBuffer[0] == '.')
                            {
                                m_pBufferedStream.Write(e.LineBuffer, 1, e.Count - 1);
                            }
                                // Normal line.
                            else
                            {
                                m_pBufferedStream.Write(e.LineBuffer, 0, e.Count);
                            }
                            // Add line break.
                            m_pBufferedStream.Write(m_pStreamHelper.m_LineBreak,
                                                    0,
                                                    m_pStreamHelper.m_LineBreak.Length);
                        }

                        // Start getting new data buffer block.   
                        m_pStreamHelper.BeginReadLineInternal(m_pLineBuffer,
                                                              m_ExceededAction,
                                                              null,
                                                              OnReadPeriodTerminated_ReadLine_Completed,
                                                              false,
                                                              false);
                    }
                }
                catch (Exception x)
                {
                    ReadPeriodTerminatedCompleted(x);
                }
            }

            /// <summary>
            /// Is called when ReadPeriodTerminated has completd.
            /// </summary>
            /// <param name="x">Exeption happened or null if operation completed successfuly.</param>
            private void ReadPeriodTerminatedCompleted(Exception x)
            {
                // Release read lock.
                m_pStreamHelper.m_IsReadActive = false;

                // Log
                if (m_pStreamHelper.Logger != null)
                {
                    m_pStreamHelper.Logger.AddRead(m_TotalReadedCount, null);
                }

                if (m_pCallback != null)
                {
                    m_pCallback(new ReadToStream_EventArgs(x, m_pStoreStream, m_TotalReadedCount, m_pTag));
                }
            }

            /// <summary>
            /// Is called when asynchrounous line reading has completed.
            /// </summary>
            /// <param name="e">Callback data.</param>
            private void OnReadHeader_ReadLine_Completed(ReadLine_EventArgs e)
            {
                try
                {
                    m_TotalReadedCount += e.ReadedCount;

                    // We got error.
                    if (e.Exception != null)
                    {
                        try
                        {
                            m_pBufferedStream.Flush();
                            m_pStoreStream.Flush();
                        }
                        catch
                        {
                            // Just skip excpetions here, otherwise we may hide original exception, 
                            // if exceptions is thrown by flush.
                        }

                        // Maximum line size exceeded, but junk data wanted.
                        if (m_ExceededAction == SizeExceededAction.JunkAndThrowException &&
                            e.Exception is LineSizeExceededException)
                        {
                            m_IsLineSizeExceeded = true;

                            // Start reading next data buffer block.
                            m_pStreamHelper.BeginReadLineInternal(m_pLineBuffer,
                                                                  m_ExceededAction,
                                                                  null,
                                                                  OnReadHeader_ReadLine_Completed,
                                                                  false,
                                                                  false);
                        }
                            // Unknown exception or ThrowException, so we are done.
                        else
                        {
                            ReadHeaderCompleted(e.Exception);
                        }
                    }
                        // We got terminator, so we are done now.
                    else if (e.Count == 0 || e.ReadedCount == 0)
                    {
                        m_pBufferedStream.Flush();
                        m_pStoreStream.Flush();

                        // LineSizeExceeded.
                        if (m_IsLineSizeExceeded)
                        {
                            ReadHeaderCompleted(new LineSizeExceededException());
                        }
                            // DataSizeExceeded.
                        else if (m_TotalReadedCount > m_MaxSize)
                        {
                            ReadHeaderCompleted(new DataSizeExceededException());
                        }
                            // Completed successfuly.
                        else
                        {
                            ReadHeaderCompleted(null);
                        }
                    }
                        // Just append line to store stream and get next line.
                    else
                    {
                        // Maximum allowed data size exceeded.
                        if (m_TotalReadedCount > m_MaxSize)
                        {
                            if (m_ExceededAction == SizeExceededAction.ThrowException)
                            {
                                ReadHeaderCompleted(new DataSizeExceededException());
                                return;
                            }
                            // Junk data.
                            //else{
                            //}
                        }
                        else
                        {
                            m_pBufferedStream.Write(e.LineBuffer, 0, e.Count);
                            m_pBufferedStream.Write(m_pStreamHelper.m_LineBreak,
                                                    0,
                                                    m_pStreamHelper.m_LineBreak.Length);
                        }

                        // Start reading new line.
                        m_pStreamHelper.BeginReadLineInternal(m_pLineBuffer,
                                                              m_ExceededAction,
                                                              null,
                                                              OnReadHeader_ReadLine_Completed,
                                                              false,
                                                              false);
                    }
                }
                catch (Exception x)
                {
                    ReadHeaderCompleted(x);
                }
            }

            /// <summary>
            /// Is called when ReadHeader has completed.
            /// </summary>
            /// <param name="x">Exception happened during read or null if operation was successfull.</param>
            private void ReadHeaderCompleted(Exception x)
            {
                // Release read lock.
                m_pStreamHelper.m_IsReadActive = false;

                // Log
                if (m_pStreamHelper.Logger != null)
                {
                    m_pStreamHelper.Logger.AddRead(m_TotalReadedCount, null);
                }

                if (m_pCallback != null)
                {
                    m_pCallback(new ReadToStream_EventArgs(x, m_pStoreStream, m_TotalReadedCount, m_pTag));
                }
            }

            /// <summary>
            /// Processes all buffer data and gets new buffer if active buffer consumed.
            /// </summary>
            private void DoReadAll()
            {
                /* Note: We do own read buffering here, because we cand do buffering here even if
                 *       buffering not enabled for StreamHelper. All this because we read all data anyway. 
                 *       But we need to use StreamHelper read buffer first if there is any data in that buffer.
                */

                try
                {
                    // We have data in buffer, consume it.
                    if (m_CountInBuffer > 0)
                    {
                        m_TotalReadedCount += m_CountInBuffer;

                        // Maximum allowed data size exceeded.
                        if (m_TotalReadedCount > m_MaxSize)
                        {
                            if (m_ExceededAction == SizeExceededAction.ThrowException)
                            {
                                ReadAllCompleted(new DataSizeExceededException());
                                return;
                            }
                            // Junk data.
                            //else{
                            //}
                        }
                            // Store readed data store stream.
                        else
                        {
                            m_pStoreStream.Write(m_pBuffer, 0, m_CountInBuffer);
                        }
                        m_CountInBuffer = 0;
                    }

                    // There is some data in StreamHelper read buffer, we need to consume it first.
                    if (m_pStreamHelper.m_ReadBufferOffset < m_pStreamHelper.m_ReadBufferEndPos)
                    {
                        Array.Copy(m_pStreamHelper.m_pReadBuffer,
                                   m_pStreamHelper.m_ReadBufferOffset,
                                   m_pBuffer,
                                   0,
                                   m_pStreamHelper.m_ReadBufferEndPos - m_pStreamHelper.m_ReadBufferOffset);
                        m_pStreamHelper.m_ReadBufferOffset = 0;
                        m_pStreamHelper.m_ReadBufferEndPos = 0;
                    }
                        // Start reading new (local)buffer data block.
                    else
                    {
                        m_pStreamHelper.Stream.BeginRead(m_pBuffer,
                                                         0,
                                                         m_pBuffer.Length,
                                                         OnReadAll_ReadBuffer_Completed,
                                                         null);
                    }
                }
                catch (Exception x)
                {
                    ReadAllCompleted(x);
                }
            }

            /// <summary>
            /// Is called when asynchrounous data buffer block reading has completed.
            /// </summary>
            /// <param name="result"></param>
            private void OnReadAll_ReadBuffer_Completed(IAsyncResult result)
            {
                try
                {
                    m_CountInBuffer = m_pStreamHelper.Stream.EndRead(result);
                    // We reached end of stream, no more data.
                    if (m_CountInBuffer == 0)
                    {
                        // Maximum allowed data size exceeded.
                        if (m_TotalReadedCount > m_MaxSize)
                        {
                            ReadAllCompleted(new DataSizeExceededException());
                        }
                            // ReadToEnd completed successfuly.
                        else
                        {
                            ReadAllCompleted(null);
                        }
                    }
                    else
                    {
                        // Continue reading.
                        DoReadAll();
                    }
                }
                catch (Exception x)
                {
                    ReadAllCompleted(x);
                }
            }

            /// <summary>
            /// Is called when ReadToEnd has completed.
            /// </summary>
            /// <param name="x">Exception happened during read or null if operation was successfull.</param>
            private void ReadAllCompleted(Exception x)
            {
                // Release read lock.
                m_pStreamHelper.m_IsReadActive = false;

                // Log
                if (m_pStreamHelper.Logger != null)
                {
                    m_pStreamHelper.Logger.AddRead(m_TotalReadedCount, null);
                }

                if (m_pCallback != null)
                {
                    m_pCallback(new ReadToStream_EventArgs(x, m_pStoreStream, m_TotalReadedCount, m_pTag));
                }
            }

            #endregion
        }

        #endregion

        #region Members

        private readonly bool m_IsReadBuffered;
        private readonly byte[] m_LineBreak = new[] {(byte) '\r', (byte) '\n'};
        private readonly int m_MaxLineSize = 4096;
        private readonly byte[] m_pLineBuffer;
        private readonly byte[] m_pReadBuffer;
        private readonly byte[] m_pRLine_ByteBuffer;
        private readonly ReadLine_EventArgs m_pRLine_EventArgs;
        private readonly Stream m_pStream;
        // BeginWrite
        private int m_BeginWrite_Count;
        private int m_BeginWrite_Readed;
        // BeginWriteAll
        private int m_BeginWriteAll_MaxSize;
        private int m_BeginWriteAll_Readed;
        // BeginWritePeriodTerminated
        private int m_BeginWritePeriodTerminated_MaxSize;
        private int m_BeginWritePeriodTerminated_Readed;
        private int m_BeginWritePeriodTerminated_Written;
        private bool m_IsReadActive;
        private bool m_IsWriteActive;
        private byte[] m_pBeginWrite_Buffer;
        private WriteStreamCallback m_pBeginWrite_Callback;
        private Stream m_pBeginWrite_Stream;
        private byte[] m_pBeginWriteAll_Buffer;
        private WriteStreamCallback m_pBeginWriteAll_Callback;
        private Stream m_pBeginWriteAll_Stream;
        private BufferedStream m_pBeginWritePeriodTerminated_BufferedStream;
        private WriteStreamCallback m_pBeginWritePeriodTerminated_Callback;
        private Stream m_pBeginWritePeriodTerminated_Stream;
        private ReadLineCallback m_pRLine_Callback;
        private byte[] m_pRLine_LineBuffer;
        private object m_pRLine_Tag;
        private int m_ReadBufferEndPos;
        private int m_ReadBufferOffset;
        private SizeExceededAction m_RLine_ExceedAction = SizeExceededAction.ThrowException;
        private int m_RLine_LineBufferOffset;
        private int m_RLine_LineBufferSize;
        private bool m_RLine_Log;
        private int m_RLine_TotalReadedCount;
        private bool m_RLine_UnlockRead = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets underlying stream.
        /// </summary>
        public Stream Stream
        {
            get { return m_pStream; }
        }

        /// <summary>
        /// Gets maximum allowed line size in bytes.
        /// </summary>
        public int MaximumLineSize
        {
            get { return m_MaxLineSize; }
        }

        /// <summary>
        /// Gets if source stream reads are buffered.
        /// </summary>
        public bool IsReadBuffered
        {
            get { return m_IsReadBuffered; }
        }

        /// <summary>
        /// Gets or sets logger to use for logging.
        /// </summary>
        public Logger Logger { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Source stream.</param>
        public StreamHelper(Stream stream) : this(stream, 4096, true) {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Source stream.</param>
        /// <param name="maxLineSize">Specifies maximum line size in bytes.</param>
        /// <param name="bufferRead">Specifies if source stream reads are buffered..</param>
        public StreamHelper(Stream stream, int maxLineSize, bool bufferRead)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (maxLineSize < 1 || maxLineSize > Workaround.Definitions.MaxStreamLineLength)
            {
                throw new ArgumentException("Parameter maxLineSize value must be >= 1 and <= Workaround.Definitions.MaxStreamLineLength !");
            }

            m_IsReadBuffered = bufferRead;
            if (m_IsReadBuffered)
            {
                m_pReadBuffer = new byte[Workaround.Definitions.MaxStreamLineLength];
            }

            m_pStream = stream;
            m_pLineBuffer = new byte[maxLineSize];
            m_MaxLineSize = maxLineSize;

            m_pRLine_EventArgs = new ReadLine_EventArgs();
            m_pRLine_ByteBuffer = new byte[1];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reades byte from source stream. Returns -1 if end of stream reached and no more data.
        /// </summary>
        /// <returns>Returns readed byte or -1 if end of stream reached.</returns>
        public int ReadByte()
        {
            if (m_IsReadBuffered)
            {
                if (m_ReadBufferOffset >= m_ReadBufferEndPos)
                {
                    m_ReadBufferEndPos = m_pStream.Read(m_pReadBuffer, 0, Workaround.Definitions.MaxStreamLineLength);
                    m_ReadBufferOffset = 0;

                    // We reached end of stream.
                    if (m_ReadBufferEndPos == 0)
                    {
                        return -1;
                    }
                }

                m_ReadBufferOffset++;
                return m_pReadBuffer[m_ReadBufferOffset - 1];
            }
            else
            {
                return m_pStream.ReadByte();
            }
        }

        /// <summary>
        /// Starts reading specified amount data and storing to the specified store stream.
        /// </summary>
        /// <param name="storeStream">Stream where to store readed data.</param>
        /// <param name="count">Number of bytes to read from source stream and write to store stream.</param>
        /// <param name="callback">Callback to be called if asynchronous reading completes.</param>
        /// <param name="tag">User data.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>storeStream</b> is null.</exception>
        /// <exception cref="ArgumentException">Raised when <b>count</b> less than 1.</exception>
        public void BeginRead(Stream storeStream, int count, ReadToStreamCallback callback, object tag)
        {
            if (storeStream == null)
            {
                throw new ArgumentNullException("storeStream");
            }
            if (count < 1)
            {
                throw new ArgumentException("Parameter count value must be >= 1 !");
            }
            lock (this)
            {
                if (m_IsReadActive)
                {
                    throw new InvalidOperationException(
                        "There is pending read operation, multiple read operations not allowed !");
                }
                m_IsReadActive = true;
            }

            _ToStreamReader reader = new _ToStreamReader(this,
                                                         storeStream,
                                                         0,
                                                         SizeExceededAction.ThrowException,
                                                         callback,
                                                         tag);
            reader.BeginRead(count);
        }

        /// <summary>
        /// Reads specified amount of data from source stream and stores to specified store stream.
        /// </summary>
        /// <param name="storeStream">Stream where to store readed data.</param>
        /// <param name="count">Number of bytes to read from source stream and write to store stream.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>storeStream</b> is null.</exception>
        /// <exception cref="ArgumentException">Raised when <b>count</b> less than 1.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        /// <exception cref="IncompleteDataException">Raised source stream has reached end of stream and doesn't have so much data as specified by <b>count</b> argument.</exception>
        public void Read(Stream storeStream, int count)
        {
            if (storeStream == null)
            {
                throw new ArgumentNullException("storeStream");
            }
            if (count < 1)
            {
                throw new ArgumentException("Parameter count value must be >= 1 !");
            }
            lock (this)
            {
                if (m_IsReadActive)
                {
                    throw new InvalidOperationException(
                        "There is pending read operation, multiple read operations not allowed !");
                }
                m_IsReadActive = true;
            }

            try
            {
                byte[] buffer = new byte[Workaround.Definitions.MaxStreamLineLength];
                int totalReadedCount = 0;
                int readedCount = 0;
                while (totalReadedCount < count)
                {
                    // We have data in read buffer, we must consume it first !
                    if (m_ReadBufferOffset < m_ReadBufferEndPos)
                    {
                        int countReadedFromBuffer = Math.Min(m_ReadBufferEndPos - m_ReadBufferOffset,
                                                             count - totalReadedCount);
                        Array.Copy(m_pReadBuffer, m_ReadBufferOffset, buffer, 0, countReadedFromBuffer);
                        m_ReadBufferOffset += countReadedFromBuffer;
                    }
                        // Just get read next data block.
                    else
                    {
                        readedCount = m_pStream.Read(buffer,
                                                     0,
                                                     Math.Min(buffer.Length, count - totalReadedCount));
                    }

                    // We have reached end of stream, no more data.
                    if (readedCount == 0)
                    {
                        throw new IncompleteDataException(
                            "Underlaying stream don't have so much data than requested, end of stream reached !");
                    }
                    totalReadedCount += readedCount;

                    // Write readed data to store stream.
                    storeStream.Write(buffer, 0, readedCount);
                }

                // Log
                if (Logger != null)
                {
                    Logger.AddRead(totalReadedCount, null);
                }
            }
            finally
            {
                m_IsReadActive = false;
            }
        }

        /// <summary>
        /// Starts reading line from source stream.
        /// </summary>
        /// <param name="buffer">Buffer where to store line data.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">Callback to be called whan asynchronous operation completes.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>buffer</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        public void BeginReadLine(byte[] buffer, object tag, ReadLineCallback callback)
        {
            BeginReadLine(buffer, SizeExceededAction.ThrowException, tag, callback);
        }

        /// <summary>
        /// Starts reading line from source stream.
        /// </summary>
        /// <param name="buffer">Buffer where to store line data.</param>
        /// <param name="exceededAction">Specifies how this method behaves when maximum line size exceeded.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">Callback to be called whan asynchronous operation completes.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>buffer</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        public void BeginReadLine(byte[] buffer,
                                  SizeExceededAction exceededAction,
                                  object tag,
                                  ReadLineCallback callback)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            lock (this)
            {
                if (m_IsReadActive)
                {
                    throw new InvalidOperationException(
                        "There is pending read operation, multiple read operations not allowed !");
                }
                m_IsReadActive = false;
            }

            BeginReadLineInternal(buffer, exceededAction, tag, callback, true, true);
        }

        /// <summary>
        /// Reads line from source stream and stores to specified buffer. This method accepts LF or CRLF lines.
        /// </summary>
        /// <param name="buffer">Buffer where to store line data.</param>
        /// <returns>Returns number of bytes stored to buffer, returns -1 if end of stream reached and no more data.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>buffer</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        /// <exception cref="LineSizeExceededException">Raised when maximum allowed line size has exceeded.</exception>
        public int ReadLine(byte[] buffer)
        {
            return ReadLine(buffer, SizeExceededAction.ThrowException);
        }

        /// <summary>
        /// Reads line from source stream and stores to specified buffer. This method accepts LF or CRLF lines.
        /// </summary>
        /// <param name="buffer">Buffer where to store line data.</param>
        /// <param name="exceededAction">Specifies how this method behaves when maximum line size exceeded.</param>
        /// <returns>Returns number of bytes stored to buffer, returns -1 if end of stream reached and no more data.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>buffer</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        /// <exception cref="LineSizeExceededException">Raised when maximum allowed line size has exceeded.</exception>
        public int ReadLine(byte[] buffer, SizeExceededAction exceededAction)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            lock (this)
            {
                if (m_IsReadActive)
                {
                    throw new InvalidOperationException(
                        "There is pending read operation, multiple read operations not allowed !");
                }
                else
                {
                    m_IsReadActive = true;
                }
            }

            try
            {
                int readedCount = 0;
                return ReadLineInternal(buffer, exceededAction, out readedCount, true);
            }
            finally
            {
                m_IsReadActive = false;
            }
        }

        /// <summary>
        /// Reads line from source stream.
        /// </summary>
        /// <param name="encoding">Encoding to use to decode line.</param>
        /// <returns>Returns readed line with specified encoding or null if end of stream reached and no more data.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>encoding</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        /// <exception cref="LineSizeExceededException">Raised when maximum allowed line size has exceeded.</exception>
        public string ReadLine(Encoding encoding)
        {
            return ReadLine(encoding, SizeExceededAction.ThrowException);
        }

        /// <summary>
        /// Reads line from source stream.
        /// </summary>
        /// <param name="encoding">Encoding to use to decode line.</param>
        /// <param name="exceededAction">Specifies how this method behaves when maximum line size exceeded.</param>
        /// <returns>Returns readed line with specified encoding or null if end of stream reached and no more data.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>encoding</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        /// <exception cref="LineSizeExceededException">Raised when maximum allowed line size has exceeded.</exception>
        public string ReadLine(Encoding encoding, SizeExceededAction exceededAction)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            int readedCount = ReadLine(m_pLineBuffer, exceededAction);
            if (readedCount == -1)
            {
                return null;
            }
            else
            {
                return encoding.GetString(m_pLineBuffer, 0, readedCount);
            }
        }

        /// <summary>
        /// Reads line from source stream and stores to specified buffer. This method accepts LF or CRLF lines.
        /// </summary>
        /// <param name="buffer">Buffer where to store line data.</param>
        /// <param name="exceededAction">Specifies how this method behaves when maximum line size exceeded.</param>
        /// <param name="readedCount">Returns how many bytes this method actually readed form source stream.</param>
        /// <param name="log">Specifies if read line is logged.</param>
        /// <returns>Returns number of bytes stored to buffer, returns -1 if end of stream reached and no more data.</returns>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        /// <exception cref="LineSizeExceededException">Raised when maximum allowed line size has exceeded.</exception>
        public int ReadLineInternal(byte[] buffer,
                                    SizeExceededAction exceededAction,
                                    out int readedCount,
                                    bool log)
        {
            readedCount = 0;
            int bufferSize = buffer.Length;
            int posInBuffer = 0;
            int currentByte = 0;

            /* Because performance gain we need todo,2 while, buffered and non buffered read.
               Each if clause in this while adds about 5% cpu.
            */

            #region Buffered

            if (m_IsReadBuffered)
            {
                while (true)
                {
                    //--- Read byte -----------------------------------------------------
                    if (m_ReadBufferOffset >= m_ReadBufferEndPos)
                    {
                        m_ReadBufferEndPos = m_pStream.Read(m_pReadBuffer, 0, Workaround.Definitions.MaxStreamLineLength);
                        m_ReadBufferOffset = 0;

                        // We reached end of stream.
                        if (m_ReadBufferEndPos == 0)
                        {
                            break;
                        }
                    }

                    currentByte = m_pReadBuffer[m_ReadBufferOffset];
                    m_ReadBufferOffset++;
                    readedCount++;
                    //-------------------------------------------------------------------

                    // We have LF.
                    if (currentByte == '\n')
                    {
                        break;
                    }
                        // We just skip all CR.
                    else if (currentByte == '\r') {}
                        // Maximum allowed line size exceeded.
                    else if (readedCount > bufferSize)
                    {
                        if (exceededAction == SizeExceededAction.ThrowException)
                        {
                            throw new LineSizeExceededException();
                        }
                    }
                        // Store readed byte.
                    else
                    {
                        buffer[posInBuffer] = (byte) currentByte;
                        posInBuffer++;
                    }
                }
            }

                #endregion

                #region No-buffered

            else
            {
                while (true)
                {
                    // Read byte
                    currentByte = m_pStream.ReadByte();
                    // We reached end of stream, no more data.
                    if (currentByte == -1)
                    {
                        break;
                    }
                    readedCount++;

                    // We have LF.
                    if (currentByte == '\n')
                    {
                        break;
                    }
                        // We just skip all CR.
                    else if (currentByte == '\r') {}
                        // Maximum allowed line size exceeded.
                    else if (readedCount > bufferSize)
                    {
                        if (exceededAction == SizeExceededAction.ThrowException)
                        {
                            throw new LineSizeExceededException();
                        }
                    }
                        // Store readed byte.
                    else
                    {
                        buffer[posInBuffer] = (byte) currentByte;
                        posInBuffer++;
                    }
                }
            }

            #endregion

            // We are end of stream, no more data.
            if (readedCount == 0)
            {
                return -1;
            }
            // Maximum allowed line size exceeded.
            if (readedCount > m_MaxLineSize)
            {
                throw new LineSizeExceededException();
            }

            // Log
            if (log && Logger != null)
            {
                Logger.AddRead(readedCount, Encoding.Default.GetString(buffer, 0, readedCount));
            }

            return posInBuffer;
        }

        /// <summary>
        /// Starts reading all source stream data.
        /// </summary>
        /// <param name="storeStream">Stream where to store data.</param>
        /// <param name="maxSize">Maximum muber of bytes to read.</param>
        /// <param name="exceededAction">Specifies how this method behaves when maximum size exceeded.</param>
        /// <param name="callback">Callback to be called if asynchronous reading completes.</param>
        /// <param name="tag">User data.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>storeStream</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        public void BeginReadAll(Stream storeStream,
                                 int maxSize,
                                 SizeExceededAction exceededAction,
                                 ReadToStreamCallback callback,
                                 object tag)
        {
            if (storeStream == null)
            {
                throw new ArgumentNullException("storeStream");
            }
            lock (this)
            {
                if (m_IsReadActive)
                {
                    throw new InvalidOperationException(
                        "There is pending read operation, multiple read operations not allowed !");
                }
                m_IsReadActive = false;
            }

            _ToStreamReader reader = new _ToStreamReader(this,
                                                         storeStream,
                                                         maxSize,
                                                         exceededAction,
                                                         callback,
                                                         tag);
            reader.BeginReadAll();
        }

        /// <summary>
        /// Reads all source stream data and stores to the specified store stream.
        /// </summary>
        /// <param name="storeStream">Stream where to store readed data.</param>
        /// <param name="maxSize">Maximum muber of bytes to read.</param>
        /// <returns>Returns number of bytes written to <b>storeStream</b>.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>storeStream</b> is null.</exception>
        /// <exception cref="ArgumentException">Raised when <b>maxSize</b> less than 1.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        /// <exception cref="DataSizeExceededException">Raised when maximum allowed data size has exceeded.</exception>
        public int ReadAll(Stream storeStream, int maxSize)
        {
            return ReadAll(storeStream, maxSize, SizeExceededAction.ThrowException);
        }

        /// <summary>
        /// Reads all source stream data and stores to the specified store stream.
        /// </summary>
        /// <param name="storeStream">Stream where to store readed data.</param>
        /// <param name="maxSize">Maximum muber of bytes to read.</param>
        /// <param name="exceededAction">Specifies how this method behaves when maximum size exceeded.</param>
        /// <returns>Returns number of bytes written to <b>storeStream</b>.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>storeStream</b> is null.</exception>
        /// <exception cref="ArgumentException">Raised when <b>maxSize</b> less than 1.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        /// <exception cref="DataSizeExceededException">Raised when maximum allowed data size has exceeded.</exception>
        public int ReadAll(Stream storeStream, int maxSize, SizeExceededAction exceededAction)
        {
            if (storeStream == null)
            {
                throw new ArgumentNullException("storeStream");
            }
            if (maxSize < 1)
            {
                throw new ArgumentException("Parameter maxSize value must be >= 1 !");
            }
            lock (this)
            {
                if (m_IsReadActive)
                {
                    throw new InvalidOperationException(
                        "There is pending read operation, multiple read operations not allowed !");
                }
                m_IsReadActive = true;
            }

            try
            {
                byte[] buffer = new byte[Workaround.Definitions.MaxStreamLineLength];
                int totalReadedCount = 0;
                int readedCount = 0;
                while (true)
                {
                    // We have data in read buffer, we must consume it first !
                    if (m_ReadBufferOffset < m_ReadBufferEndPos)
                    {
                        Array.Copy(m_pLineBuffer,
                                   m_ReadBufferOffset,
                                   buffer,
                                   0,
                                   m_ReadBufferEndPos - m_ReadBufferOffset);
                        m_ReadBufferOffset = 0;
                        m_ReadBufferEndPos = 0;
                    }
                        // Just get read next data block.
                    else
                    {
                        readedCount = m_pStream.Read(buffer, 0, buffer.Length);
                    }

                    // End of stream reached, no more data.
                    if (readedCount == 0)
                    {
                        break;
                    }
                    totalReadedCount += readedCount;

                    // Maximum allowed data size exceeded.
                    if (totalReadedCount > maxSize)
                    {
                        if (exceededAction == SizeExceededAction.ThrowException)
                        {
                            throw new DataSizeExceededException();
                        }
                    }
                    else
                    {
                        storeStream.Write(buffer, 0, readedCount);
                    }
                }

                // Maximum allowed data size exceeded, some data junked.
                if (totalReadedCount > maxSize)
                {
                    throw new DataSizeExceededException();
                }

                // Log
                if (Logger != null)
                {
                    Logger.AddRead(totalReadedCount, null);
                }

                return totalReadedCount;
            }
            finally
            {
                m_IsReadActive = false;
            }
        }

        /// <summary>
        /// Starts reading header from source stream. Reads header data while gets blank line, what is 
        /// header terminator. For example this method can be used for reading mail,http,sip, ... headers.
        /// </summary>
        /// <param name="storeStream">Stream where to store data.</param>
        /// <param name="maxSize">Maximum muber of bytes to read.</param>
        /// <param name="exceededAction">Specifies how this method behaves when maximum size exceeded.</param>
        /// <param name="callback">Callback to be called if asynchronous reading completes.</param>
        /// <param name="tag">User data.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>storeStream</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        public void BeginReadHeader(Stream storeStream,
                                    int maxSize,
                                    SizeExceededAction exceededAction,
                                    ReadToStreamCallback callback,
                                    object tag)
        {
            if (storeStream == null)
            {
                throw new ArgumentNullException("storeStream");
            }
            lock (this)
            {
                if (m_IsReadActive)
                {
                    throw new InvalidOperationException(
                        "There is pending read operation, multiple read operations not allowed !");
                }
                m_IsReadActive = false;
            }

            _ToStreamReader reader = new _ToStreamReader(this,
                                                         storeStream,
                                                         maxSize,
                                                         exceededAction,
                                                         callback,
                                                         tag);
            reader.BeginReadHeader();
        }

        /// <summary>
        /// Reads header from source stream and stores to the specified stream. Reads header data while 
        /// gets blank line, what is header terminator. For example this method can be used for reading 
        /// mail,http,sip, ... headers.
        /// </summary>
        /// <param name="storeStream">Stream where to store readed data.</param>
        /// <param name="maxSize">Maximum number of bytes to read.</param>
        /// <param name="exceededAction">Specifies how this method behaves when maximum line or data size exceeded.</param>
        /// <returns>Returns number of bytes written to <b>storeStream</b>.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>storeStream</b> is null.</exception>
        /// <exception cref="ArgumentException">Raised when <b>maxSize</b> less than 1.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        /// <exception cref="LineSizeExceededException">Raised when maximum allowed line size has exceeded.</exception>
        /// <exception cref="DataSizeExceededException">Raised when maximum allowed data size has exceeded.</exception>
        public int ReadHeader(Stream storeStream, int maxSize, SizeExceededAction exceededAction)
        {
            if (storeStream == null)
            {
                throw new ArgumentNullException("storeStream");
            }
            lock (this)
            {
                if (m_IsReadActive)
                {
                    throw new InvalidOperationException(
                        "There is pending read operation, multiple read operations not allowed !");
                }
                else
                {
                    m_IsReadActive = true;
                }
            }

            try
            {
                BufferedStream bufferedStoreStream = new BufferedStream(storeStream, Workaround.Definitions.MaxStreamLineLength);
                bool lineSizeExceeded = false;
                int totalReadedCount = 0;
                int readedCount = 0;
                int rawReadedCount = 0;
                while (true)
                {
                    // Read line.
                    readedCount = ReadLineInternal(m_pLineBuffer,
                                                   SizeExceededAction.ThrowException,
                                                   out rawReadedCount,
                                                   false);

                    // We have reached end of stream, no more data.
                    if (rawReadedCount == 0)
                    {
                        break;
                    }
                    totalReadedCount += rawReadedCount;

                    // We got header terminator.
                    if (readedCount == 0)
                    {
                        break;
                    }
                    else
                    {
                        // Maximum allowed data size exceeded.
                        if (totalReadedCount > maxSize)
                        {
                            if (exceededAction == SizeExceededAction.ThrowException)
                            {
                                throw new DataSizeExceededException();
                            }
                        }
                            // Write readed bytes to store stream.
                        else
                        {
                            bufferedStoreStream.Write(m_pLineBuffer, 0, readedCount);
                            bufferedStoreStream.Write(m_LineBreak, 0, m_LineBreak.Length);
                        }
                    }
                }
                bufferedStoreStream.Flush();

                // Maximum allowed line size exceeded, some data is junked.
                if (lineSizeExceeded)
                {
                    throw new LineSizeExceededException();
                }
                // Maximum allowed data size exceeded, some data is junked.
                if (totalReadedCount > maxSize)
                {
                    throw new DataSizeExceededException();
                }

                // Log
                if (Logger != null)
                {
                    Logger.AddRead(totalReadedCount, null);
                }

                return totalReadedCount;
            }
            finally
            {
                m_IsReadActive = false;
            }
        }

        /// <summary>
        /// Begins reading period terminated data from source stream. Reads data while gets single period on line,
        /// what is data terminator.
        /// </summary>
        /// <param name="storeStream">Stream where to store data.</param>
        /// <param name="maxSize">Maximum muber of bytes to read.</param>
        /// <param name="exceededAction">Specifies how this method behaves when maximum size exceeded.</param>
        /// <param name="callback">Callback to be called if asynchronous reading completes.</param>
        /// <param name="tag">User data.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>storeStream</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        public void BeginReadPeriodTerminated(Stream storeStream,
                                              int maxSize,
                                              SizeExceededAction exceededAction,
                                              ReadToStreamCallback callback,
                                              object tag)
        {
            if (storeStream == null)
            {
                throw new ArgumentNullException("storeStream");
            }
            lock (this)
            {
                if (m_IsReadActive)
                {
                    throw new InvalidOperationException(
                        "There is pending read operation, multiple read operations not allowed !");
                }
                m_IsReadActive = false;
            }

            _ToStreamReader reader = new _ToStreamReader(this,
                                                         storeStream,
                                                         maxSize,
                                                         exceededAction,
                                                         callback,
                                                         tag);
            reader.BeginReadPeriodTerminated();
        }

        /// <summary>
        /// Reads period terminated data from source stream. Reads data while gets single period on line,
        /// what is data terminator.
        /// </summary>
        /// <param name="storeStream">Stream where to store readed data.</param>
        /// <param name="maxSize">Maximum number of bytes to read.</param>
        /// <param name="exceededAction">Specifies how this method behaves when maximum size exceeded.</param>
        /// <returns>Returns number of bytes written to <b>storeStream</b>.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>storeStream</b> is null.</exception>
        /// <exception cref="ArgumentException">Raised when <b>maxSize</b> less than 1.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending read operation.</exception>
        /// <exception cref="LineSizeExceededException">Raised when maximum allowed line size has exceeded.</exception>
        /// <exception cref="DataSizeExceededException">Raised when maximum allowed data size has exceeded.</exception>
        /// <exception cref="IncompleteDataException">Raised when source stream was reached end of stream and data is not period terminated.</exception>
        public int ReadPeriodTerminated(Stream storeStream, int maxSize, SizeExceededAction exceededAction)
        {
            if (storeStream == null)
            {
                throw new ArgumentNullException("storeStream");
            }
            lock (this)
            {
                if (m_IsReadActive)
                {
                    throw new InvalidOperationException(
                        "There is pending read operation, multiple read operations not allowed !");
                }
                else
                {
                    m_IsReadActive = true;
                }
            }

            try
            {
                BufferedStream bufferedStoreStream = new BufferedStream(storeStream, Workaround.Definitions.MaxStreamLineLength);
                bool lineSizeExceeded = false;
                bool isPeriodTerminated = false;
                int totalReadedCount = 0;
                int readedCount = 0;
                int rawReadedCount = 0;

                // Just break reading at once if maximum allowed line or data size exceeded.
                if (exceededAction == SizeExceededAction.ThrowException)
                {
                    try
                    {
                        // Read first line.
                        readedCount = ReadLineInternal(m_pLineBuffer,
                                                       SizeExceededAction.JunkAndThrowException,
                                                       out rawReadedCount,
                                                       false);
                    }
                    catch (LineSizeExceededException x)
                    {
                        string dummy = x.Message;
                        lineSizeExceeded = true;
                    }
                    while (rawReadedCount != 0)
                    {
                        totalReadedCount += rawReadedCount;

                        // We have data terminator "<CRLF>.<CRLF>".
                        if (readedCount == 1 && m_pLineBuffer[0] == '.')
                        {
                            isPeriodTerminated = true;
                            break;
                        }
                            // If line starts with period(.), first period is removed.
                        else if (m_pLineBuffer[0] == '.')
                        {
                            // Maximum allowed line or data size exceeded.
                            if (lineSizeExceeded || totalReadedCount > maxSize)
                            {
                                // Junk data 
                            }
                                // Write readed line to store stream.
                            else
                            {
                                bufferedStoreStream.Write(m_pLineBuffer, 1, readedCount - 1);
                                bufferedStoreStream.Write(m_LineBreak, 0, m_LineBreak.Length);
                            }
                        }
                            // Normal line.
                        else
                        {
                            // Maximum allowed line or data size exceeded.
                            if (lineSizeExceeded || totalReadedCount > maxSize)
                            {
                                // Junk data                              
                            }
                                // Write readed line to store stream.
                            else
                            {
                                bufferedStoreStream.Write(m_pLineBuffer, 0, readedCount);
                                bufferedStoreStream.Write(m_LineBreak, 0, m_LineBreak.Length);
                            }
                        }

                        try
                        {
                            // Read next line.
                            readedCount = ReadLineInternal(m_pLineBuffer,
                                                           SizeExceededAction.JunkAndThrowException,
                                                           out rawReadedCount,
                                                           false);
                        }
                        catch (LineSizeExceededException x)
                        {
                            string dummy = x.Message;
                            lineSizeExceeded = true;
                        }
                    }
                }
                    // Read and junk all data if maximum allowed line or data size exceeded.
                else
                {
                    // Read first line.
                    readedCount = ReadLineInternal(m_pLineBuffer,
                                                   SizeExceededAction.JunkAndThrowException,
                                                   out rawReadedCount,
                                                   false);
                    while (rawReadedCount != 0)
                    {
                        totalReadedCount += rawReadedCount;

                        // We have data terminator "<CRLF>.<CRLF>".
                        if (readedCount == 1 && m_pLineBuffer[0] == '.')
                        {
                            isPeriodTerminated = true;
                            break;
                        }
                            // If line starts with period(.), first period is removed.
                        else if (m_pLineBuffer[0] == '.')
                        {
                            // Maximum allowed size exceeded.
                            if (totalReadedCount > maxSize)
                            {
                                throw new DataSizeExceededException();
                            }

                            // Write readed line to store stream.
                            bufferedStoreStream.Write(m_pLineBuffer, 1, readedCount - 1);
                            bufferedStoreStream.Write(m_LineBreak, 0, m_LineBreak.Length);
                        }
                            // Normal line.
                        else
                        {
                            // Maximum allowed size exceeded.
                            if (totalReadedCount > maxSize)
                            {
                                throw new DataSizeExceededException();
                            }

                            // Write readed line to store stream.
                            bufferedStoreStream.Write(m_pLineBuffer, 0, readedCount);
                            bufferedStoreStream.Write(m_LineBreak, 0, m_LineBreak.Length);
                        }

                        // Read next line.
                        readedCount = ReadLineInternal(m_pLineBuffer,
                                                       SizeExceededAction.JunkAndThrowException,
                                                       out rawReadedCount,
                                                       false);
                    }
                }
                bufferedStoreStream.Flush();

                // Log
                if (Logger != null)
                {
                    Logger.AddRead(totalReadedCount, null);
                }

                if (lineSizeExceeded)
                {
                    throw new LineSizeExceededException();
                }
                if (!isPeriodTerminated)
                {
                    throw new IncompleteDataException(
                        "Source stream was reached end of stream and data is not period terminated !");
                }
                if (totalReadedCount > maxSize)
                {
                    throw new DataSizeExceededException();
                }

                return totalReadedCount;
            }
            finally
            {
                m_IsReadActive = false;
            }
        }

        /// <summary>
        /// Starts writing specified data to source stream.
        /// </summary>
        /// <param name="data">Data what to write to source stream.</param>
        /// <param name="callback">Callback to be callled if write completes.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>data</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        public void BeginWrite(byte[] data, WriteCallback callback)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            BeginWrite(data, 0, data.Length, callback);
        }

        /// <summary>
        /// Starts writing specified data to source stream.
        /// </summary>
        /// <param name="data">Data what to write to source stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the source stream.</param>
        /// <param name="count">The number of bytes to be written to the source stream.</param>
        /// <param name="callback">Callback to be callled if write completes.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>data</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        public void BeginWrite(byte[] data, int offset, int count, WriteCallback callback)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            lock (this)
            {
                if (m_IsWriteActive)
                {
                    throw new InvalidOperationException(
                        "There is pending write operation, multiple write operations not allowed !");
                }
                m_IsWriteActive = true;
            }

            // Start writing data block.
            m_pStream.BeginWrite(data,
                                 offset,
                                 count,
                                 InternalBeginWriteCallback,
                                 new object[] {count, callback});
        }

        /// <summary>
        /// Strats writing specified amount of data from <b>stream</b> to source stream.
        /// </summary>
        /// <param name="stream">Stream which data to wite to source stream.</param>
        /// <param name="count">Number of bytes read from <b>stream</b> and write to source stream.</param>
        /// <param name="callback">Callback to be called if asynchronous write completes.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>stream</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        public void BeginWrite(Stream stream, int count, WriteStreamCallback callback)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            lock (this)
            {
                if (m_IsWriteActive)
                {
                    throw new InvalidOperationException(
                        "There is pending write operation, multiple write operations not allowed !");
                }
                m_IsWriteActive = true;
            }

            m_pBeginWrite_Buffer = new byte[Workaround.Definitions.MaxStreamLineLength];
            m_pBeginWrite_Stream = stream;
            m_BeginWrite_Count = count;
            m_pBeginWrite_Callback = callback;
            m_BeginWrite_Readed = 0;

            // Start reading data block.
            m_pBeginWrite_Stream.BeginRead(m_pBeginWrite_Buffer,
                                           0,
                                           Math.Min(m_pBeginWrite_Buffer.Length, m_BeginWrite_Count),
                                           InternalBeginWriteStreamCallback,
                                           null);
        }

        /// <summary>
        /// Writes specified buffer data to source stream.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>data</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        public void Write(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes specified buffer data to source stream.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the source stream.</param>
        /// <param name="count">The number of bytes to be written to the source stream.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>data</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        public void Write(byte[] data, int offset, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            lock (this)
            {
                if (m_IsWriteActive)
                {
                    throw new InvalidOperationException(
                        "There is pending write operation, multiple write operations not allowed !");
                }
                m_IsWriteActive = true;
            }

            try
            {
                m_pStream.Write(data, offset, count);

                // Log
                if (Logger != null)
                {
                    Logger.AddWrite(count, null);
                }
            }
            finally
            {
                m_IsWriteActive = false;
            }
        }

        /// <summary>
        /// Reads specified amount of data for the specified stream and writes it to source stream.
        /// </summary>
        /// <param name="stream">Stream from where to read data.</param>
        /// <param name="count">Number of bytes to read and write.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Raised when argument <b>count</b> is less than 1.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        /// <exception cref="IncompleteDataException">Raised <b>stream</b> has reached end of stream and doesn't have so much data as specified by <b>count</b> argument.</exception>
        public void Write(Stream stream, int count)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (count < 1)
            {
                throw new ArgumentException("Parameter count value must be >= 1 !");
            }
            lock (this)
            {
                if (m_IsWriteActive)
                {
                    throw new InvalidOperationException(
                        "There is pending write operation, multiple write operations not allowed !");
                }
                m_IsWriteActive = true;
            }

            try
            {
                byte[] buffer = new byte[Workaround.Definitions.MaxStreamLineLength];
                int totalReadedCount = 0;
                int readedCount = 0;
                while (totalReadedCount < count)
                {
                    // Read data block
                    readedCount = stream.Read(buffer, 0, Math.Min(buffer.Length, count - totalReadedCount));

                    // We reached end of stream, no more data. That means we didn't get so much data than requested.
                    if (readedCount == 0)
                    {
                        throw new IncompleteDataException(
                            "Stream reached end of stream before we got requested count of data !");
                    }
                    totalReadedCount += readedCount;

                    // Write readed data to source stream.
                    m_pStream.Write(buffer, 0, readedCount);
                }
                m_pStream.Flush();

                // Log
                if (Logger != null)
                {
                    Logger.AddWrite(count, null);
                }
            }
            finally
            {
                m_IsWriteActive = false;
            }
        }

        /// <summary>
        /// Starts writing all <b>stream</b> data to source stream.
        /// </summary>
        /// <param name="stream">Stream which data to write.</param>
        /// <param name="maxSize">Maximum number of bytes to read from <b>stream</b>.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">Callback to be called if asynchronous write completes.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>stream</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        public void BeginWriteAll(Stream stream, int maxSize, object tag, WriteStreamCallback callback)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            lock (this)
            {
                if (m_IsWriteActive)
                {
                    throw new InvalidOperationException(
                        "There is pending write operation, multiple write operations not allowed !");
                }
                m_IsWriteActive = true;
            }

            m_pBeginWriteAll_Buffer = new byte[Workaround.Definitions.MaxStreamLineLength];
            m_pBeginWriteAll_Stream = stream;
            m_BeginWriteAll_MaxSize = maxSize;
            m_pBeginWriteAll_Callback = callback;
            m_BeginWriteAll_Readed = 0;

            // Start reading data block.
            m_pBeginWriteAll_Stream.BeginRead(m_pBeginWriteAll_Buffer,
                                              0,
                                              m_pBeginWriteAll_Buffer.Length,
                                              InternalBeginWriteAllCallback,
                                              null);
        }

        /// <summary>
        /// Writes all stream data to source stream.
        /// </summary>
        /// <param name="stream">Stream which data to write.</param>
        /// <returns>Returns number of bytes written to source stream.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>stream</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        public int WriteAll(Stream stream)
        {
            return WriteAll(stream, int.MaxValue);
        }

        /// <summary>
        /// Writes all stream data to source stream.
        /// </summary>
        /// <param name="stream">Stream which data to write.</param>
        /// <param name="maxSize">Maximum muber of bytes to read from <b>stream</b> and write source stream.</param>
        /// <returns>Returns number of bytes written to source stream.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>stream</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        /// <exception cref="DataSizeExceededException">Raised when <b>stream</b> stream has more data than specified by <b>maxSize</b>.</exception>
        public int WriteAll(Stream stream, int maxSize)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            lock (this)
            {
                if (m_IsWriteActive)
                {
                    throw new InvalidOperationException(
                        "There is pending write operation, multiple write operations not allowed !");
                }
                m_IsWriteActive = true;
            }

            try
            {
                byte[] buffer = new byte[Workaround.Definitions.MaxStreamLineLength];
                int totalReadedCount = 0;
                int readedCount = 0;
                while (readedCount > 0)
                {
                    // Read data block
                    readedCount = stream.Read(buffer, 0, buffer.Length);

                    // We reached end of stream, no more data.
                    if (readedCount == 0)
                    {
                        break;
                    }
                        // We have exceeded maximum allowed data size.
                    else if ((totalReadedCount + readedCount) > maxSize)
                    {
                        throw new DataSizeExceededException();
                    }
                    totalReadedCount += readedCount;

                    // Write readed data to source stream.
                    m_pStream.Write(buffer, 0, readedCount);
                }

                // Log
                if (Logger != null)
                {
                    Logger.AddWrite(totalReadedCount, null);
                }

                return totalReadedCount;
            }
            finally
            {
                m_IsWriteActive = false;
            }
        }

        /// <summary>
        /// Starts writing <b>stream</b> data to source stream. Data will be period handled and terminated as needed.
        /// </summary>
        /// <param name="stream">Stream which data to write to source stream.</param>
        /// <param name="maxSize">Maximum muber of bytes to read from <b>stream</b> and write source stream.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">Callback to be called if asynchronous write completes.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>stream</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        public void BeginWritePeriodTerminated(Stream stream,
                                               int maxSize,
                                               object tag,
                                               WriteStreamCallback callback)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            lock (this)
            {
                if (m_IsWriteActive)
                {
                    throw new InvalidOperationException(
                        "There is pending write operation, multiple write operations not allowed !");
                }
                m_IsWriteActive = true;
            }

            m_pBeginWritePeriodTerminated_Stream = stream;
            m_BeginWritePeriodTerminated_MaxSize = maxSize;
            m_pBeginWritePeriodTerminated_Callback = callback;
            m_pBeginWritePeriodTerminated_BufferedStream = new BufferedStream(m_pStream);
        }

        /// <summary>
        /// Reades all data from the specified stream and writes it to source stream. Period handlign and period terminator is added as required.
        /// </summary>
        /// <param name="stream">Stream which data to write to source stream.</param>
        /// <returns>Returns number of bytes written to source stream. Note this value differs from 
        /// <b>stream</b> readed bytes count because of period handling and period terminator.
        /// </returns>
        /// <exception cref="ArgumentNullException">Raised when <b>stream</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        /// <exception cref="LineSizeExceededException">Raised when <b>stream</b> contains line with bigger line size than allowed.</exception>
        /// <exception cref="DataSizeExceededException">Raised when <b>stream</b> has more data than <b>maxSize</b> allows..</exception>
        public int WritePeriodTerminated(Stream stream)
        {
            return WritePeriodTerminated(stream, int.MaxValue);
        }

        /// <summary>
        /// Reades all data from the specified stream and writes it to source stream. Period handlign and period terminator is added as required.
        /// </summary>
        /// <param name="stream">Stream which data to write to source stream.</param>
        /// <param name="maxSize">Maximum muber of bytes to read from <b>stream</b> and write source stream.</param>
        /// <returns>Returns number of bytes written to source stream. Note this value differs from 
        /// <b>stream</b> readed bytes count because of period handling and period terminator.
        /// </returns>
        /// <exception cref="ArgumentNullException">Raised when <b>stream</b> is null.</exception>
        /// <exception cref="InvalidOperationException">Raised when there already is pending write operation.</exception>
        /// <exception cref="LineSizeExceededException">Raised when <b>stream</b> contains line with bigger line size than allowed.</exception>
        /// <exception cref="DataSizeExceededException">Raised when <b>stream</b> has more data than <b>maxSize</b> allows..</exception>
        public int WritePeriodTerminated(Stream stream, int maxSize)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            lock (this)
            {
                if (m_IsWriteActive)
                {
                    throw new InvalidOperationException(
                        "There is pending write operation, multiple write operations not allowed !");
                }
                m_IsWriteActive = true;
            }

            try
            {
                BufferedStream bufferedStoreStream = new BufferedStream(m_pStream, Workaround.Definitions.MaxStreamLineLength);
                StreamHelper reader = new StreamHelper(stream);
                int totalWrittenCount = 0;
                int readedCount = 0;
                int rawReadedCount = 0;
                while (true)
                {
                    // Read data block.
                    readedCount = ReadLineInternal(m_pLineBuffer,
                                                   SizeExceededAction.ThrowException,
                                                   out rawReadedCount,
                                                   false);

                    // We reached end of stream, no more data.
                    if (readedCount == 0)
                    {
                        break;
                    }

                    // Maximum allowed data size exceeded.
                    if ((totalWrittenCount + rawReadedCount) > maxSize)
                    {
                        throw new DataSizeExceededException();
                    }

                    // If line starts with period(.), additional period is added.
                    if (m_pLineBuffer[0] == '.')
                    {
                        bufferedStoreStream.WriteByte((byte) '.');
                        totalWrittenCount++;
                    }

                    // Write readed line to buffered stream.
                    bufferedStoreStream.Write(m_pLineBuffer, 0, readedCount);
                    bufferedStoreStream.Write(m_LineBreak, 0, m_LineBreak.Length);
                    totalWrittenCount += (readedCount + m_LineBreak.Length);
                }

                // Write terminator ".<CRLF>". We have start <CRLF> already in stream.
                bufferedStoreStream.Write(new[] {(byte) '.', (byte) '\r', (byte) '\n'}, 0, 3);
                bufferedStoreStream.Flush();
                m_pStream.Flush();

                // Log
                if (Logger != null)
                {
                    Logger.AddWrite(totalWrittenCount, null);
                }

                return totalWrittenCount;
            }
            finally
            {
                m_IsWriteActive = false;
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Starts reading line from source stream. This method does not do any checks and read locks.
        /// </summary>
        /// <param name="buffer">Buffer where to store line data.</param>
        /// <param name="exceededAction">Specifies how this method behaves when maximum line size exceeded.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">Callback to be called whan asynchronous operation completes.</param>
        /// <param name="unlockRead">Specifies if read lock is released.</param>
        /// <param name="log">User data.</param>
        private void BeginReadLineInternal(byte[] buffer,
                                           SizeExceededAction exceededAction,
                                           object tag,
                                           ReadLineCallback callback,
                                           bool unlockRead,
                                           bool log)
        {
            m_pRLine_LineBuffer = buffer;
            m_RLine_ExceedAction = exceededAction;
            m_pRLine_Tag = tag;
            m_pRLine_Callback = callback;
            m_RLine_LineBufferOffset = 0;
            m_RLine_TotalReadedCount = 0;
            m_RLine_LineBufferSize = buffer.Length;
            m_RLine_UnlockRead = unlockRead;
            m_RLine_Log = log;

            if (IsReadBuffered)
            {
                DoReadLine_Buffered();
            }
            else
            {
                m_pStream.BeginRead(m_pRLine_ByteBuffer, 0, 1, OnReadByte_Completed, null);
            }
        }

        /// <summary>
        /// Is called when BeginWrite(byte[] data,int offset,int count) has completed.
        /// </summary>
        /// <param name="result"></param>
        private void InternalBeginWriteCallback(IAsyncResult result)
        {
            int count = (int) ((object[]) result.AsyncState)[0];
            WriteCallback callback = (WriteCallback) ((object[]) result.AsyncState)[1];

            try
            {
                m_pStream.EndWrite(result);

                // Log
                if (Logger != null)
                {
                    Logger.AddWrite(count, null);
                }

                if (callback != null)
                {
                    callback(new Write_EventArgs(null));
                }
            }
            catch (Exception x)
            {
                if (callback != null)
                {
                    callback(new Write_EventArgs(x));
                }
            }
            finally
            {
                m_IsWriteActive = false;
            }
        }

        /// <summary>
        /// This method is called when BeginWrite has readed new data block.
        /// </summary>
        /// <param name="result"></param>
        private void InternalBeginWriteStreamCallback(IAsyncResult result)
        {
            try
            {
                int readedCount = m_pBeginWrite_Stream.EndRead(result);

                // We have reached end of stream, we din't get so much data as requested.
                if (readedCount == 0)
                {
                    InternalBeginWriteStreamCompleted(
                        new IncompleteDataException("Read stream didn't have so much data than requested !"));
                }
                else
                {
                    m_BeginWrite_Readed += readedCount;

                    // Write readed bytes to source stream.
                    m_pStream.Write(m_pBeginWrite_Buffer, 0, readedCount);

                    // We have readed all requested data.
                    if (m_BeginWrite_Count == m_BeginWrite_Readed)
                    {
                        InternalBeginWriteStreamCompleted(null);
                    }
                        // We need read more data.
                    else
                    {
                        // Start reading data block.
                        m_pBeginWrite_Stream.BeginRead(m_pBeginWrite_Buffer,
                                                       0,
                                                       Math.Min(m_pBeginWrite_Buffer.Length,
                                                                m_BeginWrite_Count - m_BeginWrite_Readed),
                                                       InternalBeginWriteStreamCallback,
                                                       null);
                    }
                }
            }
            catch (Exception x)
            {
                InternalBeginWriteStreamCompleted(x);
            }
        }

        /// <summary>
        /// Is called when BeginWrite has completed.
        /// </summary>
        /// <param name="exception">Exception happened during write or null if operation was successfull.</param>
        private void InternalBeginWriteStreamCompleted(Exception exception)
        {
            // Release write lock.
            m_IsWriteActive = false;

            // Log
            if (Logger != null)
            {
                Logger.AddWrite(m_BeginWrite_Readed, null);
            }

            if (m_pBeginWrite_Callback != null)
            {
                m_pBeginWrite_Callback(new WriteStream_EventArgs(exception,
                                                                 m_pBeginWrite_Stream,
                                                                 m_BeginWrite_Readed,
                                                                 m_BeginWrite_Readed));
            }
        }

        /// <summary>
        /// This method is called when BeginWriteAll has readed new data block.
        /// </summary>
        /// <param name="result"></param>
        private void InternalBeginWriteAllCallback(IAsyncResult result)
        {
            try
            {
                int readedCount = m_pBeginWriteAll_Stream.EndRead(result);

                // We have reached end of stream, no more data.
                if (readedCount == 0)
                {
                    InternalBeginWriteAllCompleted(null);
                }
                else
                {
                    m_BeginWriteAll_Readed += readedCount;

                    // Maximum allowed data size exceeded.
                    if (m_BeginWriteAll_Readed > m_BeginWriteAll_MaxSize)
                    {
                        throw new DataSizeExceededException();
                    }

                    // Write readed bytes to source stream.
                    m_pStream.Write(m_pBeginWriteAll_Buffer, 0, readedCount);

                    // Start reading next data block.
                    m_pBeginWriteAll_Stream.BeginRead(m_pBeginWriteAll_Buffer,
                                                      0,
                                                      m_pBeginWriteAll_Buffer.Length,
                                                      InternalBeginWriteAllCallback,
                                                      null);
                    return;
                }
            }
            catch (Exception x)
            {
                InternalBeginWriteAllCompleted(x);
            }
        }

        /// <summary>
        /// Is called when BeginWriteAll has completed.
        /// </summary>
        /// <param name="exception">Exception happened during write or null if operation was successfull.</param>
        private void InternalBeginWriteAllCompleted(Exception exception)
        {
            // Release write lock.
            m_IsWriteActive = false;

            // Log
            if (Logger != null)
            {
                Logger.AddWrite(m_BeginWriteAll_Readed, null);
            }

            if (m_pBeginWriteAll_Callback != null)
            {
                m_pBeginWriteAll_Callback(new WriteStream_EventArgs(exception,
                                                                    m_pBeginWriteAll_Stream,
                                                                    m_BeginWriteAll_Readed,
                                                                    m_BeginWriteAll_Readed));
            }
        }

        /// <summary>
        /// Is called when BeginWritePeriodTerminated stream.BeginReadLine has completed.
        /// </summary>
        /// <param name="e">Callback data.</param>
        private void InternalBeginWritePeriodTerminatedReadLineCompleted(ReadLine_EventArgs e)
        {
            try
            {
                // Error happened during read.
                if (e.Exception != null)
                {
                    InternalBeginWritePeriodTerminatedCompleted(e.Exception);
                }
                    // We reached end of stream, no more data.
                else if (e.ReadedCount == 0)
                {
                    InternalBeginWritePeriodTerminatedCompleted(null);
                }
                else
                {
                    m_BeginWritePeriodTerminated_Readed += e.ReadedCount;

                    // Maximum allowed data size exceeded.
                    if (m_BeginWritePeriodTerminated_Readed > m_BeginWritePeriodTerminated_MaxSize)
                    {
                        throw new DataSizeExceededException();
                    }

                    // If line starts with period, addtional period is added.
                    if (e.LineBuffer[0] == '.')
                    {
                        m_pBeginWritePeriodTerminated_BufferedStream.WriteByte((int) '.');
                        m_BeginWritePeriodTerminated_Written++;
                    }

                    // Write readed line to buffered stream.
                    m_pBeginWritePeriodTerminated_BufferedStream.Write(e.LineBuffer, 0, e.Count);
                    m_pBeginWritePeriodTerminated_BufferedStream.Write(m_LineBreak, 0, m_LineBreak.Length);
                    m_BeginWritePeriodTerminated_Written += e.Count + m_LineBreak.Length;
                }
            }
            catch (Exception x)
            {
                InternalBeginWritePeriodTerminatedCompleted(x);
            }
        }

        /// <summary>
        /// Is called when asynchronous write period terminated has completed.
        /// </summary>
        /// <param name="exception">Exception happened during write or null if operation was successfull.</param>
        private void InternalBeginWritePeriodTerminatedCompleted(Exception exception)
        {
            // Release write lock.
            m_IsWriteActive = false;

            // Force to write buffer to source stream, if any.
            try
            {
                m_pBeginWritePeriodTerminated_BufferedStream.Flush();
            }
            catch {}

            // Log
            if (Logger != null)
            {
                Logger.AddWrite(m_BeginWritePeriodTerminated_Written, null);
            }

            if (m_pBeginWritePeriodTerminated_Callback != null)
            {
                m_pBeginWritePeriodTerminated_Callback(new WriteStream_EventArgs(exception,
                                                                                 m_pBeginWritePeriodTerminated_Stream,
                                                                                 m_BeginWritePeriodTerminated_Readed,
                                                                                 m_BeginWritePeriodTerminated_Written));
            }
        }

        /// <summary>
        /// Is called when asynchronous read byte operation has completed.
        /// </summary>
        /// <param name="result"></param>
        private void OnReadByte_Completed(IAsyncResult result)
        {
            try
            {
                int readedCount = m_pStream.EndRead(result);
                if (readedCount == 1)
                {
                    m_RLine_TotalReadedCount++;

                    // We have LF.
                    if (m_pRLine_ByteBuffer[0] == '\n')
                    {
                        // Line size eceeded and some data junked.
                        if (m_RLine_LineBufferOffset > m_RLine_LineBufferSize)
                        {
                            OnReadLineCompleted(new LineSizeExceededException());
                        }
                            // Read line completed sucessfully.
                        else
                        {
                            OnReadLineCompleted(null);
                        }
                        return;
                    }
                        // We just skip all CR.
                    else if (m_pRLine_ByteBuffer[0] == '\r') {}
                    else
                    {
                        // Maximum allowed line size exceeded.
                        if (m_RLine_LineBufferOffset >= m_RLine_LineBufferSize)
                        {
                            if (m_RLine_ExceedAction == SizeExceededAction.ThrowException)
                            {
                                OnReadLineCompleted(new LineSizeExceededException());
                                return;
                            }
                        }
                            // Write readed byte to line buffer.
                        else
                        {
                            m_pRLine_LineBuffer[m_RLine_LineBufferOffset] = m_pRLine_ByteBuffer[0];
                            m_RLine_LineBufferOffset++;
                        }
                    }

                    // Get next byte.
                    m_pStream.BeginRead(m_pRLine_ByteBuffer, 0, 1, OnReadByte_Completed, null);
                }
                    // We have no more data.
                else
                {
                    // Line size eceeded and some data junked.
                    if (m_RLine_LineBufferOffset >= m_RLine_LineBufferSize)
                    {
                        OnReadLineCompleted(new LineSizeExceededException());
                    }
                        // Read line completed sucessfully.
                    else
                    {
                        OnReadLineCompleted(null);
                    }
                }
            }
            catch (Exception x)
            {
                OnReadLineCompleted(x);
            }
        }

        /// <summary>
        /// Tries to read line from data buffer, if no line in buffer, new data buffer will be readed(buffered).
        /// </summary>
        private void DoReadLine_Buffered()
        {
            try
            {
                byte currentByte = 0;
                while (m_ReadBufferOffset < m_ReadBufferEndPos)
                {
                    currentByte = m_pReadBuffer[m_ReadBufferOffset];
                    m_ReadBufferOffset++;
                    m_RLine_TotalReadedCount++;

                    // We have LF.
                    if (currentByte == '\n')
                    {
                        // Line size eceeded and some data junked.
                        if (m_RLine_TotalReadedCount > m_RLine_LineBufferSize)
                        {
                            OnReadLineCompleted(new LineSizeExceededException());
                        }
                            // Read line completed sucessfully.
                        else
                        {
                            OnReadLineCompleted(null);
                        }
                        return;
                    }
                        // We just skip all CR.
                    else if (currentByte == '\r') {}
                    else
                    {
                        // Maximum allowed line size exceeded.
                        if (m_RLine_TotalReadedCount >= m_RLine_LineBufferSize)
                        {
                            if (m_RLine_ExceedAction == SizeExceededAction.ThrowException)
                            {
                                OnReadLineCompleted(new LineSizeExceededException());
                                return;
                            }
                        }
                            // Write readed byte to line buffer.
                        else
                        {
                            m_pRLine_LineBuffer[m_RLine_LineBufferOffset] = currentByte;
                            m_RLine_LineBufferOffset++;
                        }
                    }
                }

                // If we reach here, that means we consumed all read buffer and no line was in it.
                // Just get new data buffer block.
                m_pStream.BeginRead(m_pReadBuffer, 0, m_pReadBuffer.Length, OnReadBuffer_Completed, null);
            }
            catch (Exception x)
            {
                OnReadLineCompleted(x);
            }
        }

        /// <summary>
        /// Is called when asynchronous data buffering has completed.
        /// </summary>
        /// <param name="result"></param>
        private void OnReadBuffer_Completed(IAsyncResult result)
        {
            try
            {
                m_ReadBufferEndPos = m_pStream.EndRead(result);
                m_ReadBufferOffset = 0;

                // We have reached end of stream, no more data.
                if (m_ReadBufferEndPos == 0)
                {
                    // Line size eceeded and some data junked.
                    if (m_RLine_TotalReadedCount > m_RLine_LineBufferSize)
                    {
                        OnReadLineCompleted(new LineSizeExceededException());
                    }
                        // Read line completed sucessfully.
                    else
                    {
                        OnReadLineCompleted(null);
                    }
                }
                    // Continue line reading.
                else
                {
                    DoReadLine_Buffered();
                }
            }
            catch (Exception x)
            {
                OnReadLineCompleted(x);
            }
        }

        /// <summary>
        /// Is called when read line has completed.
        /// </summary>
        /// <param name="x">Excheption what happened during line reading or null if read line was completed sucessfully.</param>
        private void OnReadLineCompleted(Exception x)
        {
            if (m_RLine_UnlockRead)
            {
                // Release reader lock.
                m_IsReadActive = false;
            }

            if (m_RLine_Log && Logger != null)
            {
                Logger.AddRead(m_RLine_TotalReadedCount, null);
            }

            if (m_pRLine_Callback != null)
            {
                m_pRLine_EventArgs.Reuse(x,
                                         m_RLine_TotalReadedCount,
                                         m_pRLine_LineBuffer,
                                         m_RLine_LineBufferOffset,
                                         m_pRLine_Tag);
                m_pRLine_Callback(m_pRLine_EventArgs);
            }
        }

        #endregion
    }
}