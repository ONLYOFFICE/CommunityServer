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

namespace ASC.Mail.Net.Mime.vCard
{
    /// <summary>
    /// vCard delivery address implementation.
    /// </summary>
    public class DeliveryAddress
    {
        #region Members

        private readonly Item m_pItem;
        private string m_Country = "";

        private string m_ExtendedAddress = "";
        private string m_Locality = "";
        private string m_PostalCode = "";
        private string m_PostOfficeAddress = "";
        private string m_Region = "";
        private string m_Street = "";

        private DeliveryAddressType_enum m_Type = DeliveryAddressType_enum.Ineternational |
                                                  DeliveryAddressType_enum.Postal |
                                                  DeliveryAddressType_enum.Parcel |
                                                  DeliveryAddressType_enum.Work;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="item">Owner vCard item.</param>
        /// <param name="addressType">Address type. Note: This value can be flagged value !</param>
        /// <param name="postOfficeAddress">Post office address.</param>
        /// <param name="extendedAddress">Extended address.</param>
        /// <param name="street">Street name.</param>
        /// <param name="locality">Locality(city).</param>
        /// <param name="region">Region.</param>
        /// <param name="postalCode">Postal code.</param>
        /// <param name="country">Country.</param>
        internal DeliveryAddress(Item item,
                                 DeliveryAddressType_enum addressType,
                                 string postOfficeAddress,
                                 string extendedAddress,
                                 string street,
                                 string locality,
                                 string region,
                                 string postalCode,
                                 string country)
        {
            m_pItem = item;
            m_Type = addressType;
            m_PostOfficeAddress = postOfficeAddress;
            m_ExtendedAddress = extendedAddress;
            m_Street = street;
            m_Locality = locality;
            m_Region = region;
            m_PostalCode = postalCode;
            m_Country = country;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets address type. Note: This property can be flagged value !
        /// </summary>
        public DeliveryAddressType_enum AddressType
        {
            get { return m_Type; }

            set
            {
                m_Type = value;
                Changed();
            }
        }

        /// <summary>
        /// Gets or sets country.
        /// </summary>
        public string Country
        {
            get { return m_Country; }

            set
            {
                m_Country = value;
                Changed();
            }
        }

        /// <summary>
        /// Gests or sets extended address.
        /// </summary>
        public string ExtendedAddress
        {
            get { return m_ExtendedAddress; }

            set
            {
                m_ExtendedAddress = value;
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

        /// <summary>
        /// Gets or sets locality(city).
        /// </summary>
        public string Locality
        {
            get { return m_Locality; }

            set
            {
                m_Locality = value;
                Changed();
            }
        }

        /// <summary>
        /// Gets or sets postal code.
        /// </summary>
        public string PostalCode
        {
            get { return m_PostalCode; }

            set
            {
                m_PostalCode = value;
                Changed();
            }
        }

        /// <summary>
        /// Gets or sets post office address.
        /// </summary>
        public string PostOfficeAddress
        {
            get { return m_PostOfficeAddress; }

            set
            {
                m_PostOfficeAddress = value;
                Changed();
            }
        }

        /// <summary>
        /// Gets or sets region.
        /// </summary>
        public string Region
        {
            get { return m_Region; }

            set
            {
                m_Region = value;
                Changed();
            }
        }

        /// <summary>
        /// Gets or sets street.
        /// </summary>
        public string Street
        {
            get { return m_Street; }

            set
            {
                m_Street = value;
                Changed();
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parses delivery address from vCard ADR structure string.
        /// </summary>
        /// <param name="item">vCard ADR item.</param>
        internal static DeliveryAddress Parse(Item item)
        {
            DeliveryAddressType_enum type = DeliveryAddressType_enum.NotSpecified;
            if (item.ParametersString.ToUpper().IndexOf("PREF") != -1)
            {
                type |= DeliveryAddressType_enum.Preferred;
            }
            if (item.ParametersString.ToUpper().IndexOf("DOM") != -1)
            {
                type |= DeliveryAddressType_enum.Domestic;
            }
            if (item.ParametersString.ToUpper().IndexOf("INTL") != -1)
            {
                type |= DeliveryAddressType_enum.Ineternational;
            }
            if (item.ParametersString.ToUpper().IndexOf("POSTAL") != -1)
            {
                type |= DeliveryAddressType_enum.Postal;
            }
            if (item.ParametersString.ToUpper().IndexOf("PARCEL") != -1)
            {
                type |= DeliveryAddressType_enum.Parcel;
            }
            if (item.ParametersString.ToUpper().IndexOf("HOME") != -1)
            {
                type |= DeliveryAddressType_enum.Home;
            }
            if (item.ParametersString.ToUpper().IndexOf("WORK") != -1)
            {
                type |= DeliveryAddressType_enum.Work;
            }

            string[] items = item.DecodedValue.Split(';');
            return new DeliveryAddress(item,
                                       type,
                                       items.Length >= 1 ? items[0] : "",
                                       items.Length >= 2 ? items[1] : "",
                                       items.Length >= 3 ? items[2] : "",
                                       items.Length >= 4 ? items[3] : "",
                                       items.Length >= 5 ? items[4] : "",
                                       items.Length >= 6 ? items[5] : "",
                                       items.Length >= 7 ? items[6] : "");
        }

        /// <summary>
        /// Converts DeliveryAddressType_enum to vCard item parameters string.
        /// </summary>
        /// <param name="type">Value to convert.</param>
        /// <returns></returns>
        internal static string AddressTypeToString(DeliveryAddressType_enum type)
        {
            string retVal = "";
            if ((type & DeliveryAddressType_enum.Domestic) != 0)
            {
                retVal += "DOM,";
            }
            if ((type & DeliveryAddressType_enum.Home) != 0)
            {
                retVal += "HOME,";
            }
            if ((type & DeliveryAddressType_enum.Ineternational) != 0)
            {
                retVal += "INTL,";
            }
            if ((type & DeliveryAddressType_enum.Parcel) != 0)
            {
                retVal += "PARCEL,";
            }
            if ((type & DeliveryAddressType_enum.Postal) != 0)
            {
                retVal += "POSTAL,";
            }
            if ((type & DeliveryAddressType_enum.Preferred) != 0)
            {
                retVal += "Preferred,";
            }
            if ((type & DeliveryAddressType_enum.Work) != 0)
            {
                retVal += "Work,";
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
        /// This method is called when some property has changed, we need to update underlaying vCard item.
        /// </summary>
        private void Changed()
        {
            string value = "" + m_PostOfficeAddress + ";" + m_ExtendedAddress + ";" + m_Street + ";" +
                           m_Locality + ";" + m_Region + ";" + m_PostalCode + ";" + m_Country;

            m_pItem.ParametersString = AddressTypeToString(m_Type);
            m_pItem.SetDecodedValue(value);
        }

        #endregion
    }
}