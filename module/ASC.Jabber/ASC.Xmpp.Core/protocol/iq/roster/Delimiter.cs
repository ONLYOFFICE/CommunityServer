/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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

namespace ASC.Xmpp.Core.protocol.iq.roster
{
    /// <summary>
    ///   Extension JEP-0083, delimiter for nested roster groups
    /// </summary>
    public class Delimiter : Element
    {
        /*
		3.1 Querying for the delimiter 
		All compliant clients SHOULD query for an existing delimiter at login.

		Example 1. Querying for the Delimiter
			
		CLIENT:																												 CLIENT:
		<iq type='get'
			 id='1'>
		<query xmlns='jabber:iq:private'>
			 <roster xmlns='roster:delimiter'/>
				  </query>
		</iq>

		SERVER:
		<iq type='result'
			 id='1'
		from='bill@shakespeare.lit/Globe'
		to='bill@shakespeare.lit/Globe'>
		<query xmlns='jabber:iq:private'>
			 <roster xmlns='roster:delimiter'>::</roster>
		</query>
		</iq>
		*/

        public Delimiter()
        {
            TagName = "roster";
            Namespace = Uri.ROSTER_DELIMITER;
        }

        public Delimiter(string delimiter) : this()
        {
            Value = delimiter;
        }
    }
}