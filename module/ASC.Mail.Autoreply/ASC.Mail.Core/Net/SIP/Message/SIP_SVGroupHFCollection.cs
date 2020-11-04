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


namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Implements same single value header fields group. Group can contain one type header fields only.
    /// This is class is used by Authorization:,Proxy-Authorization:, ... .
    /// </summary>
    public class SIP_SVGroupHFCollection<T> where T : SIP_t_Value
    {
        #region Members

        private readonly string m_FieldName = "";
        private readonly List<SIP_SingleValueHF<T>> m_pFields;
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
        public SIP_SingleValueHF<T>[] HeaderFields
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
        public SIP_SVGroupHFCollection(SIP_Message owner, string fieldName)
        {
            m_pMessage = owner;
            m_FieldName = fieldName;

            m_pFields = new List<SIP_SingleValueHF<T>>();

            Refresh();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds specified header field value to the end of header.
        /// </summary>
        /// <param name="value">Header field value.</param>
        public void Add(string value)
        {
            m_pMessage.Header.Add(m_FieldName, value);
            Refresh();
        }

        /// <summary>
        /// Removes header field from specified index.
        /// </summary>
        /// <param name="index">Index of the header field what to remove. Index is relative ths group.</param>
        public void Remove(int index)
        {
            m_pMessage.Header.Remove(m_pFields[index]);
            m_pFields.RemoveAt(index);
        }

        /// <summary>
        /// Removes specified header field from header.
        /// </summary>
        /// <param name="field">Header field to remove.</param>
        public void Remove(SIP_SingleValueHF<T> field)
        {
            m_pMessage.Header.Remove(field);
            m_pFields.Remove(field);
        }

        /// <summary>
        /// Removes all this gorup header fields from header.
        /// </summary>
        public void RemoveAll()
        {
            foreach (SIP_SingleValueHF<T> h in m_pFields)
            {
                m_pMessage.Header.Remove(h);
            }
            m_pFields.Clear();
        }

        /// <summary>
        /// Gets the first(Top-Most) header field. Returns null if no header fields.
        /// </summary>
        /// <returns>Returns first header field or null if no header fields.</returns>
        public SIP_SingleValueHF<T> GetFirst()
        {
            if (m_pFields.Count > 0)
            {
                return m_pFields[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all header field values.
        /// </summary>
        /// <returns></returns>
        public T[] GetAllValues()
        {
            List<T> retVal = new List<T>();
            foreach (SIP_SingleValueHF<T> hf in m_pFields)
            {
                retVal.Add(hf.ValueX);
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
                    m_pFields.Add((SIP_SingleValueHF<T>) h);
                }
            }
        }

        #endregion
    }
}