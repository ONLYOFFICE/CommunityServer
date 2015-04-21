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


using ASC.Xmpp.Core.protocol.x.data;
using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.commands
{
    public class Command : Element
    {
        #region << Constructors >>

        public Command()
        {
            TagName = "command";
            Namespace = Uri.COMMANDS;
        }

        public Command(string node) : this()
        {
            Node = node;
        }

        public Command(Action action) : this()
        {
            Action = action;
        }

        public Command(Status status) : this()
        {
            Status = status;
        }

        public Command(string node, string sessionId) : this(node)
        {
            SessionId = sessionId;
        }

        public Command(string node, string sessionId, Action action) : this(node, sessionId)
        {
            Action = action;
        }

        public Command(string node, string sessionId, Status status) : this(node, sessionId)
        {
            Status = status;
        }

        public Command(string node, string sessionId, Action action, Status status) : this(node, sessionId, action)
        {
            Status = status;
        }

        #endregion

        public Action Action
        {
            get { return (Action) GetAttributeEnum("action", typeof (Action)); }
            set
            {
                if (value == Action.NONE)
                    RemoveAttribute("action");
                else
                    SetAttribute("action", value.ToString());
            }
        }

        public Status Status
        {
            get { return (Status) GetAttributeEnum("status", typeof (Status)); }
            set
            {
                if (value == Status.NONE)
                    RemoveAttribute("status");
                else
                    SetAttribute("status", value.ToString());
            }
        }


        // <xs:attribute name='node' type='xs:string' use='required'/>

        /// <summary>
        ///   Node is Required
        /// </summary>
        public string Node
        {
            get { return GetAttribute("node"); }
            set { SetAttribute("node", value); }
        }

        // <xs:attribute name='sessionid' type='xs:string' use='optional'/>
        public string SessionId
        {
            get { return GetAttribute("sessionid"); }
            set { SetAttribute("sessionid", value); }
        }

        /// <summary>
        ///   The X-Data Element
        /// </summary>
        public Data Data
        {
            get { return SelectSingleElement(typeof (Data)) as Data; }
            set
            {
                if (HasTag(typeof (Data)))
                    RemoveTag(typeof (Data));

                if (value != null)
                    AddChild(value);
            }
        }

        public Note Note
        {
            get { return SelectSingleElement(typeof (Note)) as Note; }
            set
            {
                if (HasTag(typeof (Note)))
                    RemoveTag(typeof (Note));

                if (value != null)
                    AddChild(value);
            }
        }

        public Actions Actions
        {
            get { return SelectSingleElement(typeof (Actions)) as Actions; }
            set
            {
                if (HasTag(typeof (Actions)))
                    RemoveTag(typeof (Actions));

                if (value != null)
                    AddChild(value);
            }
        }
    }
}