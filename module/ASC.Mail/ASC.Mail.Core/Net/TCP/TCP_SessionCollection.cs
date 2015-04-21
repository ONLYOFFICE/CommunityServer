/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System.Collections;

namespace ASC.Mail.Net.TCP
{
    #region usings

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// This class implements TCP session collection.
    /// </summary>
    public class TCP_SessionCollection<T>:IEnumerable<T> where T : TCP_Session
    {
        #region Members

        private readonly Dictionary<string, T> m_pItems;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal TCP_SessionCollection()
        {
            m_pItems = new Dictionary<string, T>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pItems.Count; }
        }

        /// <summary>
        /// Gets TCP session with the specified ID.
        /// </summary>
        /// <param name="id">Session ID.</param>
        /// <returns>Returns TCP session with the specified ID.</returns>
        public T this[string id]
        {
            get { return m_pItems[id]; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Copies all TCP server session to new array. This method is thread-safe.
        /// </summary>
        /// <returns>Returns TCP sessions array.</returns>
        public T[] ToArray()
        {
            lock (m_pItems)
            {
                T[] retVal = new T[m_pItems.Count];
                m_pItems.Values.CopyTo(retVal, 0);

                return retVal;
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Adds specified TCP session to the colletion.
        /// </summary>
        /// <param name="session">TCP server session to add.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>session</b> is null.</exception>
        internal void Add(T session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            lock (m_pItems)
            {
                m_pItems.Add(session.ID, session);
            }
        }

        /// <summary>
        /// Removes specified TCP server session from the collection.
        /// </summary>
        /// <param name="session">TCP server session to remove.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>session</b> is null.</exception>
        internal void Remove(T session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            lock (m_pItems)
            {
                m_pItems.Remove(session.ID);
            }
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        internal void Clear()
        {
            lock (m_pItems)
            {
                m_pItems.Clear();
            }
        }

        #endregion

        public IEnumerator<T> GetEnumerator()
        {
            return m_pItems.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}