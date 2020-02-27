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


namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Implements same multi value header fields group. Group can contain one type header fields only.
    /// This is class is used by Via:,Route:, ... .
    /// </summary>
    public class SIP_MVGroupHFCollection<T> where T : SIP_t_Value, new()
    {
        #region Members

        private readonly string m_FieldName = "";
        private readonly List<SIP_MultiValueHF<T>> m_pFields;
        private readonly SIP_Message m_pMessage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets header field name what this group holds.
        /// </summary>
        public string FieldName
        {
            get { return m_FieldName; }
        }

        /// <summary>
        /// Gets number of header fields in this group.
        /// </summary>
        public int Count
        {
            get { return m_pFields.Count; }
        }

        /// <summary>
        /// Gets header fields what are in this group.
        /// </summary>
        public SIP_MultiValueHF<T>[] HeaderFields
        {
            get { return m_pFields.ToArray(); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="owner">Owner message that owns this group.</param>
        /// <param name="fieldName">Header field name what group holds.</param>
        public SIP_MVGroupHFCollection(SIP_Message owner, string fieldName)
        {
            m_pMessage = owner;
            m_FieldName = fieldName;

            m_pFields = new List<SIP_MultiValueHF<T>>();

            Refresh();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add new header field on the top of the whole header.
        /// </summary>
        /// <param name="value">Header field value.</param>
        public void AddToTop(string value)
        {
            m_pMessage.Header.Insert(0, m_FieldName, value);
            Refresh();
        }

        /// <summary>
        /// Add new header field on the bottom of the whole header.
        /// </summary>
        /// <param name="value">Header field value.</param>
        public void Add(string value)
        {
            m_pMessage.Header.Add(m_FieldName, value);
            Refresh();
        }

        /// <summary>
        /// Removes all specified header fields with their values.
        /// </summary>
        public void RemoveAll()
        {
            m_pMessage.Header.RemoveAll(m_FieldName);
            m_pFields.Clear();
        }

        /// <summary>
        /// Gets top most header field first value. 
        /// </summary>
        public T GetTopMostValue()
        {
            if (m_pFields.Count > 0)
            {
                return m_pFields[0].Values[0];
            }

            return null;
        }

        /// <summary>
        /// Removes top most header field first value. If value is the last value, 
        /// the whole header field will be removed.
        /// </summary>
        public void RemoveTopMostValue()
        {
            if (m_pFields.Count > 0)
            {
                SIP_MultiValueHF<T> h = m_pFields[0];
                if (h.Count > 1)
                {
                    h.Remove(0);
                }
                else
                {
                    m_pMessage.Header.Remove(m_pFields[0]);
                    m_pFields.Remove(m_pFields[0]);
                }
            }
        }

        /// <summary>
        /// Removes last value. If value is the last value n header field, the whole header field will be removed.
        /// </summary>
        public void RemoveLastValue()
        {
            SIP_MultiValueHF<T> h = m_pFields[m_pFields.Count - 1];
            if (h.Count > 1)
            {
                h.Remove(h.Count - 1);
            }
            else
            {
                m_pMessage.Header.Remove(m_pFields[0]);
                m_pFields.Remove(h);
            }
        }

        /// <summary>
        /// Gets all header field values.
        /// </summary>
        /// <returns></returns>
        public T[] GetAllValues()
        {
            List<T> retVal = new List<T>();
            foreach (SIP_MultiValueHF<T> h in m_pFields)
            {
                foreach (SIP_t_Value v in h.Values)
                {
                    retVal.Add((T) v);
                }
            }

            return retVal.ToArray();
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Refreshes header fields in group from actual header.
        /// </summary>
        private void Refresh()
        {
            m_pFields.Clear();

            foreach (SIP_HeaderField h in m_pMessage.Header)
            {
                if (h.Name.ToLower() == m_FieldName.ToLower())
                {
                    m_pFields.Add((SIP_MultiValueHF<T>) h);
                }
            }
        }

        #endregion
    }
}