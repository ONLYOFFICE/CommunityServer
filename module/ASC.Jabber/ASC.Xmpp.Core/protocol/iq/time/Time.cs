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

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.iq.time
{
    // Send:<iq type='get' id='MX_7' to='jfrankel@coversant.net/SoapBox'>
    //			<query xmlns='jabber:iq:time'></query>
    //		</iq>
    //
    // Recv:<iq from="jfrankel@coversant.net/SoapBox" id="MX_7" to="gnauck@myjabber.net/Office" type="result">
    //			<query xmlns="jabber:iq:time">
    //				<utc>20050125T00:06:15</utc>
    //				<display>Tuesday, January 25, 2005 12:06:15 AM</display>	
    //				<tz>W. Europe Standard Time</tz>
    //			</query>
    //		</iq> 

    /// <summary>
    ///   Zusammenfassung f�r Time.
    /// </summary>
    public class Time : Element
    {
        public Time()
        {
            TagName = "query";
            Namespace = Uri.IQ_TIME;
        }


        public string Utc
        {
            get { return GetTag("utc"); }
            set { SetTag("utc", value); }
        }

        /// <summary>
        ///   Timezone
        /// </summary>
        //TODO: return System.TimeZone?
        public string Tz
        {
            get { return GetTag("tz"); }
            set { SetTag("tz", value); }
        }

        /// <summary>
        ///   Human-readable date/time.
        /// </summary>
        public string Display
        {
            get { return GetTag("display"); }
            set { SetTag("display", value); }
        }
    }
}