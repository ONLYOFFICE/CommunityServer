/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

namespace ASC.Xmpp.Core.protocol.iq.time
{
    // Send:<iq type='get' id='MX_7' to='jfrankel@coversant.net/SoapBox'>
    //			<time xmlns='urn:jabber:time'/>
    //		</iq>
    //
    // Recv:<iq from="jfrankel@coversant.net/SoapBox" id="MX_7" to="gnauck@myjabber.net/Office" type="result">
    //			<time xmlns='urn:jabber:time'>
    //				<tzo>-06:00</tzo>
    //				<utc>2006-12-19T17:58:35Z</utc>
    //			</time>
    //		</iq>

    public class EntityTime : Element
    {
        public EntityTime()
        {
            TagName = "time";
            Namespace = Uri.ENTITY_TIME;
        }

        /// <summary>
        ///   The entity's numeric time zone offset from UTC. The format MUST conform to the Time Zone Definition (TZD) specified in XEP-0082.
        /// </summary>
        public string Tzo
        {
            get { return GetTag("tzo"); }
            set { SetTag("tzo", value); }
        }

        /// <summary>
        ///   The UTC time according to the responding entity. The format MUST conform to the dateTime profile specified in XEP-0082 and MUST be expressed in UTC.
        /// </summary>
        public string Utc
        {
            get { return GetTag("utc"); }
            set { SetTag("utc", value); }
        }
    }
}