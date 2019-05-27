/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


namespace ASC.Mail.Net.Mime.vCard
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// vCard structure item.
    /// </summary>
    public class Item
    {
        #region Members

        private readonly string m_Name = "";
        private string m_Parameters = "";
        private string m_Value = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Item name.</param>
        /// <param name="parameters">Item parameters.</param>
        /// <param name="value">Item encoded value value.</param>
        internal Item(string name, string parameters, string value)
        {
            m_Name = name;
            m_Parameters = parameters;
            m_Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets item decoded value. If param string specifies Encoding and/or Charset, 
        /// item.Value will be decoded accordingly.
        /// </summary>
        public string DecodedValue
        {
            /* RFC 2426 vCrad 5. Differences From vCard v2.1              
                The CRLF character sequence in a text type value is specified 
                with the backslash character sequence "\n" or "\N".
             
                Any COMMA or SEMICOLON in a text type value must be backslash escaped.
            */

            get
            {
                string data = m_Value;
                string encoding = null;
                string charset = null;
                string[] parameters = m_Parameters.ToLower().Split(';');
                foreach (string parameter in parameters)
                {
                    string[] name_value = parameter.Split('=');
                    if (name_value[0] == "encoding" && name_value.Length > 1)
                    {
                        encoding = name_value[1];
                    }
                    else if (name_value[0] == "charset" && name_value.Length > 1)
                    {
                        charset = name_value[1];
                    }
                }

                // Encoding specified, decode data.
                if (encoding != null)
                {
                    if (encoding == "quoted-printable")
                    {
                        data = Core.QuotedPrintableDecode(data,Encoding.Default);
                    }
                    else if (encoding == "b")
                    {
                        data = Encoding.Default.GetString(Core.Base64Decode(Encoding.Default.GetBytes(data)));
                    }
                    else
                    {
                        throw new Exception("Unknown data encoding '" + encoding + "' !");
                    }
                }
                // Charset specified, convert data to specified charset.
                if (charset != null)
                {
                    data = Encoding.GetEncoding(charset).GetString(Encoding.Default.GetBytes(data));
                }

                // FIX ME: this must be done with structured fields
                //data = data.Replace("\\n","\r\n");
                //data = TextUtils.UnEscapeString(data); Messes up structured fields

                return data;
            }
        }

        /// <summary>
        /// Gest item name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets or sets item parameters.
        /// </summary>
        public string ParametersString
        {
            get { return m_Parameters; }

            set { m_Parameters = value; }
        }

        /// <summary>
        /// Gets or sets item encoded value. NOTE: If you set this property value, you must encode data 
        /// by yourself and also set right ENCODING=encoding; and CHARSET=charset; prameter in item.ParametersString !!!
        /// Normally use method item.SetDecodedStringValue method instead, this does all you need.
        /// </summary>
        public string Value
        {
            get { return m_Value; }

            set { m_Value = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets item decoded value. Value will be encoded as needed and stored to item.Value property.
        /// Also property item.ParametersString is updated to reflect right encoding(always base64, required by rfc) and charset (utf-8).
        /// </summary>
        /// <param name="value"></param>
        public void SetDecodedValue(string value)
        {
            /* RFC 2426 vCrad 5. Differences From vCard v2.1
                The QUOTED-PRINTABLE inline encoding has been eliminated.
                Only the "B" encoding of [RFC 2047] is an allowed value for
                the ENCODING parameter.
              
                The CRLF character sequence in a text type value is specified 
                with the backslash character sequence "\n" or "\N".
             
                Any COMMA or SEMICOLON in a text type value must be backslash escaped.
            */

            // FIX ME: this must be done with structured fields
            //value = value.Replace("\r\n","\n").Replace("\n","\\n");
            //value = TextUtils.EscapeString(value,new char[]{',',';'});

            bool needEncode = false;
            if (!Core.IsAscii(value))
            {
                needEncode = true;
            }

            if (needEncode)
            {
                // Remove encoding and charset parameters
                string newParmString = "";
                string[] parameters = m_Parameters.ToLower().Split(';');
                foreach (string parameter in parameters)
                {
                    string[] name_value = parameter.Split('=');
                    if (name_value[0] == "encoding" || name_value[0] == "charset") {}
                    else if (parameter.Length > 0)
                    {
                        newParmString += parameter + ";";
                    }
                }
                // Add encoding parameter
                newParmString += "ENCODING=b;CHARSET=utf-8";

                ParametersString = newParmString;
                Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            }
            else
            {
                Value = value;
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Converts item to vCal item string.
        /// </summary>
        /// <returns></returns>
        internal string ToItemString()
        {
            if (m_Parameters.Length > 0)
            {
                return m_Name + ";" + m_Parameters + ":" + MimeUtils.FoldData(m_Value);
            }
            else
            {
                return m_Name + ":" + MimeUtils.FoldData(m_Value);
            }
        }

        #endregion
    }
}