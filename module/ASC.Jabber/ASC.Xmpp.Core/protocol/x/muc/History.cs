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


using System;
using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.x.muc
{

    #region usings

    #endregion

    /*
        Example 29. User Requests Limit on Number of Messages in History

        <presence
            from='hag66@shakespeare.lit/pda'
            to='darkcave@macbeth.shakespeare.lit/thirdwitch'>
          <x xmlns='http://jabber.org/protocol/muc'>
            <history maxstanzas='20'/>
          </x>
        </presence>
              

        Example 30. User Requests History in Last 3 Minutes

        <presence
            from='hag66@shakespeare.lit/pda'
            to='darkcave@macbeth.shakespeare.lit/thirdwitch'>
          <x xmlns='http://jabber.org/protocol/muc'>
            <history seconds='180'/>
          </x>
        </presence>
              

        Example 31. User Requests All History Since the Beginning of the Unix Era

        <presence
            from='hag66@shakespeare.lit/pda'
            to='darkcave@macbeth.shakespeare.lit/thirdwitch'>
          <x xmlns='http://jabber.org/protocol/muc'>
            <history since='1970-01-01T00:00Z'/>
          </x>
        </presence>
    */

    /// <summary>
    ///   This is used to get the history of a muc room
    /// </summary>
    public class History : Element
    {
        #region Constructor

        /// <summary>
        ///   Empty default constructor
        /// </summary>
        public History()
        {
            TagName = "history";
            Namespace = Uri.MUC;
        }

        /// <summary>
        ///   get the history starting from a given date when available
        /// </summary>
        /// <param name="date"> </param>
        public History(DateTime date) : this()
        {
            Since = date;
        }

        /// <summary>
        ///   Specify the maximum nunber of messages to retrieve from the history
        /// </summary>
        /// <param name="max"> </param>
        public History(int max) : this()
        {
            MaxStanzas = max;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Limit the total number of characters in the history to "X" (where the character count is the characters of the complete XML stanzas, not only their XML character data).
        /// </summary>
        public int MaxCharacters
        {
            get { return GetAttributeInt("maxchars"); }

            set { SetAttribute("maxchars", value); }
        }

        /// <summary>
        ///   Request maximum stanzas of history when available
        /// </summary>
        public int MaxStanzas
        {
            get { return GetAttributeInt("maxstanzas"); }

            set { SetAttribute("maxstanzas", value); }
        }

        /// <summary>
        ///   request the last xxx seconds of history when available
        /// </summary>
        public int Seconds
        {
            get { return GetAttributeInt("seconds"); }

            set { SetAttribute("seconds", value); }
        }

        /// <summary>
        ///   Request history from a given date when available
        /// </summary>
        public DateTime Since
        {
            get { return Time.ISO_8601Date(GetAttribute("since")); }

            set { SetAttribute("since", Time.ISO_8601Date(value)); }
        }

        #endregion
    }
}