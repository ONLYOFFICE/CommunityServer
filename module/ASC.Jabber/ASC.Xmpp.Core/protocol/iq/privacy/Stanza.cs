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


namespace ASC.Xmpp.Core.protocol.iq.privacy
{
    /// <summary>
    ///   enum for block or allow communications. This flags could be combined under the following conditions.
    /// </summary>
    /// <remarks>
    ///   <list type="bullet">
    ///     <item>All must stand alone, its not allowed to combine thsi flag</item>
    ///     <item>Message, Iq, IncomingPresence and Outgoing Presence could be combined,
    ///       <b>but</b>
    ///       its not allowed to combine more than 3 of this flag.
    ///       If you need all of them you have to use the All flag</item>
    ///   </list>
    /// </remarks>
    public enum Stanza
    {
        /// <summary>
        ///   Block all stanzas !!! Don't combine this flag with others!!!
        /// </summary>
        All = 0,

        /// <summary>
        ///   Block messages
        /// </summary>
        Message = 1,

        /// <summary>
        ///   Block IQs
        /// </summary>
        Iq = 2,

        /// <summary>
        ///   Block Incoming Presences
        /// </summary>
        IncomingPresence = 4,

        /// <summary>
        ///   Block Outgoing Presences
        /// </summary>
        OutgoingPresence = 8,
    }
}