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