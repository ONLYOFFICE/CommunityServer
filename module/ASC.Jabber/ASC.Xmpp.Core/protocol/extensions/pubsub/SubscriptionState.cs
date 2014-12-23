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

namespace ASC.Xmpp.Core.protocol.extensions.pubsub
{
    /*
    None  	The node MUST NOT send event notifications or payloads to the Entity.
    Pending 	An entity has requested to subscribe to a node and the request has not yet been approved by a node owner. The node MUST NOT send event notifications or payloads to the entity while it is in this state.
    Unconfigured 	An entity has subscribed but its subscription options have not yet been configured. The node MAY send event notifications or payloads to the entity while it is in this state. The service MAY timeout unconfigured subscriptions.
    Subscribed
    */

    public enum SubscriptionState
    {
        /// <summary>
        ///   The node MUST NOT send event notifications or payloads to the Entity.
        /// </summary>
        none,

        /// <summary>
        ///   An entity has requested to subscribe to a node and the request has not yet been approved by a node owner. The node MUST NOT send event notifications or payloads to the entity while it is in this state.
        /// </summary>
        pending,

        /// <summary>
        ///   An entity has subscribed but its subscription options have not yet been configured. The node MAY send event notifications or payloads to the entity while it is in this state. The service MAY timeout unconfigured subscriptions.
        /// </summary>
        unconfigured,

        /// <summary>
        /// </summary>
        subscribed
    }
}