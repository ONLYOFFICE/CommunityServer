/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Net.MIME
{
    /// <summary>
    /// This class holds well known Content-Type header field media types. For example: text/plain, application/octet-stream.
    /// Full IANA registered list can be found from: http://www.iana.org/assignments/media-types.
    /// </summary>
    public class MIME_MediaTypes
    {
        #region Nested type: Application

        /// <summary>
        /// This class holds well-known application/xxx media types.
        /// </summary>
        public class Application
        {
            #region Members

            /// <summary>
            /// "application/octet-stream". Defined in RFC 2045,2046.
            /// </summary>
            public static readonly string octet_stream = "application/octet-stream";

            /// <summary>
            /// "application/pdf". Defined in RFC 3778.
            /// </summary>
            public static readonly string pdf = "application/pdf";

            /// <summary>
            /// "application/sdp". Defined in RFC 4566.
            /// </summary>
            public static readonly string sdp = "application/sdp";

            /// <summary>
            /// "application/xml". Defined RFC 3023.
            /// </summary>
            public static readonly string xml = "application/xml";

            /// <summary>
            /// "application/zip". Defined in RFC 4566.
            /// </summary>
            public static readonly string zip = "application/zip";

            #endregion
        }

        #endregion

        #region Nested type: Image

        /// <summary>
        /// This class holds well-known image/xxx media types.
        /// </summary>
        public class Image
        {
            #region Members

            /// <summary>
            /// "image/gif".
            /// </summary>
            public static readonly string gif = "image/gif";

            /// <summary>
            /// "image/jpeg".
            /// </summary>
            public static readonly string jpeg = "image/jpeg";

            /// <summary>
            /// "image/tiff".
            /// </summary>
            public static readonly string tiff = "image/tiff";

            #endregion
        }

        #endregion

        #region Nested type: Message

        /// <summary>
        /// This class holds well-known message/xxx media types.
        /// </summary>
        public class Message
        {
            #region Members

            /// <summary>
            /// "message/disposition-notification". 
            /// </summary>
            public static readonly string disposition_notification = "message/disposition-notification";

            /// <summary>
            /// "message/rfc822". 
            /// </summary>
            public static readonly string rfc822 = "message/rfc822";

            #endregion
        }

        #endregion

        #region Nested type: Multipart

        /// <summary>
        /// This class holds well-known multipart/xxx media types.
        /// </summary>
        public class Multipart
        {
            #region Members

            /// <summary>
            /// "multipart/alternative". Defined in RFC 2045,2046.
            /// </summary>
            public static readonly string alternative = "multipart/alternative";

            /// <summary>
            /// "multipart/digest". Defined in RFC 2045,2046.
            /// </summary>
            public static readonly string digest = "multipart/digest";

            /// <summary>
            /// "multipart/digest". Defined in RFC 1847.
            /// </summary>
            public static readonly string encrypted = "multipart/digest";

            /// <summary>
            /// "multipart/form-data". Defined in RFC 2388.
            /// </summary>
            public static readonly string form_data = "multipart/form-data";

            /// <summary>
            /// "multipart/mixed". Defined in RFC 2045,2046.
            /// </summary>
            public static readonly string mixed = "multipart/mixed";

            /// <summary>
            /// "multipart/parallel". Defined in RFC 2045,2046.
            /// </summary>
            public static readonly string parallel = "multipart/parallel";

            /// <summary>
            /// "multipart/related". Defined in RFC 2387.
            /// </summary>
            public static readonly string related = "multipart/related";

            /// <summary>
            /// "multipart/report". Defined in RFC 1892.
            /// </summary>
            public static readonly string report = "multipart/report";

            /// <summary>
            /// "multipart/signed". Defined in RFC 1847.
            /// </summary>
            public static readonly string signed = "multipart/signed";

            /// <summary>
            /// "multipart/voice-message". Defined in RFC 2421,2423.
            /// </summary>
            public static readonly string voice_message = "multipart/voice-message";

            #endregion
        }

        #endregion

        #region Nested type: Text

        /// <summary>
        /// This class holds well-known text/xxx media types.
        /// </summary>
        public class Text
        {
            #region Members

            /// <summary>
            /// "text/calendar". Defined in RFC 2445.
            /// </summary>
            public static readonly string calendar = "text/calendar";

            /// <summary>
            /// "text/css". Defined in RFC 2854
            /// </summary>
            public static readonly string css = "text/css";

            /// <summary>
            /// "text/html". Defined in RFC 2854.
            /// </summary>
            public static readonly string html = "text/html";

            /// <summary>
            /// "text/plain". Defined in RFC 2646,2046.
            /// </summary>
            public static readonly string plain = "text/plain";

            /// <summary>
            /// "text/rfc822-headers". Defined in RFC 1892.
            /// </summary>
            public static readonly string rfc822_headers = "text/rfc822-headers";

            /// <summary>
            /// "text/richtext". Defined in RFC 2045,2046.
            /// </summary>
            public static readonly string richtext = "text/richtext";

            /// <summary>
            /// "text/xml". Defined in RFC 3023.
            /// </summary>
            public static readonly string xml = "text/xml";

            #endregion
        }

        #endregion
    }
}