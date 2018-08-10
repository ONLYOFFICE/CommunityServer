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


namespace ASC.Xmpp.Core.protocol.client
{
    /// <summary>
    ///   Enumeration for the Presence Type structure. This enum is used to describe what type of Subscription Type the current subscription is. When sending a presence or receiving a subscription this type is used to easily identify the type of subscription it is.
    /// </summary>
    public enum PresenceType
    {
        /// <summary>
        ///   Used when one wants to send presence to someone/server/transport that you�re available.
        /// </summary>
        available = -1,

        /// <summary>
        ///   Used to send a subscription request to someone.
        /// </summary>
        subscribe,

        /// <summary>
        ///   Used to accept a subscription request.
        /// </summary>
        subscribed,

        /// <summary>
        ///   Used to unsubscribe someone from your presence.
        /// </summary>
        unsubscribe,

        /// <summary>
        ///   Used to deny a subscription request.
        /// </summary>
        unsubscribed,

        /// <summary>
        ///   Used when one wants to send presence to someone/server/transport that you�re unavailable.
        /// </summary>
        unavailable,

        /// <summary>
        ///   Used when you want to see your roster, but don't want anyone on you roster to see you
        /// </summary>
        invisible,

        /// <summary>
        ///   presence error
        /// </summary>
        error,

        /// <summary>
        ///   used in server to server protocol to request presences
        /// </summary>
        probe
    }
}