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
    using IO;

    #endregion

    /// <summary>
    /// This class represents MIME video/xxx bodies. Defined in RFC 2046 4.4.
    /// </summary>
    /// <remarks>
    /// A media type of "video" indicates that the body contains a time-
    /// varying-picture image, possibly with color and coordinated sound.
    /// </remarks>
    public class MIME_b_Video : MIME_b_SinglepartBase
    {
        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="mediaType">MIME media type.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>mediaType</b> is null reference.</exception>
        public MIME_b_Video(string mediaType) : base(mediaType) {}

        #endregion

        /// <summary>
        /// Parses body from the specified stream
        /// </summary>
        /// <param name="owner">Owner MIME entity.</param>
        /// <param name="mediaType">MIME media type. For example: text/plain.</param>
        /// <param name="stream">Stream from where to read body.</param>
        /// <returns>Returns parsed body.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b>, <b>mediaType</b> or <b>strean</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when any parsing errors.</exception>
        protected new static MIME_b Parse(MIME_Entity owner, string mediaType, SmartStream stream)
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

            MIME_b_Video retVal = new MIME_b_Video(mediaType);

            Net_Utils.StreamCopy(stream, retVal.EncodedStream, Workaround.Definitions.MaxStreamLineLength);

            return retVal;
        }
    }
}