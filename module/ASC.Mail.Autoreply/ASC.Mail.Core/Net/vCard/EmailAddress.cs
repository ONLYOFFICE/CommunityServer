/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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