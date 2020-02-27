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