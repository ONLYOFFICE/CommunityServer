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

using ASC.Xmpp.Core.protocol.client;

namespace ASC.Xmpp.Server
{
    public static class SignalRHelper
    {
        private const byte NO_STATE = 0;
        public const byte USER_ONLINE = 1;
        private const byte USER_AWAY = 2;
        private const byte USER_NOT_AVAILVABLE = 3;
        public const byte USER_OFFLINE = 4;
        public const int PRIORITY = 1;
        public const int NUMBER_OF_RECENT_MSGS = 10;
        public const string SIGNALR_RESOURCE = "SignalR";

        public static ShowType GetShowType(byte state)
        {
            switch (state)
            {
                case USER_ONLINE:
                    return ShowType.NONE;

                case USER_AWAY:
                    return ShowType.away;

                case USER_NOT_AVAILVABLE:
                    return ShowType.xa;

                default:
                    return ShowType.NONE;
            }
        }

        public static PresenceType GetPresenceType(byte state)
        {
            switch (state)
            {
                case USER_OFFLINE:
                case NO_STATE:
                    return PresenceType.unavailable;
                default:
                    return PresenceType.available;
            }
        }

        public static byte GetState(ShowType show, PresenceType type)
        {
            switch (show)
            {
                case ShowType.NONE:
                    switch (type)
                    {
                        case PresenceType.unavailable:
                            return USER_OFFLINE;
                        default:
                            return USER_ONLINE;
                    }
                case ShowType.chat:
                    return USER_ONLINE;
                case ShowType.away:
                    return USER_AWAY;
                case ShowType.dnd:
                case ShowType.xa:
                    return USER_NOT_AVAILVABLE;
                default:
                    return NO_STATE;
            }
        }
    }
}
