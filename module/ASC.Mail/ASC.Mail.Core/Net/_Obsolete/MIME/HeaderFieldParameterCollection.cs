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

namespace ASC.Mail.Net.Mime
{
    #region usings

    using System;
    using System.Collections;

    #endregion

    /// <summary>
    /// Header field parameters collection.
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public class HeaderFieldParameterCollection : IEnumerable
    {
        #region Members

        private readonly ParametizedHeaderField m_pHeaderField;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="headerField">Header field.</param>
        internal HeaderFieldParameterCollection(ParametizedHeaderField headerField)
        {
            m_pHeaderField = headerField;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets header field parameters count in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pHeaderField.ParseParameters().Count; }
        }

        /// <summary>
        /// Gets or sets specified parameter value.
        /// </summary>
        public string this[string parameterName]
        {
            get
            {
                parameterName = parameterName.ToLower();

                Hashtable parameters = m_pHeaderField.ParseParameters();
                if (!parameters.ContainsKey(parameterName))
                {
                    throw new Exception("Specified parameter '" + parameterName + "' doesn't exist !");
                }
                else
                {
                    return (string) parameters[parameterName];
                }
            }

            set
            {
                parameterName = parameterName.ToLower();

                Hashtable parameters = m_pHeaderField.ParseParameters();
                if (parameters.ContainsKey(parameterName))
                {
                    parameters[parameterName] = value;

                    m_pHeaderField.StoreParameters(m_pHeaderField.Value, parameters);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new header field parameter with specified name and value to the end of the collection.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterValue">Parameter value.</param>
        public void Add(string parameterName, string parameterValue)
        {
            parameterName = parameterName.ToLower();

            Hashtable parameters = m_pHeaderField.ParseParameters();
            if (!parameters.ContainsKey(parameterName))
            {
                parameters.Add(parameterName, parameterValue);

                m_pHeaderField.StoreParameters(m_pHeaderField.Value, parameters);
            }
            else
            {
                throw new Exception("Header field '" + m_pHeaderField.Name + "' parameter '" + parameterName +
                                    "' already exists !");
            }
        }

        /// <summary>
        /// Removes specified header field parameter from the collection.
        /// </summary>
        /// <param name="parameterName">The name of the header field parameter to remove.</param>
        public void Remove(string parameterName)
        {
            parameterName = parameterName.ToLower();

            Hashtable parameters = m_pHeaderField.ParseParameters();
            if (!parameters.ContainsKey(parameterName))
            {
                parameters.Remove(parameterName);

                m_pHeaderField.StoreParameters(m_pHeaderField.Value, parameters);
            }
        }

        /// <summary>
        /// Clears the collection of all header field parameters.
        /// </summary>
        public void Clear()
        {
            Hashtable parameters = m_pHeaderField.ParseParameters();
            parameters.Clear();
            m_pHeaderField.StoreParameters(m_pHeaderField.Value, parameters);
        }

        /// <summary>
        /// Gets if collection contains specified parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <returns></returns>
        public bool Contains(string parameterName)
        {
            parameterName = parameterName.ToLower();

            Hashtable parameters = m_pHeaderField.ParseParameters();
            return parameters.ContainsKey(parameterName);
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            Hashtable parameters = m_pHeaderField.ParseParameters();
            HeaderFieldParameter[] retVal = new HeaderFieldParameter[parameters.Count];
            int i = 0;
            foreach (DictionaryEntry entry in parameters)
            {
                retVal[i] = new HeaderFieldParameter(entry.Key.ToString(), entry.Value.ToString());
                i++;
            }

            return retVal.GetEnumerator();
        }

        #endregion
    }
}