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


namespace ASC.Mail.Net.Mime
{
    #region usings

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    #endregion

    /// <summary>
    /// Rfc 2822 3.4 address-list. Rfc defines two types of addresses mailbox and group.
    /// <p/>
    /// <p style="margin-top: 0; margin-bottom: 0"/><b>address-list</b> syntax: address *("," address).
    /// <p style="margin-top: 0; margin-bottom: 0"/><b>address</b> syntax: mailbox / group.
    /// <p style="margin-top: 0; margin-bottom: 0"/><b>mailbox</b> syntax: ['"'dispaly-name'"' ]&lt;localpart@domain&gt;.
    /// <p style="margin-top: 0; margin-bottom: 0"/><b>group</b> syntax: '"'dispaly-name'":' [mailbox *(',' mailbox)]';'.
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public class AddressList : IEnumerable
    {
        #region Members

        private readonly List<Address> m_pAddresses;
        private HeaderField m_HeaderField;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AddressList()
        {
            m_pAddresses = new List<Address>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets address count in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pAddresses.Count; }
        }

        /// <summary>
        /// Gets address from specified index.
        /// </summary>
        public Address this[int index]
        {
            get { return m_pAddresses[index]; }
        }

        /// <summary>
        /// Gets all mailbox addresses. Note: group address mailbox addresses are also included.
        /// </summary>
        public MailboxAddress[] Mailboxes
        {
            get
            {
                ArrayList adressesAll = new ArrayList();
                foreach (Address adress in this)
                {
                    if (!adress.IsGroupAddress)
                    {
                        adressesAll.Add(adress);
                    }
                    else
                    {
                        foreach (MailboxAddress groupChildAddress in ((GroupAddress) adress).GroupMembers)
                        {
                            adressesAll.Add(groupChildAddress);
                        }
                    }
                }

                MailboxAddress[] retVal = new MailboxAddress[adressesAll.Count];
                adressesAll.CopyTo(retVal);

                return retVal;
            }
        }

        /// <summary>
        /// Bound address-list to specified header field.
        /// </summary>
        internal HeaderField BoundedHeaderField
        {
            get { return m_HeaderField; }

            set { m_HeaderField = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new address to the end of the collection.
        /// </summary>
        /// <param name="address">Address to add.</param>
        public void Add(Address address)
        {
            address.Owner = this;
            m_pAddresses.Add(address);

            OnCollectionChanged();
        }

        /// <summary>
        /// Inserts a new address into the collection at the specified location.
        /// </summary>
        /// <param name="index">The location in the collection where you want to add the address.</param>
        /// <param name="address">Address to add.</param>
        public void Insert(int index, Address address)
        {
            address.Owner = this;
            m_pAddresses.Insert(index, address);

            OnCollectionChanged();
        }

        /// <summary>
        /// Removes address at the specified index from the collection.
        /// </summary>
        /// <param name="index">Index of the address which to remove.</param>
        public void Remove(int index)
        {
            Remove(m_pAddresses[index]);
        }

        /// <summary>
        /// Removes specified address from the collection.
        /// </summary>
        /// <param name="address">Address to remove.</param>
        public void Remove(Address address)
        {
            address.Owner = null;
            m_pAddresses.Remove(address);

            OnCollectionChanged();
        }

        /// <summary>
        /// Clears the collection of all addresses.
        /// </summary>
        public void Clear()
        {
            foreach (Address address in m_pAddresses)
            {
                address.Owner = null;
            }
            m_pAddresses.Clear();

            OnCollectionChanged();
        }

        /// <summary>
        /// Parses address-list from string.
        /// </summary>
        /// <param name="addressList">Address list string.</param>
        /// <returns></returns>
        public void Parse(string addressList)
        {
            addressList = addressList.Trim();

            StringReader reader = new StringReader(addressList);
            while (reader.SourceString.Length > 0)
            {
                // See if mailbox or group. If ',' is before ':', then mailbox
                // Example: xxx@domain.com,	group:xxxgroup@domain.com;
                int commaIndex = TextUtils.QuotedIndexOf(reader.SourceString, ',');
                int colonIndex = TextUtils.QuotedIndexOf(reader.SourceString, ':');

                // Mailbox
                if (colonIndex == -1 || (commaIndex < colonIndex && commaIndex != -1))
                {
                    // FIX: why quotes missing
                    //System.Windows.Forms.MessageBox.Show(reader.SourceString + "#" + reader.OriginalString);

                    // Read to ',' or to end if last element
                    MailboxAddress address = MailboxAddress.Parse(reader.QuotedReadToDelimiter(','));
                    m_pAddresses.Add(address);
                    address.Owner = this;
                }
                    // Group
                else
                {
                    // Read to ';', this is end of group
                    GroupAddress address = GroupAddress.Parse(reader.QuotedReadToDelimiter(';'));
                    m_pAddresses.Add(address);
                    address.Owner = this;

                    // If there are next items, remove first comma because it's part of group address
                    if (reader.SourceString.Length > 0)
                    {
                        reader.QuotedReadToDelimiter(',');
                    }
                }
            }

            OnCollectionChanged();
        }

        /// <summary>
        /// Convert addresses to Rfc 2822 address-list string.
        /// </summary>
        /// <returns></returns>
        public string ToAddressListString()
        {
            StringBuilder retVal = new StringBuilder();
            for (int i = 0; i < m_pAddresses.Count; i++)
            {
                if (m_pAddresses[i] is MailboxAddress)
                {
                    // For last address don't add , and <TAB>
                    if (i == (m_pAddresses.Count - 1))
                    {
                        retVal.Append(((MailboxAddress) m_pAddresses[i]).ToMailboxAddressString());
                    }
                    else
                    {
                        retVal.Append(((MailboxAddress) m_pAddresses[i]).ToMailboxAddressString() + ",\t");
                    }
                }
                else if (m_pAddresses[i] is GroupAddress)
                {
                    // For last address don't add , and <TAB>
                    if (i == (m_pAddresses.Count - 1))
                    {
                        retVal.Append(((GroupAddress) m_pAddresses[i]).GroupString);
                    }
                    else
                    {
                        retVal.Append(((GroupAddress) m_pAddresses[i]).GroupString + ",\t");
                    }
                }
            }

            return retVal.ToString();
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pAddresses.GetEnumerator();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// This called when collection has changed. Item is added,deleted,changed or collection cleared.
        /// </summary>
        internal void OnCollectionChanged()
        {
            // AddressList is bounded to HeaderField, update header field value
            if (m_HeaderField != null)
            {
                m_HeaderField.Value = ToAddressListString();
            }
        }

        #endregion
    }
}