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


#region using

using ASC.Xmpp.Core.utils.Xml.Dom;

#endregion

namespace ASC.Xmpp.Core.protocol.x.rosterx
{

    #region usings

    #endregion

    /// <summary>
    ///   Roster Item Exchange (JEP-0144)
    /// </summary>
    public class RosterX : Element
    {
        #region Constructor

        /// <summary>
        ///   Initializes a new instance of the <see cref="RosterX" /> class.
        /// </summary>
        public RosterX()
        {
            TagName = "x";
            Namespace = Uri.X_ROSTERX;
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Gets the roster.
        /// </summary>
        /// <returns> </returns>
        public RosterItem[] GetRoster()
        {
            ElementList nl = SelectElements(typeof (RosterItem));
            int i = 0;
            var result = new RosterItem[nl.Count];
            foreach (RosterItem ri in nl)
            {
                result[i] = ri;
                i++;
            }

            return result;
        }

        /// <summary>
        ///   Adds a roster item.
        /// </summary>
        /// <param name="r"> The r. </param>
        public void AddRosterItem(RosterItem r)
        {
            ChildNodes.Add(r);
        }

        #endregion

        /*
		<message from='horatio@denmark.lit' to='hamlet@denmark.lit'>
		<body>Some visitors, m'lord!</body>
		<x xmlns='http://jabber.org/protocol/rosterx'> 
			<item action='add'
				jid='rosencrantz@denmark.lit'
				name='Rosencrantz'>
				<group>Visitors</group>
			</item>
			<item action='add'
				jid='guildenstern@denmark.lit'
				name='Guildenstern'>
				<group>Visitors</group>
			</item>
		</x>
		</message>
		*/
    }
}