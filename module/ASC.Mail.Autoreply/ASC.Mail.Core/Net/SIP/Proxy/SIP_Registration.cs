/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
    using System.Collections.Generic;
    using Message;
    using Stack;

    #endregion

    /// <summary>
    /// This class implements SIP registrar registration entry. Defined in RFC 3261 10.3.
    /// </summary>
    public class SIP_Registration
    {
        #region Members

        private readonly string m_AOR = "";
        private readonly DateTime m_CreateTime;
        private readonly List<SIP_RegistrationBinding> m_pBindings;
        private readonly object m_pLock = new object();
        private readonly string m_UserName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets time when this registration entry was created.
        /// </summary>
        public DateTime CreateTime
        {
            get { return m_CreateTime; }
        }

        /// <summary>
        /// Gets user name who owns this registration.
        /// </summary>
        public string UserName
        {
            get { return m_UserName; }
        }

        /// <summary>
        /// Gets registration address of record.
        /// </summary>
        public string AOR
        {
            get { return m_AOR; }
        }

        /// <summary>
        /// Gets this registration priority ordered bindings.
        /// </summary>
        public SIP_RegistrationBinding[] Bindings
        {
            get
            {
                SIP_RegistrationBinding[] retVal = m_pBindings.ToArray();

                // Sort by qvalue, higer qvalue means higher priority.
                Array.Sort(retVal);

                return retVal;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="userName">User name who owns this registration.</param>
        /// <param name="aor">Address of record. For example: john.doe@lumisoft.ee.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>userName</b> or <b>aor</b> is null reference.</exception>
        public SIP_Registration(string userName, string aor)
        {
            if (userName == null)
            {
                throw new ArgumentNullException("userName");
            }
            if (aor == null)
            {
                throw new ArgumentNullException("aor");
            }
            if (aor == "")
            {
                throw new ArgumentException("Argument 'aor' value must be specified.");
            }

            m_UserName = userName;
            m_AOR = aor;

            m_CreateTime = DateTime.Now;
            m_pBindings = new List<SIP_RegistrationBinding>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets matching binding. Returns null if no match.
        /// </summary>
        /// <param name="contactUri">URI to match.</param>
        /// <returns>Returns matching binding. Returns null if no match.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>contactUri</b> is null reference.</exception>
        public SIP_RegistrationBinding GetBinding(AbsoluteUri contactUri)
        {
            if (contactUri == null)
            {
                throw new ArgumentNullException("contactUri");
            }

            lock (m_pLock)
            {
                foreach (SIP_RegistrationBinding binding in m_pBindings)
                {
                    if (contactUri.Equals(binding.ContactURI))
                    {
                        return binding;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Adds or updates matching bindings.
        /// </summary>
        /// <param name="flow">SIP data flow what updates this binding. This value is null if binding was not added through network or
        /// flow has disposed.</param>
        /// <param name="callID">Call-ID header field value.</param>
        /// <param name="cseqNo">CSeq header field sequence number value.</param>
        /// <param name="contacts">Contacts to add or update.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>callID</b> or <b>contacts</b> is null reference.</exception>
        public void AddOrUpdateBindings(SIP_Flow flow,
                                        string callID,
                                        int cseqNo,
                                        SIP_t_ContactParam[] contacts)
        {
            if (callID == null)
            {
                throw new ArgumentNullException("callID");
            }
            if (cseqNo < 0)
            {
                throw new ArgumentException("Argument 'cseqNo' value must be >= 0.");
            }
            if (contacts == null)
            {
                throw new ArgumentNullException("contacts");
            }

            lock (m_pLock)
            {
                foreach (SIP_t_ContactParam contact in contacts)
                {
                    SIP_RegistrationBinding binding = GetBinding(contact.Address.Uri);
                    // Add binding.
                    if (binding == null)
                    {
                        binding = new SIP_RegistrationBinding(this, contact.Address.Uri);
                        m_pBindings.Add(binding);
                    }

                    // Update binding.
                    binding.Update(flow,
                                   contact.Expires == -1 ? 3600 : contact.Expires,
                                   contact.QValue == -1 ? 1.0 : contact.QValue,
                                   callID,
                                   cseqNo);
                }
            }
        }

        /// <summary>
        /// Removes specified binding.
        /// </summary>
        /// <param name="binding">Registration binding.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>binding</b> is null reference.</exception>
        public void RemoveBinding(SIP_RegistrationBinding binding)
        {
            if (binding == null)
            {
                throw new ArgumentNullException("binding");
            }

            lock (m_pLock)
            {
                m_pBindings.Remove(binding);
            }
        }

        /// <summary>
        /// Removes all this registration bindings.
        /// </summary>
        public void RemoveAllBindings()
        {
            lock (m_pLock)
            {
                m_pBindings.Clear();
            }
        }

        /// <summary>
        /// Removes all expired bindings.
        /// </summary>
        public void RemoveExpiredBindings()
        {
            lock (m_pLock)
            {
                for (int i = 0; i < m_pBindings.Count; i++)
                {
                    if (m_pBindings[i].IsExpired)
                    {
                        m_pBindings.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        #endregion
    }
}