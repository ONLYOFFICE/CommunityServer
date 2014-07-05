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