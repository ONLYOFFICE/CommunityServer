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


namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;
    using System.IO;
    using System.Text;
    using IO;

    #endregion

    /// <summary>
    /// This class is base class for MIME entity bodies.
    /// </summary>
    public abstract class MIME_b
    {
        #region Members

        private readonly MIME_h_ContentType m_pContentType;
        private MIME_Entity m_pEntity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if body has modified.
        /// </summary>
        public abstract bool IsModified { get; }

        /// <summary>
        /// Gets body owner entity. Returns null if body not bounded to any entity yet.
        /// </summary>
        public MIME_Entity Entity
        {
            get { return m_pEntity; }
        }

        /// <summary>
        /// Gets MIME entity body content type.
        /// </summary>
        public MIME_h_ContentType ContentType
        {
            get { return m_pContentType; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="contentType">Content type.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>contentType</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public MIME_b(MIME_h_ContentType contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }

            m_pContentType = contentType;
        }

        #endregion

        #region Abstract methods

        /// <summary>
        /// Stores MIME entity body to the specified stream.
        /// </summary>
        /// <param name="stream">Stream where to store body data.</param>
        /// <param name="headerWordEncoder">Header 8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="headerParmetersCharset">Charset to use to encode 8-bit header parameters. Value null means parameters not encoded.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        protected internal abstract void ToStream(Stream stream,
                                                  MIME_Encoding_EncodedWord headerWordEncoder,
                                                  Encoding headerParmetersCharset);

        #endregion

        #region Internal methods

        /// <summary>
        /// Sets body parent.
        /// </summary>
        /// <param name="entity">Owner entity.</param>
        internal void SetParent(MIME_Entity entity)
        {
            m_pEntity = entity;
        }

        #endregion

        /// <summary>
        /// Parses body from the specified stream
        /// </summary>
        /// <param name="owner">Owner MIME entity.</param>
        /// <param name="mediaType">MIME media type. For example: text/plain.</param>
        /// <param name="stream">Stream from where to read body.</param>
        /// <returns>Returns parsed body.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b>, <b>mediaType</b> or <b>stream</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when any parsing errors.</exception>
        protected static MIME_b Parse(MIME_Entity owner, string mediaType, SmartStream stream)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            throw new NotImplementedException("Body provider class does not implement required Parse method.");
        }
    }
}