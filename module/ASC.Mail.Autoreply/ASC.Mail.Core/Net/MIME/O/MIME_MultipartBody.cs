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
using System.IO;
using System.Collections.Generic;
using System.Text;

using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME
{
    /// <summary>
    /// Represents MIME entity multipart/... body. Defined in RFC 2045.
    /// </summary>
    public class MIME_MultipartBody : MIME_Body
    {
        #region class _MIME_MultipartReader

        /// <summary>
        /// This class implements mulitpart/xxx body parts reader.
        /// </summary>
        internal class _MIME_MultipartReader : LineReader
        {
            #region enum State

            /// <summary>
            /// Specifies reader state.
            /// </summary>
            private enum State
            {            
                /// <summary>
                /// Body reading pending, the whole body isn't readed yet.
                /// </summary>
                InBody,

                /// <summary>
                /// First "body part" start must be searched.
                /// </summary>
                SeekFirst,

                /// <summary>
                /// Multipart "body part" reading has completed, next "body part" reading is pending.
                /// </summary>
                NextWaited,

                /// <summary>
                /// All "body parts" readed.
                /// </summary>
                Finished,
            }

            #endregion

            private LineReader m_pReader  = null;
            private string     m_Boundary = "";
            private State      m_State    = State.SeekFirst;

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="reader">Line reader.</param>
            /// <param name="boundary">Boundary ID.</param>
            public _MIME_MultipartReader(LineReader reader,string boundary) : base(reader.Stream,false,32000)
            {
                m_pReader  = reader;
                m_Boundary = boundary;

                if(reader.CanSyncStream){
                    reader.SyncStream();
                }
            }


            #region method Next

            /// <summary>
            /// Moves to next "body part". Returns true if moved to next "body part" or false if there are no more parts.
            /// </summary>
            /// <returns>Returns true if moved to next "body part" or false if there are no more parts.</returns>
            public bool Next()
            {
                // Seek first.
                if(m_State == State.SeekFirst){
                    while(true){
                        string line = m_pReader.ReadLine();
                        // We reached end of stream, no more data.
                        if(line == null){
                            m_State = State.Finished;

                            return false;
                        }
                        else if(line == "--" + m_Boundary){
                            m_State = State.InBody;

                            return true;
                        }
                    }
                }
                else if(m_State != State.NextWaited){
                    return false;
                }
                else{
                    m_State = State.InBody;

                    return true;
                }
            }

            #endregion


            #region method override ReadLine

            /// <summary>
            /// Reads binary line and stores it to the specified buffer.
            /// </summary>
            /// <param name="buffer">Buffer where to store line data.</param>
            /// <param name="offset">Start offset in the buffer.</param>
            /// <param name="count">Maximum number of bytes store to the buffer.</param>
            /// <param name="exceededAction">Specifies how reader acts when line buffer too small.</param>
            /// <param name="rawBytesReaded">Gets raw number of bytes readed from source.</param>
            /// <returns>Returns number of bytes stored to <b>buffer</b> or -1 if end of stream reached.</returns>
            /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
            /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
            /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
            /// <exception cref="LineSizeExceededException">Is raised when line is bigger than <b>buffer</b> can store.</exception>
            public override int ReadLine(byte[] buffer,int offset,int count,SizeExceededAction exceededAction,out int rawBytesReaded)
            {
                rawBytesReaded = 0;

                // We are at the end of body or "body part".
                if(m_State == State.Finished || m_State == State.NextWaited){
                    return -1;
                }
                // Read next line.
                else{
                    int readedCount = m_pReader.ReadLine(buffer,offset,count,exceededAction,out rawBytesReaded);
                    // End of stream reached, no more data.
                    if(readedCount == -1){
                        m_State = State.Finished;

                        return -1;
                    }
                    // For multipart we must check boundary tags.
                    else if(!string.IsNullOrEmpty(m_Boundary) && readedCount > 2 && buffer[0] == '-'){
                        string line = Encoding.Default.GetString(buffer,0,readedCount);

                        // Boundray end-tag reached, no more "body parts".
                        if(line == "--" + m_Boundary + "--"){
                            m_State = State.Finished;

                            return -1;
                        }
                        // Boundary start-tag reached, wait for this.Next() call.
                        else if(line == "--" + m_Boundary){
                            m_State = State.NextWaited;

                            return -1;
                        }
                    }

                    return readedCount;
                }
            }

            #endregion

            #region method SyncStream

            /// <summary>
            /// Sets stream position to the place we have consumed from stream and clears buffer data.
            /// For example if we have 10 byets in buffer, stream position is actually +10 bigger than 
            /// we readed, the result is that stream.Position -= 10 and buffer is cleared.
            /// </summary>
            /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
            /// <exception cref="InvalidOperationException">Is raised when source stream won't support seeking.</exception>
            public override void SyncStream()
            {
                m_pReader.SyncStream();
            }

            #endregion


            #region Properties Implementation

            #endregion
        }

        #endregion

        private string                m_Boundary = "";
        private MIME_EntityCollection m_pParts   = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="owner">Owner MIME entity.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>owner</b> is null.</exception>
        internal MIME_MultipartBody(MIME_Entity owner) : base(owner)
        {
            m_Boundary = owner.ContentType.Param_Boundary;

            m_pParts = new MIME_EntityCollection();
        }
                        
        
        #region override method ToStream

        /// <summary>
        /// Stores MIME entity body data to the specified stream
        /// </summary>
        /// <param name="stream">Stream where to store body. Storing starts from stream current position.</param> 
        /// <param name="encoded">If true, encoded data is stored, if false, decoded data will be stored.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
        public override void ToStream(Stream stream,bool encoded)
        {
        }

        #endregion


        #region override method ParseFromReader

        /// <summary>
        /// Parses MIME entity body from the specified reader.
        /// </summary>
        /// <param name="reader">Body reader from where to parse body.</param>
        /// <param name="owner">Specifies if body will be stream owner.</param>
        /// <returns>Returns true if this is last boundary in the message or in multipart "body parts".</returns>
        internal override void ParseFromReader(LineReader reader,bool owner)
        {
            // For multipart we need todo new limiting(limits to specified boundary) reader.
            _MIME_MultipartReader r = new _MIME_MultipartReader(reader,m_Boundary);
            while(r.Next()){
                MIME_Entity bodyPart = new MIME_Entity();                
                bodyPart.Parse(r,owner);
                m_pParts.Add(bodyPart);                
            }
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets boundary ID.
        /// </summary>
        public string Boundary
        {
            get{ return m_Boundary; }
        }

        /// <summary>
        /// Gets body parts collection.
        /// </summary>
        public MIME_EntityCollection BodyParts
        {
            get{ return m_pParts; }
        }

        #endregion

    }
}
