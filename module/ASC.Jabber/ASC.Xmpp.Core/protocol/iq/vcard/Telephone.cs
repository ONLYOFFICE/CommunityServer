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
    public enum TelephoneLocation
    {
        NONE = -1,
        HOME,
        WORK
    }

    public enum TelephoneType
    {
        NONE = -1,
        VOICE,
        FAX,
        PAGER,
        MSG,
        CELL,
        VIDEO,
        BBS,
        MODEM,
        ISDN,
        PCS,
        PREF,
        NUMBER
    }

    /// <summary>
    ///   Zusammenfassung f�r Telephone.
    /// </summary>
    public class Telephone : Element
    {
        #region << Constructors >>

        public Telephone()
        {
            TagName = "TEL";
            Namespace = Uri.VCARD;
        }

        public Telephone(TelephoneLocation loc, TelephoneType type, string number) : this()
        {
            if (loc != TelephoneLocation.NONE)
                Location = loc;

            if (type != TelephoneType.NONE)
                Type = type;

            Number = number;
        }

        #endregion

        //	<TEL><VOICE/><WORK/><NUMBER>303-308-3282</NUMBER></TEL>
        //	<TEL><FAX/><WORK/><NUMBER/></TEL>
        //	<TEL><MSG/><WORK/><NUMBER/></TEL>

        public string Number
        {
            get { return GetTag("NUMBER"); }
            set { SetTag("NUMBER", value); }
        }

        public TelephoneLocation Location
        {
            get { return (TelephoneLocation) HasTagEnum(typeof (TelephoneLocation)); }
            set { SetTag(value.ToString()); }
        }

        public TelephoneType Type
        {
            get { return (TelephoneType) HasTagEnum(typeof (TelephoneType)); }
            set { SetTag(value.ToString()); }
        }
    }
}