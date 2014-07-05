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
// // <copyright company="Ascensio System Limited" file="Vcard.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using System;
using ASC.Xmpp.Core.utils.Xml.Dom;

// JEP-0054
// http://www.jabber.org/jeps/jep-0054.html

// Example 2. Receiving One's Own vCard
//
//	<iq 
//		to='stpeter@jabber.org/Gabber'
//		type='result'
//		id='v1'>
//	<vCard xmlns='vcard-temp'>
//		<FN>Peter Saint-Andre</FN>
//		<N>
//			<FAMILY>Saint-Andre<FAMILY>
//			<GIVEN>Peter</GIVEN>
//			<MIDDLE/>
//		</N>
//		<NICKNAME>stpeter</NICKNAME>
//		<URL>http://www.jabber.org/people/stpeter.php</URL>
//		<BDAY>1966-08-06</BDAY>
//		<ORG>
//		<ORGNAME>Jabber Software Foundation</ORGNAME>
//		<ORGUNIT/>
//		</ORG>
//		<TITLE>Executive Director</TITLE>
//		<ROLE>Patron Saint</ROLE>
//		<TEL><VOICE/><WORK/><NUMBER>303-308-3282</NUMBER></TEL>
//		<TEL><FAX/><WORK/><NUMBER/></TEL>
//		<TEL><MSG/><WORK/><NUMBER/></TEL>
//		<ADR>
//			<WORK/>
//			<EXTADD>Suite 600</EXTADD>
//			<STREET>1899 Wynkoop Street</STREET>
//			<LOCALITY>Denver</LOCALITY>
//			<REGION>CO</REGION>
//			<PCODE>80202</PCODE>
//			<CTRY>USA</CTRY>
//		</ADR>
//		<TEL><VOICE/><HOME/><NUMBER>303-555-1212</NUMBER></TEL>
//		<TEL><FAX/><HOME/><NUMBER/></TEL>
//		<TEL><MSG/><HOME/><NUMBER/></TEL>
//		<ADR>
//			<HOME/>
//			<EXTADD/>
//			<STREET/>
//			<LOCALITY>Denver</LOCALITY>
//			<REGION>CO</REGION>
//			<PCODE>80209</PCODE>
//			<CTRY>USA</CTRY>
//		</ADR>
//		<EMAIL><INTERNET/><PREF/><USERID>stpeter@jabber.org</USERID></EMAIL>
//		<JABBERID>stpeter@jabber.org</JABBERID>
//		<DESC>
//			More information about me is located on my 
//			personal website: http://www.saint-andre.com/
//		</DESC>	
//		<GENDER>Male</GENDER>
//		</vCard>
//</iq>
//    

namespace ASC.Xmpp.Core.protocol.iq.vcard
{
    //	<!-- Telephone number property. -->
    //	<!ELEMENT TEL (
    //	HOME?, 
    //	WORK?, 
    //	VOICE?, 
    //	FAX?, 
    //	PAGER?, 
    //	MSG?, 
    //	CELL?, 
    //	VIDEO?, 
    //	BBS?, 
    //	MODEM?, 
    //	ISDN?, 
    //	PCS?, 
    //	PREF?, 
    //	NUMBER
    //	)>


    /// <summary>
    ///   Summary description for Vcard.
    /// </summary>
    public class Vcard : Element
    {
        #region << Constructors >>

        public Vcard()
        {
            TagName = "vCard";
            Namespace = Uri.VCARD;
        }

        #endregion

        ///<summary>
        ///</summary>
        public string Url
        {
            get { return GetTag("URL"); }
            set { SetTag("URL", value); }
        }

        /// <summary>
        /// </summary>
        public DateTime Birthday
        {
            get
            {
                try
                {
                    string sDate = GetTag("BDAY");
                    if (sDate != null)
                        return DateTime.Parse(sDate);
                    else
                        return DateTime.MinValue;
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
            set { SetTag("BDAY", value.ToString("yyyy-MM-dd")); }
        }

        /// <summary>
        /// </summary>
        public string Title
        {
            get { return GetTag("TITLE"); }
            set { SetTag("TITLE", value); }
        }

        /// <summary>
        /// </summary>
        public string Role
        {
            get { return GetTag("ROLE"); }
            set { SetTag("ROLE", value); }
        }

        public string Fullname
        {
            get { return GetTag("FN"); }
            set { SetTag("FN", value); }
        }

        public string Nickname
        {
            get { return GetTag("NICKNAME"); }
            set { SetTag("NICKNAME", value); }
        }

        public Jid JabberId
        {
            get { return new Jid(GetTag("JABBERID")); }
            set { SetTag("JABBERID", value.ToString()); }
        }

        /// <summary>
        /// </summary>
        public string Description
        {
            get { return GetTag("DESC"); }
            set { SetTag("DESC", value); }
        }

        /// <summary>
        /// </summary>
        public Name Name
        {
            get { return SelectSingleElement(typeof (Name)) as Name; }
            set
            {
                Element n = SelectSingleElement(typeof (Name));
                if (n != null)
                    n.Remove();

                AddChild(value);
            }
        }

#if !CF
        /// <summary>
        ///   a Photograph
        /// </summary>
        public Photo Photo
        {
            get { return SelectSingleElement(typeof (Photo)) as Photo; }
            set
            {
                Element p = SelectSingleElement(typeof (Photo));
                if (p != null)
                    p.Remove();

                AddChild(value);
            }
        }
#endif

        /// <summary>
        /// </summary>
        public Organization Organization
        {
            get { return SelectSingleElement(typeof (Organization)) as Organization; }
            set
            {
                Element org = SelectSingleElement(typeof (Organization));
                if (org != null)
                    org.Remove();

                AddChild(value);
            }
        }

        public Gender Gender
        {
            get { return (Gender) GetTagEnum("GENDER", typeof (Gender)); }
            set
            {
                if (value == Gender.NONE) RemoveTag("GENDER");
                else SetTag("GENDER", value.ToString());
            }
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public Address[] GetAddresses()
        {
            ElementList el = SelectElements(typeof (Address));
            int i = 0;
            var result = new Address[el.Count];
            foreach (Address add in el)
            {
                result[i] = add;
                i++;
            }
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="loc"> </param>
        /// <returns> </returns>
        public Address GetAddress(AddressLocation loc)
        {
            foreach (Address adr in GetAddresses())
            {
                if (adr.Location == loc)
                    return adr;
            }
            return null;
        }


        public void AddAddress(Address addr)
        {
            Address a = GetAddress(addr.Location);
            if (a != null)
                a.Remove();

            AddChild(addr);
        }

        public Address GetPreferedAddress()
        {
            foreach (Address adr in GetAddresses())
            {
                if (adr.IsPrefered)
                    return adr;
            }
            return null;
        }


        public Telephone[] GetTelephoneNumbers()
        {
            ElementList el = SelectElements(typeof (Telephone));
            int i = 0;
            var result = new Telephone[el.Count];
            foreach (Telephone tel in el)
            {
                result[i] = tel;
                i++;
            }
            return result;
        }

        public Telephone GetTelephoneNumber(TelephoneType type, TelephoneLocation loc)
        {
            foreach (Telephone phone in GetTelephoneNumbers())
            {
                if (phone.Type == type && phone.Location == loc)
                    return phone;
            }
            return null;
        }

        public void AddTelephoneNumber(Telephone tel)
        {
            Telephone t = GetTelephoneNumber(tel.Type, tel.Location);
            if (t != null)
                t.Remove();

            AddChild(tel);
        }

        /// <summary>
        ///   Adds a new Email Adress object
        /// </summary>
        /// <param name="mail"> </param>
        public void AddEmailAddress(Email mail)
        {
            Email e = GetEmailAddress(mail.Type);
            if (e != null)
                e.Remove();

            AddChild(mail);
        }

        /// <summary>
        ///   Get all Email addresses
        /// </summary>
        /// <returns> </returns>
        public Email[] GetEmailAddresses()
        {
            ElementList el = SelectElements(typeof (Email));
            int i = 0;
            var result = new Email[el.Count];
            foreach (Email mail in el)
            {
                result[i] = mail;
                i++;
            }
            return result;
        }

        public Email GetEmailAddress(EmailType type)
        {
            foreach (Email email in GetEmailAddresses())
            {
                if (email.Type == type)
                    return email;
            }
            return null;
        }

        public Email GetPreferedEmailAddress()
        {
            foreach (Email email in GetEmailAddresses())
            {
                if (email.IsPrefered)
                    return email;
            }
            return null;
        }
    }
}