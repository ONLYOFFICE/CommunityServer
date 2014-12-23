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
    #region usings

    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// vCard delivery address collection implementation.
    /// </summary>
    public class DeliveryAddressCollection : IEnumerable
    {
        #region Members

        private readonly List<DeliveryAddress> m_pCollection;
        private readonly vCard m_pOwner;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="owner">Owner vCard.</param>
        internal DeliveryAddressCollection(vCard owner)
        {
            m_pOwner = owner;
            m_pCollection = new List<DeliveryAddress>();

            foreach (Item item in owner.Items.Get("ADR"))
            {
                m_pCollection.Add(DeliveryAddress.Parse(item));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pCollection.Count; }
        }

        /// <summary>
        /// Gets item at the specified index.
        /// </summary>
        /// <param name="index">Index of item which to get.</param>
        /// <returns></returns>
        public DeliveryAddress this[int index]
        {
            get { return m_pCollection[index]; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add new delivery address to the collection.
        /// </summary>
        /// <param name="type">Delivery address type. Note: This value can be flagged value !</param>
        /// <param name="postOfficeAddress">Post office address.</param>
        /// <param name="extendedAddress">Extended address.</param>
        /// <param name="street">Street name.</param>
        /// <param name="locality">Locality(city).</param>
        /// <param name="region">Region.</param>
        /// <param name="postalCode">Postal code.</param>
        /// <param name="country">Country.</param>
        public void Add(DeliveryAddressType_enum type,
                        string postOfficeAddress,
                        string extendedAddress,
                        string street,
                        string locality,
                        string region,
                        string postalCode,
                        string country)
        {
            string value = "" + postOfficeAddress + ";" + extendedAddress + ";" + street + ";" + locality +
                           ";" + region + ";" + postalCode + ";" + country;

            Item item = m_pOwner.Items.Add("ADR", DeliveryAddress.AddressTypeToString(type), "");
            item.SetDecodedValue(value);
            m_pCollection.Add(new DeliveryAddress(item,
                                                  type,
                                                  postOfficeAddress,
                                                  extendedAddress,
                                                  street,
                                                  locality,
                                                  region,
                                                  postalCode,
                                                  country));
        }

        /// <summary>
        /// Removes specified item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        public void Remove(DeliveryAddress item)
        {
            m_pOwner.Items.Remove(item.Item);
            m_pCollection.Remove(item);
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            foreach (DeliveryAddress email in m_pCollection)
            {
                m_pOwner.Items.Remove(email.Item);
            }
            m_pCollection.Clear();
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pCollection.GetEnumerator();
        }

        #endregion
    }
}