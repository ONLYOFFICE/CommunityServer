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
    #region usings

    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// vCard phone number collection implementation.
    /// </summary>
    public class PhoneNumberCollection : IEnumerable
    {
        #region Members

        private readonly List<PhoneNumber> m_pCollection;
        private readonly vCard m_pOwner;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="owner">Owner vCard.</param>
        internal PhoneNumberCollection(vCard owner)
        {
            m_pOwner = owner;
            m_pCollection = new List<PhoneNumber>();

            foreach (Item item in owner.Items.Get("TEL"))
            {
                m_pCollection.Add(PhoneNumber.Parse(item));
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

        #endregion

        #region Methods

        /// <summary>
        /// Add new phone number to the collection.
        /// </summary>
        /// <param name="type">Phone number type. Note: This value can be flagged value !</param>
        /// <param name="number">Phone number.</param>
        public void Add(PhoneNumberType_enum type, string number)
        {
            Item item = m_pOwner.Items.Add("TEL", PhoneNumber.PhoneTypeToString(type), number);
            m_pCollection.Add(new PhoneNumber(item, type, number));
        }

        /// <summary>
        /// Removes specified item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        public void Remove(PhoneNumber item)
        {
            m_pOwner.Items.Remove(item.Item);
            m_pCollection.Remove(item);
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            foreach (PhoneNumber number in m_pCollection)
            {
                m_pOwner.Items.Remove(number.Item);
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