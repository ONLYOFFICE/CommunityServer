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


using ASC.Xmpp.Core.protocol.client;

namespace ASC.Xmpp.Core.protocol.extensions.featureneg
{
    /// <summary>
    ///   JEP-0020: Feature Negotiation This JEP defines a A protocol that enables two Jabber entities to mutually negotiate feature options.
    /// </summary>
    public class FeatureNegIq : IQ
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

        private readonly FeatureNeg m_FeatureNeg = new FeatureNeg();

        public FeatureNegIq()
        {
            AddChild(m_FeatureNeg);
            GenerateId();
        }

        public FeatureNegIq(IqType type) : this()
        {
            Type = type;
        }

        public FeatureNeg FeatureNeg
        {
            get { return m_FeatureNeg; }
        }
    }
}