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