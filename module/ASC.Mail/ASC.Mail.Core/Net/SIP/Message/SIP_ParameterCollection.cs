/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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