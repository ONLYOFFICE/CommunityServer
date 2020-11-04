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
    using System.Collections.Generic;
    using System.Reflection;
    using IO;

    #endregion

    /// <summary>
    /// This class represent MIME entity body provider.
    /// </summary>
    public class MIME_b_Provider
    {
        #region Members

        private readonly Dictionary<string, Type> m_pBodyTypes;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MIME_b_Provider()
        {
            m_pBodyTypes = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase);
            m_pBodyTypes.Add("message/rfc822", typeof (MIME_b_MessageRfc822));
            m_pBodyTypes.Add("multipart/alternative", typeof (MIME_b_MultipartAlternative));
            m_pBodyTypes.Add("multipart/digest", typeof (MIME_b_MultipartDigest));
            m_pBodyTypes.Add("multipart/encrypted", typeof (MIME_b_MultipartEncrypted));
            m_pBodyTypes.Add("multipart/form-data", typeof (MIME_b_MultipartFormData));
            m_pBodyTypes.Add("multipart/mixed", typeof (MIME_b_MultipartMixed));
            m_pBodyTypes.Add("multipart/parallel", typeof (MIME_b_MultipartParallel));
            m_pBodyTypes.Add("multipart/related", typeof (MIME_b_MultipartRelated));
            m_pBodyTypes.Add("multipart/report", typeof (MIME_b_MultipartReport));
            m_pBodyTypes.Add("multipart/signed", typeof (MIME_b_MultipartSigned));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses MIME entity body from specified stream.
        /// </summary>
        /// <param name="owner">Owner MIME entity.</param>
        /// <param name="stream">Stream from where to parse entity body.</param>
        /// <param name="defaultContentType">Default content type.</param>
        /// <returns>Returns parsed body.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>owner</b>, <b>strean</b> or <b>defaultContentType</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when header field parsing errors.</exception>
        public MIME_b Parse(MIME_Entity owner, SmartStream stream, MIME_h_ContentType defaultContentType)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (defaultContentType == null)
            {
                throw new ArgumentNullException("defaultContentType");
            }

            string mediaType = defaultContentType.TypeWithSubype;
            string mediaTypeWithParams = defaultContentType.ToString().Split(':')[1].TrimStart(' ');
            if (owner.ContentType != null)
            {
                mediaType = owner.ContentType.TypeWithSubype;
                mediaTypeWithParams = owner.ContentType.ToString().Split(':')[1].TrimStart(' ');
            }

            Type bodyType = null;

            // We have exact body provider for specified mediaType.
            if (m_pBodyTypes.ContainsKey(mediaType))
            {
                bodyType = m_pBodyTypes[mediaType];
            }
                // Use default mediaType.
            else
            {
                // Registered list of mediaTypes are available: http://www.iana.org/assignments/media-types/.

                string mediaRootType = mediaType.Split('/')[0].ToLowerInvariant();
                if (mediaRootType == "application")
                {
                    bodyType = typeof (MIME_b_Application);
                }
                else if (mediaRootType == "audio")
                {
                    bodyType = typeof (MIME_b_Audio);
                }
                else if (mediaRootType == "image")
                {
                    bodyType = typeof (MIME_b_Image);
                }
                else if (mediaRootType == "message")
                {
                    bodyType = typeof (MIME_b_Message);
                }
                else if (mediaRootType == "multipart")
                {
                    bodyType = typeof (MIME_b_Multipart);
                }
                else if (mediaRootType == "text")
                {
                    bodyType = typeof (MIME_b_Text);
                }
                else if (mediaRootType == "video")
                {
                    bodyType = typeof (MIME_b_Video);
                }
                else
                {
                    throw new ParseException("Invalid media-type '" + mediaType + "'.");
                }
            }

            return
                (MIME_b)
                bodyType.GetMethod("Parse",
                                   BindingFlags.Static | BindingFlags.NonPublic |
                                   BindingFlags.FlattenHierarchy).Invoke(null,
                                                                         new object[] { owner, mediaTypeWithParams, stream });
        }

        #endregion
    }
}