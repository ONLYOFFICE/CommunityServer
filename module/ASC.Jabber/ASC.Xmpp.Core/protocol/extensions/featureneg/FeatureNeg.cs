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


using ASC.Xmpp.Core.protocol.x.data;
using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.featureneg
{
    /// <summary>
    ///   JEP-0020: Feature Negotiation This JEP defines a A protocol that enables two Jabber entities to mutually negotiate feature options.
    /// </summary>
    public class FeatureNeg : Element
    {
        /*
		<iq type='get'
			from='romeo@montague.net/orchard'
			to='juliet@capulet.com/balcony'
			id='neg1'>
			<feature xmlns='http://jabber.org/protocol/feature-neg'>
				<x xmlns='jabber:x:data' type='form'>
				<field type='list-single' var='places-to-meet'>
					<option><value>Lover's Lane</value></option>
					<option><value>Secret Grotto</value></option>
					<option><value>Verona Park</value></option>
				</field>
				<field type='list-single' var='times-to-meet'>
					<option><value>22:00</value></option>
					<option><value>22:30</value></option>
					<option><value>23:00</value></option>
					<option><value>23:30</value></option>
				</field>
				</x>
			</feature>
		</iq>
		*/

        public FeatureNeg()
        {
            TagName = "feature";
            Namespace = Uri.FEATURE_NEG;
        }

        /// <summary>
        ///   data form of type "form" which defines the available options for one or more features. Each feature is represented as an x-data "field", which MUST be of type "list-single".
        /// </summary>
        public x.data.Data Data
        {
            get
            {
                Element data = SelectSingleElement(typeof (x.data.Data));
                if (data != null)
                    return data as x.data.Data;
                else
                    return null;
            }
            set
            {
                if (HasTag(typeof (x.data.Data)))
                    RemoveTag(typeof (x.data.Data));

                AddChild(value);
            }
        }
    }
}