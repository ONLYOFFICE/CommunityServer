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


using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.extensions.nickname;
using ASC.Xmpp.Core.protocol.extensions.primary;
using ASC.Xmpp.Core.protocol.x;
using ASC.Xmpp.Core.protocol.x.muc;

namespace ASC.Xmpp.Core.protocol.client
{

    #region usings

    #endregion

    /// <summary>
    ///   Zusammenfassung f�r Presence.
    /// </summary>
    public class Presence : Stanza
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public Presence()
        {
            TagName = "presence";
            Namespace = Uri.CLIENT;
        }

        /// <summary>
        /// </summary>
        /// <param name="show"> </param>
        /// <param name="status"> </param>
        public Presence(ShowType show, string status)
            : this()
        {
            Show = show;
            Status = status;
        }

        /// <summary>
        /// </summary>
        /// <param name="show"> </param>
        /// <param name="status"> </param>
        /// <param name="priority"> </param>
        public Presence(ShowType show, string status, int priority)
            : this(show, status)
        {
            Priority = priority;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Error Child Element
        /// </summary>
        public Error Error
        {
            get { return SelectSingleElement(typeof (Error)) as Error; }

            set
            {
                if (HasTag(typeof (Error)))
                {
                    RemoveTag(typeof (Error));
                }

                if (value != null)
                {
                    AddChild(value);
                }
            }
        }

        /// <summary>
        /// </summary>
        public bool IsPrimary
        {
            get { return GetTag(typeof (Primary)) == null ? false : true; }

            set
            {
                if (value)
                {
                    SetTag(typeof (Primary));
                }
                else
                {
                    RemoveTag(typeof (Primary));
                }
            }
        }

        /// <summary>
        /// </summary>
        public User MucUser
        {
            get { return SelectSingleElement(typeof (User)) as User; }

            set
            {
                if (HasTag(typeof (User)))
                {
                    RemoveTag(typeof (User));
                }

                if (value != null)
                {
                    AddChild(value);
                }
            }
        }

        /// <summary>
        ///   Nickname Element
        /// </summary>
        public Nickname Nickname
        {
            get { return SelectSingleElement(typeof (Nickname)) as Nickname; }

            set
            {
                if (HasTag(typeof (Nickname)))
                {
                    RemoveTag(typeof (Nickname));
                }

                if (value != null)
                {
                    AddChild(value);
                }
            }
        }

        /// <summary>
        ///   The priority level of the resource. The value MUST be an integer between -128 and +127. If no priority is provided, a server SHOULD consider the priority to be zero.
        /// </summary>
        /// <remarks>
        ///   For information regarding the semantics of priority values in stanza routing within instant messaging and presence applications, refer to Server Rules for Handling XML StanzasServer Rules for Handling XML Stanzas.
        /// </remarks>
        public int Priority
        {
            get
            {
                try
                {
                    string tag = GetTag("priority");
                    if (!string.IsNullOrEmpty(tag)) return int.Parse(GetTag("priority"));
                    return 0;
                }
                catch
                {
                    return 0;
                }
            }

            set { SetTag("priority", value.ToString()); }
        }

        /// <summary>
        ///   The OPTIONAL show element contains non-human-readable XML character data that specifies the particular availability status of an entity or specific resource.
        /// </summary>
        public ShowType Show
        {
            get { return (ShowType) GetTagEnum("show", typeof (ShowType)); }

            set
            {
                if (value != ShowType.NONE)
                {
                    SetTag("show", value.ToString());
                }
                else
                {
                    RemoveAttribute("show");
                }
            }
        }

        /// <summary>
        ///   The OPTIONAL statuc contains a natural-language description of availability status. It is normally used in conjunction with the show element to provide a detailed description of an availability state (e.g., "In a meeting").
        /// </summary>
        public string Status
        {
            get { return GetTag("status"); }

            set { SetTag("status", value); }
        }

        /// <summary>
        ///   The type of a presence stanza is OPTIONAL. A presence stanza that does not possess a type attribute is used to signal to the server that the sender is online and available for communication. If included, the type attribute specifies a lack of availability, a request to manage a subscription to another entity's presence, a request for another entity's current presence, or an error related to a previously-sent presence stanza.
        /// </summary>
        public PresenceType Type
        {
            get { return (PresenceType) GetAttributeEnum("type", typeof (PresenceType)); }

            set
            {
                // dont add type="available"
                if (value == PresenceType.available)
                {
                    RemoveAttribute("type");
                }
                else
                {
                    SetAttribute("type", value.ToString());
                }
            }
        }

        /// <summary>
        /// </summary>
        public Delay XDelay
        {
            get { return SelectSingleElement(typeof (Delay)) as Delay; }

            set
            {
                if (HasTag(typeof (Delay)))
                {
                    RemoveTag(typeof (Delay));
                }

                if (value != null)
                {
                    AddChild(value);
                }
            }
        }

        #endregion
    }
}