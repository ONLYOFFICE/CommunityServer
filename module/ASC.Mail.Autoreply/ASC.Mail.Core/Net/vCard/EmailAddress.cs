/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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