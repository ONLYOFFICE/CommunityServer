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