/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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