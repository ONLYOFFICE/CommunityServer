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
// // <copyright company="Ascensio System Limited" file="Version.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.iq.version
{
    // Send:<iq type='get' id='MX_6' to='jfrankel@coversant.net/SoapBox'>
    //			<query xmlns='jabber:iq:version'></query>
    //		</iq>
    //
    // Recv:<iq from="jfrankel@coversant.net/SoapBox" id="MX_6" to="gnauck@myjabber.net/Office" type="result">
    //			<query xmlns="jabber:iq:version">
    //				<name>SoapBox</name>
    //				<version>2.1.2 beta</version>
    //				<os>Windows NT 5.1 (en-us)</os>
    //			</query>
    //		</iq> 


    /// <summary>
    ///   Zusammenfassung f�r Version.
    /// </summary>
    public class Version : Element
    {
        public Version()
        {
            TagName = "query";
            Namespace = Uri.IQ_VERSION;
        }

        public string Name
        {
            set { SetTag("name", value); }
            get { return GetTag("name"); }
        }

        public string Ver
        {
            set { SetTag("version", value); }
            get { return GetTag("version"); }
        }

        public string Os
        {
            set { SetTag("os", value); }
            get { return GetTag("os"); }
        }
    }
}