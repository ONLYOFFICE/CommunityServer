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


namespace ASC.Mail.Net.AUTH
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Timers;

    #endregion

    /// <summary>
    /// HTTP digest authentication nonce manager.
    /// </summary>
    public class Auth_HttpDigest_NonceManager : IDisposable
    {
        #region Nested type: NonceEntry

        /// <summary>
        /// This class represents nonce entry in active nonces collection.
        /// </summary>
        private class NonceEntry
        {
            #region Members

            private readonly DateTime m_CreateTime;
            private readonly string m_Nonce = "";

            #endregion

            #region Properties

            /// <summary>
            /// Gets nonce value.
            /// </summary>
            public string Nonce
            {
                get { return m_Nonce; }
            }

            /// <summary>
            /// Gets time when this nonce entry was created.
            /// </summary>
            public DateTime CreateTime
            {
                get { return m_CreateTime; }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="nonce"></param>
            public NonceEntry(string nonce)
            {
                m_Nonce = nonce;
                m_CreateTime = DateTime.Now;
            }

            #endregion
        }

        #endregion

        #region Members

        private int m_ExpireTime = 30;
        private List<NonceEntry> m_pNonces;
        private Timer m_pTimer;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets nonce expire time in seconds.
        /// </summary>
        public int ExpireTime
        {
            get { return m_ExpireTime; }

            set
            {
                if (value < 5)
                {
                    throw new ArgumentException("Property ExpireTime value must be >= 5 !");
                }

                m_ExpireTime = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Auth_HttpDigest_NonceManager()
        {
            m_pNonces = new List<NonceEntry>();

            m_pTimer = new Timer(15000);
            m_pTimer.Elapsed += m_pTimer_Elapsed;
            m_pTimer.Enabled = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up nay resource being used.
        /// </summary>
        public void Dispose()
        {
            if (m_pNonces == null)
            {
                m_pNonces.Clear();
                m_pNonces = null;
            }

            if (m_pTimer != null)
            {
                m_pTimer.Dispose();
                m_pTimer = null;
            }
        }

        /// <summary>
        /// Creates new nonce and adds it to active nonces collection.
        /// </summary>
        /// <returns>Returns new created nonce.</returns>
        public string CreateNonce()
        {
            string nonce = Guid.NewGuid().ToString().Replace("-", "");
            m_pNonces.Add(new NonceEntry(nonce));

            return nonce;
        }

        /// <summary>
        /// Checks if specified nonce exists in active nonces collection.
        /// </summary>
        /// <param name="nonce">Nonce to check.</param>
        /// <returns>Returns true if nonce exists in active nonces collection, otherwise returns false.</returns>
        public bool NonceExists(string nonce)
        {
            lock (m_pNonces)
            {
                foreach (NonceEntry e in m_pNonces)
                {
                    if (e.Nonce == nonce)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Removes specified nonce from active nonces collection.
        /// </summary>
        /// <param name="nonce">Nonce to remove.</param>
        public void RemoveNonce(string nonce)
        {
            lock (m_pNonces)
            {
                for (int i = 0; i < m_pNonces.Count; i++)
                {
                    if (m_pNonces[i].Nonce == nonce)
                    {
                        m_pNonces.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        #endregion

        #region Event handlers

        private void m_pTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RemoveExpiredNonces();
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Removes not used nonces what has expired.
        /// </summary>
        private void RemoveExpiredNonces()
        {
            lock (m_pNonces)
            {
                for (int i = 0; i < m_pNonces.Count; i++)
                {
                    // Nonce expired, remove it.
                    if (m_pNonces[i].CreateTime.AddSeconds(m_ExpireTime) > DateTime.Now)
                    {
                        m_pNonces.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        #endregion
    }
}