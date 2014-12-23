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

namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// This class represents normal unstructured text header field.
    /// </summary>
    public class MIME_h_Unstructured : MIME_h
    {
        #region Members

        private string m_Name = "";
        private string m_ParseValue;
        private string m_Value = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets if this header field is modified since it has loaded.
        /// </summary>
        /// <remarks>All new added header fields has <b>IsModified = true</b>.</remarks>
        /// <exception cref="ObjectDisposedException">Is riased when this class is disposed and this property is accessed.</exception>
        public override bool IsModified
        {
            get { return m_ParseValue == null; }
        }

        /// <summary>
        /// Gets header field name.
        /// </summary>
        public override string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets or sets header field value.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when when null reference is passed.</exception>
        public string Value
        {
            get { return m_Value; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_Value = value;
                // Reset parse value.
                m_ParseValue = null;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Header field name.</param>
        /// <param name="value">Header field value.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>name</b> or <b>value</b> is null reference.</exception>
        public MIME_h_Unstructured(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == string.Empty)
            {
                throw new ArgumentException("Argument 'name' value must be specified.", "name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            m_Name = name;
            m_Value = value;
        }

        /// <summary>
        /// Internal parser constructor.
        /// </summary>
        private MIME_h_Unstructured() {}

        #endregion

        #region Methods

        /// <summary>
        /// Parses header field from the specified value.
        /// </summary>
        /// <param name="value">Header field value. Header field name must be included. For example: 'Content-Type: text/plain'.</param>
        /// <returns>Returns parsed header field.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when header field parsing errors.</exception>
        public static MIME_h_Unstructured Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            MIME_h_Unstructured retVal = new MIME_h_Unstructured();

            string[] name_value = value.Split(new[] {':'}, 2);
            if (name_value[0].Trim() == string.Empty)
            {
                throw new ParseException("Invalid header field '" + value + "' syntax.");
            }

            retVal.m_Name = name_value[0];
            retVal.m_Value = MIME_Encoding_EncodedWord.DecodeAll(
                     MIME_Utils.UnfoldHeader(name_value.Length == 2 ? name_value[1].TrimStart() : ""));

            retVal.m_ParseValue = value;

            return retVal;
        }

        /// <summary>
        /// Returns header field as string.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="parmetersCharset">Charset to use to encode 8-bit characters. Value null means parameters not encoded.</param>
        /// <returns>Returns header field as string.</returns>
        public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
        {
            if (m_ParseValue != null)
            {
                return m_ParseValue;
            }
            else
            {
                if (wordEncoder != null)
                {
                    return m_Name + ": " + wordEncoder.Encode(m_Value) + "\r\n";
                }
                else
                {
                    return m_Name + ": " + m_Value + "\r\n";
                }
            }
        }

        #endregion
    }
}