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


using ASC.Xmpp.Core.utils.Xml.Dom;
using System;

namespace ASC.Xmpp.Core.protocol.Base
{

    #region usings

    #endregion

    /// <summary>
    ///   Base XMPP Element This must ne used to build all other new packets
    /// </summary>
    public abstract class DirectionalElement : Element
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public DirectionalElement()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="tag"> </param>
        public DirectionalElement(string tag) : base(tag)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="tag"> </param>
        /// <param name="ns"> </param>
        public DirectionalElement(string tag, string ns) : base(tag)
        {
            Namespace = ns;
        }

        /// <summary>
        /// </summary>
        /// <param name="tag"> </param>
        /// <param name="text"> </param>
        /// <param name="ns"> </param>
        public DirectionalElement(string tag, string text, string ns) : base(tag, text)
        {
            Namespace = ns;
        }

        #endregion

        #region Properties

        public int InternalId { get; set; }

        public DateTime DbStamp { get; set; }

        /// <summary>
        /// </summary>
        public Jid From
        {
            get
            {
                if (HasAttribute("from"))
                {
                    return new Jid(GetAttribute("from"));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    SetAttribute("from", value.ToString());
                }
                else
                {
                    RemoveAttribute("from");
                }
            }
        }

        /// <summary>
        /// </summary>
        public Jid To
        {
            get
            {
                if (HasAttribute("to"))
                {
                    return new Jid(GetAttribute("to"));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    SetAttribute("to", value.ToString());
                }
                else
                {
                    RemoveAttribute("to");
                }
            }
        }

        public bool Switched { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        ///   Switches the from and to attributes when existing
        /// </summary>
        public void SwitchDirection()
        {
            Jid from = From;
            Jid to = To;

            // Remove from and to now
            RemoveAttribute("from");
            RemoveAttribute("to");

            Jid helper = null;

            helper = from;
            from = to;
            to = helper;

            From = from;
            To = to;

            Switched = !Switched;
        }

        #endregion
    }
}