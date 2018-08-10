/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


namespace ASC.Mail.Net.RTP
{
    /// <summary>
    /// RTP payload specifies data type which is RTP packet.
    /// IANA registered RTP payload types. Defined in http://www.iana.org/assignments/rtp-parameters.
    /// </summary>
    public class RTP_PayloadTypes
    {
        #region Members

        /// <summary>
        /// CELB video codec. Defined in RFC 2029.
        /// </summary>
        public static readonly int CELB = 25;

        /// <summary>
        /// DVI4 11025hz audio codec.
        /// </summary>
        public static readonly int DVI4_11025 = 16;

        /// <summary>
        /// DVI4 16khz audio codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int DVI4_16000 = 6;

        /// <summary>
        /// DVI4 220505hz audio codec.
        /// </summary>
        public static readonly int DVI4_22050 = 17;

        /// <summary>
        /// DVI4 8khz audio codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int DVI4_8000 = 5;

        /// <summary>
        /// G722 audio codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int G722 = 9;

        /// <summary>
        /// G723 audio codec.
        /// </summary>
        public static readonly int G723 = 4;

        /// <summary>
        /// G728 audio codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int G728 = 15;

        /// <summary>
        /// G729 audio codec.
        /// </summary>
        public static readonly int G729 = 18;

        /// <summary>
        /// GSM audio codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int GSM = 3;

        /// <summary>
        /// H261 video codec. Defined in RFC 2032.
        /// </summary>
        public static readonly int H261 = 31;

        /// <summary>
        /// H263 video codec.
        /// </summary>
        public static readonly int H263 = 34;

        /// <summary>
        /// JPEG video codec. Defined in RFC 2435.
        /// </summary>
        public static readonly int JPEG = 26;

        /// <summary>
        /// L16 1 channel audio codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int L16_1CH = 10;

        /// <summary>
        /// L16 2 channel audio codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int L16_2CH = 11;

        /// <summary>
        /// LPC audio codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int LPC = 7;

        /// <summary>
        /// MP2T video codec. Defined in RFC 2250.
        /// </summary>
        public static readonly int MP2T = 33;

        /// <summary>
        /// MPA audio codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int MPA = 14;

        /// <summary>
        /// H261 video codec. Defined in RFC 2250.
        /// </summary>
        public static readonly int MPV = 32;

        /// <summary>
        /// NV video codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int NV = 28;

        /// <summary>
        /// PCMA(a-law) audio codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int PCMA = 8;

        /// <summary>
        /// PCMU8(u-law) audio codec. Defined in RFC 3551.
        /// </summary>
        public static readonly int PCMU;

        /// <summary>
        /// QCELP audio codec.
        /// </summary>
        public static readonly int QCELP = 12;

        #endregion
    }
}