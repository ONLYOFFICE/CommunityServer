/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
    using Message;
    using Stack;

    #endregion

    /// <summary>
    /// This class represents SIP registrar registration binding entry. Defined in RFC 3261 10.3.
    /// </summary>
    public class SIP_RegistrationBinding : IComparable
    {
        #region Members

        private readonly AbsoluteUri m_ContactURI;
        private readonly SIP_Registration m_pRegistration;
        private string m_CallID = "";
        private int m_CSeqNo = 1;
        private int m_Expires = 3600;
        private DateTime m_LastUpdate;
        private SIP_Flow m_pFlow;
        private double m_QValue = 1.0;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the last time when the binding was updated.
        /// </summary>
        public DateTime LastUpdate
        {
            get { return m_LastUpdate; }
        }

        /// <summary>
        /// Gets if binding has expired.
        /// </summary>
        public bool IsExpired
        {
            get { return TTL <= 0; }
        }

        /// <summary>
        /// Gets how many seconds binding has time to live. This is live calulated value, so it decreases every second.
        /// </summary>
        public int TTL
        {
            get
            {
                if (DateTime.Now > m_LastUpdate.AddSeconds(m_Expires))
                {
                    return 0;
                }
                else
                {
                    return (int) ((m_LastUpdate.AddSeconds(m_Expires) - DateTime.Now)).TotalSeconds;
                }
            }
        }

        /// <summary>
        /// Gets data flow what added this binding. This value is null if binding was not added through network or
        /// flow has disposed.
        /// </summary>
        public SIP_Flow Flow
        {
            get { return m_pFlow; }
        }

        /// <summary>
        /// Gets contact URI what can be used to contact the registration.
        /// </summary>
        public AbsoluteUri ContactURI
        {
            get { return m_ContactURI; }
        }

        /// <summary>
        /// Gets binding priority. Higher value means greater priority.
        /// </summary>
        public double QValue
        {
            get { return m_QValue; }
        }

        /// <summary>
        /// Gets Call-ID header field value which added this binding.
        /// </summary>
        public string CallID
        {
            get { return m_CallID; }
        }

        /// <summary>
        /// Gets CSeq header field sequence number value which added this binding.
        /// </summary>
        public int CSeqNo
        {
            get { return m_CSeqNo; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="owner">Owner registration.</param>
        /// <param name="contactUri">Contact URI what can be used to contact the registration.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>owner</b> or <b>contactUri</b> is null reference.</exception>
        internal SIP_RegistrationBinding(SIP_Registration owner, AbsoluteUri contactUri)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (contactUri == null)
            {
                throw new ArgumentNullException("contactUri");
            }

            m_pRegistration = owner;
            m_ContactURI = contactUri;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates specified binding.
        /// </summary>
        /// <param name="flow">SIP data flow what updates this binding. This value is null if binding was not added through network or
        /// flow has disposed.</param>
        /// <param name="expires">Time in seconds when binding will expire.</param>
        /// <param name="qvalue">Binding priority. Higher value means greater priority.</param>
        /// <param name="callID">Call-ID header field value which added/updated this binding.</param>
        /// <param name="cseqNo">CSeq header field sequence number value which added/updated this binding.</param>
        public void Update(SIP_Flow flow, int expires, double qvalue, string callID, int cseqNo)
        {
            if (expires < 0)
            {
                throw new ArgumentException("Argument 'expires' value must be >= 0.");
            }
            if (qvalue < 0 || qvalue > 1)
            {
                throw new ArgumentException("Argument 'qvalue' value must be >= 0.000 and <= 1.000");
            }
            if (callID == null)
            {
                throw new ArgumentNullException("callID");
            }
            if (cseqNo < 0)
            {
                throw new ArgumentException("Argument 'cseqNo' value must be >= 0.");
            }

            m_pFlow = flow;
            m_Expires = expires;
            m_QValue = qvalue;
            m_CallID = callID;
            m_CSeqNo = cseqNo;

            m_LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Removes this binding from the registration.
        /// </summary>
        public void Remove()
        {
            m_pRegistration.RemoveBinding(this);
        }

        /// <summary>
        /// Converts <b>ContactUri</b> to valid Contact header value.
        /// </summary>
        /// <returns>Returns contact header value.</returns>
        public string ToContactValue()
        {
            SIP_t_ContactParam retVal = new SIP_t_ContactParam();
            retVal.Parse(new StringReader(m_ContactURI.ToString()));
            retVal.Expires = m_Expires;

            return retVal.ToStringValue();
        }

        /// <summary>
        /// Compares the current instance with another object of the same type. 
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>Returns 0 if two objects equal, -1 if this instance is less or 1 this instance is greater.</returns>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }
            if (!(obj is SIP_RegistrationBinding))
            {
                return -1;
            }

            // We must reverse values, because greater value mean higer priority.

            SIP_RegistrationBinding compareValue = (SIP_RegistrationBinding) obj;
            if (compareValue.QValue == QValue)
            {
                return 0;
            }
            else if (compareValue.QValue > QValue)
            {
                return 1;
            }
            else if (compareValue.QValue < QValue)
            {
                return -1;
            }

            return -1;
        }

        #endregion
    }
}