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

namespace ASC.Xmpp.Core.protocol.iq.vcard
{
    public enum AddressLocation
    {
        NONE = -1,
        HOME,
        WORK
    }

    /// <summary>
    /// </summary>
    public class Address : Element
    {
        //		<!-- Structured address property. Address components with
        //		multiple values must be specified as a comma separated list
        //		of values. -->
        //		<!ELEMENT ADR (
        //		HOME?, 
        //		WORK?, 
        //		POSTAL?, 
        //		PARCEL?, 
        //		(DOM | INTL)?, 
        //		PREF?, 
        //		POBOX?, 
        //		EXTADR?, 
        //		STREET?, 
        //		LOCALITY?, 
        //		REGION?, 
        //		PCODE?, 
        //		CTRY?
        //		)>

        // <ADR>
        //	<WORK/>
        //	<EXTADD>Suite 600</EXTADD>
        //	<STREET>1899 Wynkoop Street</STREET>
        //	<LOCALITY>Denver</LOCALITY>
        //	<REGION>CO</REGION>
        //	<PCODE>80202</PCODE>
        //	<CTRY>USA</CTRY>
        // </ADR>
        public Address()
        {
            TagName = "ADR";
            Namespace = Uri.VCARD;
        }

        public Address(AddressLocation loc, string extra, string street, string locality, string region,
                       string postalcode, string country, bool prefered) : this()
        {
            Location = loc;
            ExtraAddress = extra;
            Street = street;
            Locality = locality;
            Region = region;
            PostalCode = postalcode;
            Country = country;
            IsPrefered = prefered;
        }

        public AddressLocation Location
        {
            get { return (AddressLocation) HasTagEnum(typeof (AddressLocation)); }
            set { SetTag(value.ToString()); }
        }

        public bool IsPrefered
        {
            get { return HasTag("PREF"); }
            set
            {
                if (value)
                    SetTag("PREF");
                else
                    RemoveTag("PREF");
            }
        }

        public string ExtraAddress
        {
            get { return GetTag("EXTADD"); }
            set { SetTag("EXTADD", value); }
        }

        public string Street
        {
            get { return GetTag("STREET"); }
            set { SetTag("STREET", value); }
        }

        public string Locality
        {
            get { return GetTag("LOCALITY"); }
            set { SetTag("LOCALITY", value); }
        }

        public string Region
        {
            get { return GetTag("REGION"); }
            set { SetTag("REGION", value); }
        }

        public string PostalCode
        {
            get { return GetTag("PCODE"); }
            set { SetTag("PCODE", value); }
        }

        public string Country
        {
            get { return GetTag("CTRY"); }
            set { SetTag("CTRY", value); }
        }
    }
}