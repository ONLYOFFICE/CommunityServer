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
