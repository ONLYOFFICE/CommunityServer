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


namespace ASC.Mail.Net.SIP.Proxy
{
    #region usings

    using System;
    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// SIP registration collection.
    /// </summary>
    public class SIP_RegistrationCollection : IEnumerable
    {
        #region Members

        private readonly List<SIP_Registration> m_pRegistrations;

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of registrations in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pRegistrations.Count; }
        }

        /// <summary>
        /// Gets registration with specified registration name. Returns null if specified registration doesn't exist.
        /// </summary>
        /// <param name="addressOfRecord">Address of record of resgistration.</param>
        /// <returns></returns>
        public SIP_Registration this[string addressOfRecord]
        {
            get
            {
                lock (m_pRegistrations)
                {
                    foreach (SIP_Registration registration in m_pRegistrations)
                    {
                        if (registration.AOR.ToLower() == addressOfRecord.ToLower())
                        {
                            return registration;
                        }
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets SIP registrations what in the collection.
        /// </summary>
        public SIP_Registration[] Values
        {
            get { return m_pRegistrations.ToArray(); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_RegistrationCollection()
        {
            m_pRegistrations = new List<SIP_Registration>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds specified registration to collection.
        /// </summary>
        /// <param name="registration">Registration to add.</param>
        public void Add(SIP_Registration registration)
        {
            lock (m_pRegistrations)
            {
                if (Contains(registration.AOR))
                {
                    throw new ArgumentException(
                        "Registration with specified registration name already exists !");
                }

                m_pRegistrations.Add(registration);
            }
        }

        /// <summary>
        /// Deletes specified registration and all it's contacts. 
        /// </summary>
        /// <param name="addressOfRecord">Registration address of record what to remove.</param>
        public void Remove(string addressOfRecord)
        {
            lock (m_pRegistrations)
            {
                foreach (SIP_Registration registration in m_pRegistrations)
                {
                    if (registration.AOR.ToLower() == addressOfRecord.ToLower())
                    {
                        m_pRegistrations.Remove(registration);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets if registration with specified name already exists.
        /// </summary>
        /// <param name="addressOfRecord">Address of record of resgistration.</param>
        /// <returns></returns>
        public bool Contains(string addressOfRecord)
        {
            lock (m_pRegistrations)
            {
                foreach (SIP_Registration registration in m_pRegistrations)
                {
                    if (registration.AOR.ToLower() == addressOfRecord.ToLower())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Removes all expired registrations from the collection.
        /// </summary>
        public void RemoveExpired()
        {
            lock (m_pRegistrations)
            {
                for (int i = 0; i < m_pRegistrations.Count; i++)
                {
                    SIP_Registration registration = m_pRegistrations[i];

                    // Force registration to remove all its expired contacts.
                    registration.RemoveExpiredBindings();
                    // No bindings left, so we need to remove that registration.
                    if (registration.Bindings.Length == 0)
                    {
                        m_pRegistrations.Remove(registration);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pRegistrations.GetEnumerator();
        }

        #endregion
    }
}