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

namespace ASC.Mail.Net.Mime.vCard
{
    /// <summary>
    /// vCard email address implementation.
    /// </summary>
    public class EmailAddress
    {
        #region Members

        private readonly Item m_pItem;
        private string m_EmailAddress = "";
        private EmailAddressType_enum m_Type = EmailAddressType_enum.Internet;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="item">Owner vCard item.</param>
        /// <param name="type">Email type. Note: This value can be flagged value !</param>
        /// <param name="emailAddress">Email address.</param>
        internal EmailAddress(Item item, EmailAddressType_enum type, string emailAddress)
        {
            m_pItem = item;
            m_Type = type;
            m_EmailAddress = emailAddress;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets email address.
        /// </summary>
        public string Email
        {
            get { return m_EmailAddress; }

            set
            {
                m_EmailAddress = value;
                Changed();
            }
        }

        /// <summary>
        /// Gets or sets email type. Note: This property can be flagged value !
        /// </summary>
        public EmailAddressType_enum EmailType
        {
            get { return m_Type; }

            set
            {
                m_Type = value;
                Changed();
            }
        }

        /// <summary>
        /// Gets underlaying vCrad item.
        /// </summary>
        public Item Item
        {
            get { return m_pItem; }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parses email address from vCard EMAIL structure string.
        /// </summary>
        /// <param name="item">vCard EMAIL item.</param>
        internal static EmailAddress Parse(Item item)
        {
            EmailAddressType_enum type = EmailAddressType_enum.NotSpecified;
            if (item.ParametersString.ToUpper().IndexOf("PREF") != -1)
            {
                type |= EmailAddressType_enum.Preferred;
            }
            if (item.ParametersString.ToUpper().IndexOf("INTERNET") != -1)
            {
                type |= EmailAddressType_enum.Internet;
            }
            if (item.ParametersString.ToUpper().IndexOf("X400") != -1)
            {
                type |= EmailAddressType_enum.X400;
            }

            return new EmailAddress(item, type, item.DecodedValue);
        }

        /// <summary>
        /// Converts EmailAddressType_enum to vCard item parameters string.
        /// </summary>
        /// <param name="type">Value to convert.</param>
        /// <returns></returns>
        internal static string EmailTypeToString(EmailAddressType_enum type)
        {
            string retVal = "";
            if ((type & EmailAddressType_enum.Internet) != 0)
            {
                retVal += "INTERNET,";
            }
            if ((type & EmailAddressType_enum.Preferred) != 0)
            {
                retVal += "PREF,";
            }
            if ((type & EmailAddressType_enum.X400) != 0)
            {
                retVal += "X400,";
            }
            if (retVal.EndsWith(","))
            {
                retVal = retVal.Substring(0, retVal.Length - 1);
            }

            return retVal;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// This method is called when some property has changed, wee need to update underlaying vCard item.
        /// </summary>
        private void Changed()
        {
            m_pItem.ParametersString = EmailTypeToString(m_Type);
            m_pItem.SetDecodedValue(m_EmailAddress);
        }

        #endregion
    }
}