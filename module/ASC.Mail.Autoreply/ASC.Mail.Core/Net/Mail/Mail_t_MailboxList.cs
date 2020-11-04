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


namespace ASC.Mail.Net.Mail
{
    #region usings

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    #endregion

    /// <summary>
    /// This class represents <b>mailbox-list</b>. Defined in RFC 5322 3.4.
    /// </summary>
    public class Mail_t_MailboxList : IEnumerable
    {
        #region Members

        private readonly List<Mail_t_Mailbox> m_pList;
        private bool m_IsModified;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if list has modified since it was loaded.
        /// </summary>
        public bool IsModified
        {
            get { return m_IsModified; }
        }

        /// <summary>
        /// Gets number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pList.Count; }
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>Returns the element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when <b>index</b> is out of range.</exception>
        public Mail_t_Mailbox this[int index]
        {
            get
            {
                if (index < 0 || index >= m_pList.Count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                return m_pList[index];
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Mail_t_MailboxList()
        {
            m_pList = new List<Mail_t_Mailbox>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts a address into the collection at the specified location.
        /// </summary>
        /// <param name="index">The location in the collection where you want to add the item.</param>
        /// <param name="value">Address to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when <b>index</b> is out of range.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public void Insert(int index, Mail_t_Mailbox value)
        {
            if (index < 0 || index > m_pList.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            m_pList.Insert(index, value);
            m_IsModified = true;
        }

        /// <summary>
        /// Adds specified address to the end of the collection.
        /// </summary>
        /// <param name="value">Address to add.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference value.</exception>
        public void Add(Mail_t_Mailbox value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            m_pList.Add(value);
            m_IsModified = true;
        }

        /// <summary>
        /// Removes specified item from the collection.
        /// </summary>
        /// <param name="value">Address to remove.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference value.</exception>
        public void Remove(Mail_t_Mailbox value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            m_pList.Remove(value);
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            m_pList.Clear();
            m_IsModified = true;
        }

        /// <summary>
        /// Copies addresses to new array.
        /// </summary>
        /// <returns>Returns addresses array.</returns>
        public Mail_t_Mailbox[] ToArray()
        {
            return m_pList.ToArray();
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pList.GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder retVal = new StringBuilder();
            for (int i = 0; i < m_pList.Count; i++)
            {
                if (i == (m_pList.Count - 1))
                {
                    retVal.Append(m_pList[i].ToString().Trim());
                }
                else
                {
                    retVal.Append(m_pList[i].ToString().Trim() + ",");
                }
            }

            return retVal.ToString();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Resets IsModified property to false.
        /// </summary>
        internal void AcceptChanges()
        {
            m_IsModified = false;
        }

        #endregion
    }
}