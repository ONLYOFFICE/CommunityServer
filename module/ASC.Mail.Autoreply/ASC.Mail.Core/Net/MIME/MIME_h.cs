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
    using System.Text;

    #endregion

    /// <summary>
    /// This is base class for MIME header fields. Defined in RFC 2045 3.
    /// </summary>
    public abstract class MIME_h
    {
        #region Properties

        /// <summary>
        /// Gets if this header field is modified since it has loaded.
        /// </summary>
        /// <remarks>All new added header fields has <b>IsModified = true</b>.</remarks>
        /// <exception cref="ObjectDisposedException">Is riased when this class is disposed and this property is accessed.</exception>
        public abstract bool IsModified { get; }

        /// <summary>
        /// Gets header field name. For example "Content-Type".
        /// </summary>
        public abstract string Name { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns header field as string.
        /// </summary>
        /// <returns>Returns header field as string.</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>
        /// Returns header field as string.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="parmetersCharset">Charset to use to encode 8-bit characters. Value null means parameters not encoded.
        /// If encoding needed, UTF-8 is strongly reccomended if not sure.</param>
        /// <returns>Returns header field as string.</returns>
        public abstract string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset);

        #endregion
    }
}