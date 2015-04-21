/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


namespace ASC.Mail.Net.Mime.vCard
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// vCal phone number type. Note this values may be flagged !
    /// </summary>
    [Flags]
    public enum PhoneNumberType_enum
    {
        /// <summary>
        /// Phone number type not specified.
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// Preferred phone number.
        /// </summary>
        Preferred = 1,

        /// <summary>
        /// Telephone number associated with a residence.
        /// </summary>
        Home = 2,

        /// <summary>
        /// Telephone number has voice messaging support.
        /// </summary>
        Msg = 4,

        /// <summary>
        /// Telephone number associated with a place of work.
        /// </summary>
        Work = 8,

        /// <summary>
        /// Voice telephone number.
        /// </summary>
        Voice = 16,

        /// <summary>
        /// Fax number.
        /// </summary>
        Fax = 32,

        /// <summary>
        /// Cellular phone number.
        /// </summary>
        Cellular = 64,

        /// <summary>
        /// Video conferencing telephone number.
        /// </summary>
        Video = 128,

        /// <summary>
        /// Paging device telephone number.
        /// </summary>
        Pager = 256,

        /// <summary>
        /// Bulletin board system telephone number.
        /// </summary>
        BBS = 512,

        /// <summary>
        /// Modem connected telephone number.
        /// </summary>
        Modem = 1024,

        /// <summary>
        /// Car-phone telephone number.
        /// </summary>
        Car = 2048,

        /// <summary>
        /// ISDN service telephone number.
        /// </summary>
        ISDN = 4096,

        /// <summary>
        /// Personal communication services telephone number.
        /// </summary>
        PCS = 8192,
    }
}