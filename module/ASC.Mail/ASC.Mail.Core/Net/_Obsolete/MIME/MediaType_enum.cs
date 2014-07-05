/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

namespace ASC.Mail.Net.Mime
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Rfc 2046,2387 Media Types.
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public enum MediaType_enum
    {
        /// <summary>
        /// Text data.
        /// </summary>
        Text = 1,

        /// <summary>
        /// Simple text data. Defined in Rfc 1521.
        /// </summary>
        Text_plain = Text | (1 << 1),

        /// <summary>
        /// Html data. Defined in Rfc 2854.
        /// </summary>
        Text_html = Text | (1 << 2),

        /// <summary>
        /// Xml data. Defined in Rfc 3023 3.1.
        /// </summary>
        Text_xml = Text | (1 << 3),

        /// <summary>
        /// Rich text (RTF) data.
        /// </summary>
        Text_rtf = Text | (1 << 4),

        /// <summary>
        /// Image data.
        /// </summary>
        Image = 1 << 5,

        /// <summary>
        /// Gif image. Defined in Rfc 1521.
        /// </summary>
        Image_gif = Image | (1 << 6),

        /// <summary>
        /// Tiff image.
        /// </summary>
        Image_tiff = Image | (1 << 7),

        /// <summary>
        /// Jpeg image. Defined in Rfc 1521.
        /// </summary>
        Image_jpeg = Image | (1 << 8),

        /// <summary>
        /// Audio data.
        /// </summary>
        Audio = 1 << 8,

        /// <summary>
        /// Video data.
        /// </summary>
        Video = 1 << 10,

        /// <summary>
        /// Some other kind of data, typically either uninterpreted binary data or information to be processed by an application.
        /// </summary>
        Application = 1 << 11,

        /// <summary>
        /// The "octet-stream" subtype is used to indicate that a body contains	arbitrary binary data. Defined in Rfc 4046 4.5.1.
        /// </summary>
        Application_octet_stream = Application | (1 << 12),

        /// <summary>
        /// Data consisting of multiple entities of	independent data types.
        /// </summary>
        Multipart = 1 << 13,

        /// <summary>
        /// Data consisting of multiple entities of	independent data types.
        /// </summary>
        Multipart_mixed = Multipart | (1 << 14),

        /// <summary>
        /// Data consisting of multiple entities of	independent data types.
        /// </summary>
        Multipart_alternative = Multipart | (1 << 15),

        /// <summary>
        /// Data consisting of multiple entities of	independent data types.
        /// </summary>
        Multipart_parallel = Multipart | (1 << 16),

        /// <summary>
        /// Data consisting of multiple entities of	independent data types. (Rfc 2387)
        /// </summary>
        Multipart_related = Multipart | (1 << 17),

        /// <summary>
        /// Multipart signed. Defined in Rfc 1847.
        /// </summary>
        Multipart_signed = Multipart | (1 << 18),

        /// <summary>
        /// Message -- an encapsulated message.
        /// </summary>
        Message = 1 << 19,

        /// <summary>
        /// Rfc 822 encapsulated message.
        /// </summary>
        Message_rfc822 = Message | (1 << 20),

        /// <summary>
        /// Media type isn't specified.
        /// </summary>
        NotSpecified = 1 << 21,

        /// <summary>
        /// Media type is unknown.
        /// </summary>
        Unknown = 1 << 22,
    }
}