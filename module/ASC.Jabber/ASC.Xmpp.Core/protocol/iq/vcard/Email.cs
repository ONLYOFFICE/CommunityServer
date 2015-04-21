/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
    //	<!-- Email address property. Default type is INTERNET. -->
    //	<!ELEMENT EMAIL (
    //	HOME?, 
    //	WORK?, 
    //	INTERNET?, 
    //	PREF?, 
    //	X400?, 
    //	USERID
    //	)>
    public enum EmailType
    {
        NONE = -1,
        HOME,
        WORK,
        INTERNET,
        X400,
    }

    /// <summary>
    /// </summary>
    public class Email : Element
    {
        #region << Constructors >>

        public Email()
        {
            TagName = "EMAIL";
            Namespace = Uri.VCARD;
        }

        /// <summary>
        /// </summary>
        /// <param name="type"> Type of the new Email Adress </param>
        /// <param name="address"> Email Adress </param>
        /// <param name="prefered"> Is this adressed prefered </param>
        public Email(EmailType type, string userid, bool prefered) : this()
        {
            Type = type;
            IsPrefered = prefered;
            UserId = userid;
        }

        #endregion

        // <EMAIL><INTERNET/><PREF/><USERID>stpeter@jabber.org</USERID></EMAIL>

        public EmailType Type
        {
            get { return (EmailType) HasTagEnum(typeof (EmailType)); }
            set
            {
                if (value != EmailType.NONE)
                    SetTag(value.ToString());
            }
        }

        /// <summary>
        ///   Is this the prefered contact adress?
        /// </summary>
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

        /// <summary>
        ///   The email Adress
        /// </summary>
        public string UserId
        {
            get { return GetTag("USERID"); }
            set { SetTag("USERID", value); }
        }
    }
}