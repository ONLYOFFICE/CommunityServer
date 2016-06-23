/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


namespace ASC.Xmpp.Core.protocol.sasl
{
    public enum FailureCondition
    {
        /// <summary>
        ///   The receiving entity acknowledges an <abort /> element sent by the initiating entity; sent in reply to the <abort /> element.
        /// </summary>
        aborted,

        /// <summary>
        ///   The data provided by the initiating entity could not be processed because the [BASE64] (Josefsson, S., �The Base16, Base32, and Base64 Data Encodings,� July 2003.) encoding is incorrect (e.g., because the encoding does not adhere to the definition in Section 3 of [BASE64] (Josefsson, S., �The Base16, Base32, and Base64 Data Encodings,� July 2003.)); sent in reply to a <response /> element or an <auth /> element with initial response data.
        /// </summary>
        incorrect_encoding,

        /// <summary>
        ///   The authzid provided by the initiating entity is invalid, either because it is incorrectly formatted or because the initiating entity does not have permissions to authorize that ID; sent in reply to a <response /> element or an <auth /> element with initial response data.
        /// </summary>
        invalid_authzid,

        /// <summary>
        ///   The initiating entity did not provide a mechanism or requested a mechanism that is not supported by the receiving entity; sent in reply to an <auth /> element.
        /// </summary>
        invalid_mechanism,

        /// <summary>
        ///   The mechanism requested by the initiating entity is weaker than server policy permits for that initiating entity; sent in reply to a <response /> element or an <auth /> element with initial response data.
        /// </summary>
        mechanism_too_weak,

        /// <summary>
        ///   The authentication failed because the initiating entity did not provide valid credentials (this includes but is not limited to the case of an unknown username); sent in reply to a <response /> element or an <auth /> element with initial response data.
        /// </summary>
        not_authorized,

        /// <summary>
        ///   The authentication failed because of a temporary error condition within the receiving entity; sent in reply to an <auth /> element or <response /> element.
        /// </summary>
        temporary_auth_failure,

        UnknownCondition
    }
}