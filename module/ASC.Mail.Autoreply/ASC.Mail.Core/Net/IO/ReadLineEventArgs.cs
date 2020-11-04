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

namespace LumiSoft.Net.IO
{
    /// <summary>
    /// This class provides data to <see cref="SmartStream.ReadLine">SmartStream.ReadLine</see> method.
    /// </summary>
    /// <remarks>This class can be reused on multiple calls of <see cref="SmartStream.ReadLine">SmartStream.ReadLine</see> method.</remarks>
    public class ReadLineEventArgs : EventArgs
    {
        private bool               m_IsDisposed     = false;
        private bool               m_IsCompleted    = false;
        private byte[]             m_pBuffer        = null;
        private SizeExceededAction m_ExceededAction = SizeExceededAction.JunkAndThrowException;
        private bool               m_CRLFLinesOnly  = false;
        private int                m_BytesInBuffer  = 0;
        private Exception          m_pException     = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="buffer">Line buffer.</param>
        /// <param name="exceededAction">Specifies how line-reader behaves when maximum line size exceeded.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null reference.</exception>
        public ReadLineEventArgs(byte[] buffer,SizeExceededAction exceededAction)
        {
            if(buffer == null){
                throw new ArgumentNullException("buffer");
            }

            m_pBuffer        = buffer;
            m_ExceededAction = exceededAction;
        }


        #region method Start 

        internal bool Start(SmartStream stream)
        {   
            // TODO: Clear old data, if any.
            m_IsCompleted   = false;
            m_BytesInBuffer = 0;
            m_pException    = null;
   
            return DoLineReading();
        }

        #endregion

        #region method Buffering_Completed

        /// <summary>
        /// Is called when asynchronous read buffer buffering has completed.
        /// </summary>
        /// <param name="x">Exception that occured during async operation.</param>
        private void Buffering_Completed(Exception x)
        {
            /*
            if(x != null){
                m_pException = x;
                Completed();
            }
            // We reached end of stream, no more data.
            else if(m_pOwner.BytesInReadBuffer == 0){
                Completed();
            }
            // Continue line reading.
            else{
                DoLineReading();
            }*/
        }

        #endregion

        #region method DoLineReading

        /// <summary>
        /// Starts/continues line reading.
        /// </summary>
        /// <returns>Returns true if line reading completed.</returns>
        private bool DoLineReading()
        {
            try{
                while(true){
                    /*
                    // Read buffer empty, buff next data block.
                    if(m_pOwner.BytesInReadBuffer == 0){
                        // Buffering started asynchronously.
                        if(m_pOwner.BufferRead(true,this.Buffering_Completed)){
                            return;
                        }
                        // Buffering completed synchronously, continue processing.
                        else{
                            // We reached end of stream, no more data.
                            if(m_pOwner.BytesInReadBuffer == 0){
                                Completed();
                                return;
                            }
                        }
                    }*/

                    byte b = 1; //m_pOwner.m_pReadBuffer[m_pOwner.m_ReadBufferOffset++];
                    //m_BytesInBuffer++;

                    // TODO: Check for room, Store byte.

                    // We have LF line.
                    if(b == '\n'){
                        // TODO:
                        // m_CRLFLinesOnly
                    }
                }
            }
            catch(Exception x){
                m_pException = x;
                OnCompleted();
            }

            return true;
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get{ return m_IsDisposed; }
        }

        /// <summary>
        /// Gets if asynchronous operation has completed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public bool IsCompleted
        {
            get{
                if(m_IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_IsCompleted; 
            }
        }

        /// <summary>
        /// Gets line buffer.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public byte[] Buffer
        {
            get{
                if(m_IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_pBuffer; 
            }
        }

        /// <summary>
        /// Gets number of bytes stored in the buffer. Line feed characters not included.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public int BytesInBuffer
        {
            get{
                if(m_IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_BytesInBuffer; 
            }
        }

        /// <summary>
        /// Gets line as ASCII string.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public string LineAscii
        {
            get{
                if(m_IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return Encoding.ASCII.GetString(m_pBuffer,0,m_BytesInBuffer); 
            }
        }

        /// <summary>
        /// Gets line as UTF-8 string.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public string LineUtf8
        {
            get{
                if(m_IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return Encoding.UTF8.GetString(m_pBuffer,0,m_BytesInBuffer);
            }
        }

        /// <summary>
        /// Gets error occured during asynchronous operation. Value null means no error.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public Exception Error
        {
            get{
                if(m_IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_pException; 
            }
        }

        #endregion

        #region Events implementation

        /// <summary>
        /// Is raised when asynchronous operation has completed.
        /// </summary>
        public event EventHandler<ReadLineEventArgs> Completed = null;

        #region method OnCompleted

        /// <summary>
        /// Raises <b>Completed</b> event.
        /// </summary>
        private void OnCompleted()
        {
            if(this.Completed != null){
                this.Completed(this,this);
            }
        }

        #endregion

        #endregion
    }
}
