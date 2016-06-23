/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

namespace ASC.Xmpp.Core.protocol.iq.vcard
{
    //<iq id="id_62" to="gnauck@myjabber.net" type="get"><vCard xmlns="vcard-temp"/></iq>

    /// <summary>
    ///   Summary description for VcardIq.
    /// </summary>
    public class VcardIq : IQ
    {
        private readonly Vcard m_Vcard = new Vcard();

        #region << Constructors >>

        public VcardIq()
        {
            GenerateId();
            AddChild(m_Vcard);
        }

        public VcardIq(IqType type) : this()
        {
            Type = type;
        }

        public VcardIq(IqType type, Vcard vcard) : this(type)
        {
            Vcard = vcard;
        }

        public VcardIq(IqType type, Jid to) : this(type)
        {
            To = to;
        }

        public VcardIq(IqType type, Jid to, Vcard vcard) : this(type, to)
        {
            Vcard = vcard;
        }

        public VcardIq(IqType type, Jid to, Jid from) : this(type, to)
        {
            From = from;
        }

        public VcardIq(IqType type, Jid to, Jid from, Vcard vcard) : this(type, to, from)
        {
            Vcard = vcard;
        }

        #endregion

        public override Vcard Vcard
        {
            get { return m_Vcard; }
            set { ReplaceChild(value); }
        }
    }
}