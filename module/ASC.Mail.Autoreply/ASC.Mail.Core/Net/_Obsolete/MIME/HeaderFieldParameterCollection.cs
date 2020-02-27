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