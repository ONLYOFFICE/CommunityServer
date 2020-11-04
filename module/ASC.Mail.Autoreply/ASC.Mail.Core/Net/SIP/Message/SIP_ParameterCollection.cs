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

    using System;
    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// This class represents SIP value parameters collection.
    /// </summary>
    public class SIP_ParameterCollection : IEnumerable
    {
        #region Members

        private readonly List<SIP_Parameter> m_pCollection;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_ParameterCollection()
        {
            m_pCollection = new List<SIP_Parameter>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets parameters count in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pCollection.Count; }
        }

        /// <summary>
        /// Gets specified parameter from collection. Returns null if parameter with specified name doesn't exist.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <returns>Returns parameter with specified name or null if not found.</returns>
        public SIP_Parameter this[string name]
        {
            get
            {
                foreach (SIP_Parameter parameter in m_pCollection)
                {
                    if (parameter.Name.ToLower() == name.ToLower())
                    {
                        return parameter;
                    }
                }

                return null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds new parameter to the collection.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>name</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when 'name' is '' or parameter with specified name 
        /// already exists in the collection.</exception>
        public void Add(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (Contains(name))
            {
                throw new ArgumentException(
                    "Prameter '' with specified name already exists in the collection !");
            }

            m_pCollection.Add(new SIP_Parameter(name, value));
        }

        /// <summary>
        /// Adds or updates specified parameter value.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        public void Set(string name, string value)
        {
            if (Contains(name))
            {
                this[name].Value = value;
            }
            else
            {
                Add(name, value);
            }
        }

        /// <summary>
        /// Removes all parameters from the collection.
        /// </summary>
        public void Clear()
        {
            m_pCollection.Clear();
        }

        /// <summary>
        /// Removes specified parameter from the collection.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        public void Remove(string name)
        {
            SIP_Parameter parameter = this[name];
            if (parameter != null)
            {
                m_pCollection.Remove(parameter);
            }
        }

        /// <summary>
        /// Checks if the collection contains parameter with the specified name.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <returns>Returns true if collection contains specified parameter.</returns>
        public bool Contains(string name)
        {
            SIP_Parameter parameter = this[name];
            if (parameter != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pCollection.GetEnumerator();
        }

        #endregion
    }
}