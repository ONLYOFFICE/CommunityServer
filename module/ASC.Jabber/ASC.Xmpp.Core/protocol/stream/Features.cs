/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="Features.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.protocol.iq.bind;
using ASC.Xmpp.Core.protocol.sasl;
using ASC.Xmpp.Core.protocol.stream.feature;
using ASC.Xmpp.Core.protocol.stream.feature.compression;
using ASC.Xmpp.Core.protocol.tls;
using ASC.Xmpp.Core.utils.Xml.Dom;

//</stream:features>
// <stream:features>
//		<mechanisms xmlns='urn:ietf:params:xml:ns:xmpp-sasl'>
//			<mechanism>DIGEST-MD5</mechanism>
//			<mechanism>PLAIN</mechanism>
//		</mechanisms>
// </stream:features>

// <stream:features>
//		<starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'>
//			<required/>
//		</starttls>
//		<mechanisms xmlns='urn:ietf:params:xml:ns:xmpp-sasl'>
//			<mechanism>DIGEST-MD5</mechanism>
//			<mechanism>PLAIN</mechanism>
//		</mechanisms>
// </stream:features>

namespace ASC.Xmpp.Core.protocol.stream
{
    /// <summary>
    ///   Summary description for Features.
    /// </summary>
    public class Features : Element
    {
        public Features()
        {
            TagName = "features";
            Namespace = Uri.STREAM;
        }

        public StartTls StartTls
        {
            get { return SelectSingleElement(typeof (StartTls)) as StartTls; }
            set
            {
                if (HasTag(typeof (StartTls)))
                    RemoveTag(typeof (StartTls));

                if (value != null)
                    AddChild(value);
            }
        }

        public Bind Bind
        {
            get { return SelectSingleElement(typeof (Bind)) as Bind; }
            set
            {
                if (HasTag(typeof (Bind)))
                    RemoveTag(typeof (Bind));

                if (value != null)
                    AddChild(value);
            }
        }

        // <stream:stream from="beta.soapbox.net" xml:lang="de" id="373af7e9-6107-4729-8cea-e8b8ea05ceea" xmlns="jabber:client" version="1.0" xmlns:stream="http://etherx.jabber.org/streams">

        // <stream:features xmlns:stream="http://etherx.jabber.org/streams">
        //      <compression xmlns="http://jabber.org/features/compress"><method>zlib</method></compression>
        //      <starttls xmlns="urn:ietf:params:xml:ns:xmpp-tls" />
        //      <register xmlns="http://jabber.org/features/iq-register" />
        //      <auth xmlns="http://jabber.org/features/iq-auth" />
        //      <mechanisms xmlns="urn:ietf:params:xml:ns:xmpp-sasl">
        //          <mechanism>PLAIN</mechanism>
        //          <mechanism>DIGEST-MD5</mechanism>
        //          <mechanism>ANONYMOUS</mechanism>
        //      </mechanisms>
        // </stream:features>


        public Compression Compression
        {
            get { return SelectSingleElement(typeof (Compression)) as Compression; }
            set
            {
                if (HasTag(typeof (Compression)))
                    RemoveTag(typeof (Compression));

                if (value != null)
                    AddChild(value);
            }
        }

        public Register Register
        {
            get { return SelectSingleElement(typeof (Register)) as Register; }
            set
            {
                if (HasTag(typeof (Register)))
                    RemoveTag(typeof (Register));

                if (value != null)
                    AddChild(value);
            }
        }

        public Mechanisms Mechanisms
        {
            get { return SelectSingleElement(typeof (Mechanisms)) as Mechanisms; }
            set
            {
                if (HasTag(typeof (Mechanisms)))
                    RemoveTag(typeof (Mechanisms));

                if (value != null)
                    AddChild(value);
            }
        }

        public bool SupportsBind
        {
            get { return Bind != null ? true : false; }
        }

        public bool SupportsStartTls
        {
            get { return StartTls != null ? true : false; }
        }

        /// <summary>
        ///   Is Stream Compression supported?
        /// </summary>
        public bool SupportsCompression
        {
            get { return Compression != null ? true : false; }
        }

        /// <summary>
        ///   Is Registration supported?
        /// </summary>
        public bool SupportsRegistration
        {
            get { return Register != null ? true : false; }
        }
    }
}