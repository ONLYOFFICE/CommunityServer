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
    /// vCard email address collection implementation.
    /// </summary>
    public class EmailAddressCollection : IEnumerable
    {
        #region Members

        private readonly List<EmailAddress> m_pCollection;
        private readonly vCard m_pOwner;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="owner">Owner vCard.</param>
        internal EmailAddressCollection(vCard owner)
        {
            m_pOwner = owner;
            m_pCollection = new List<EmailAddress>();

            foreach (Item item in owner.Items.Get("EMAIL"))
            {
                m_pCollection.Add(EmailAddress.Parse(item));
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
        public EmailAddress this[int index]
        {
            get { return m_pCollection[index]; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add new email address to the collection.
        /// </summary>
        /// <param name="type">Email address type. Note: This value can be flagged value !</param>
        /// <param name="email">Email address.</param>
        public EmailAddress Add(EmailAddressType_enum type, string email)
        {
            Item item = m_pOwner.Items.Add("EMAIL", EmailAddress.EmailTypeToString(type), "");
            item.SetDecodedValue(email);
            EmailAddress emailAddress = new EmailAddress(item, type, email);
            m_pCollection.Add(emailAddress);

            return emailAddress;
        }

        /// <summary>
        /// Removes specified item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        public void Remove(EmailAddress item)
        {
            m_pOwner.Items.Remove(item.Item);
            m_pCollection.Remove(item);
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            foreach (EmailAddress email in m_pCollection)
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