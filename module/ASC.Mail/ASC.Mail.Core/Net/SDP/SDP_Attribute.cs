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


namespace ASC.Mail.Net.SDP
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Implements SDP attribute.
    /// </summary>
    public class SDP_Attribute
    {
        #region Members

        private readonly string m_Name = "";
        private string m_Value = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets attribute name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets or sets attribute value.
        /// </summary>
        public string Value
        {
            get { return m_Value; }

            set { m_Value = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="value">Attribute value.</param>
        public SDP_Attribute(string name, string value)
        {
            m_Name = name;
            Value = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses media from "a" SDP message field.
        /// </summary>
        /// <param name="aValue">"a" SDP message field.</param>
        /// <returns></returns>
        public static SDP_Attribute Parse(string aValue)
        {
            // a=<attribute>
            // a=<attribute>:<value>

            // Remove a=
            StringReader r = new StringReader(aValue);
            r.QuotedReadToDelimiter('=');

            //--- <attribute> ------------------------------------------------------------
            string name = "";
            string word = r.QuotedReadToDelimiter(':');
            if (word == null)
            {
                throw new Exception("SDP message \"a\" field <attribute> name is missing !");
            }
            name = word;

            //--- <value> ----------------------------------------------------------------
            string value = "";
            word = r.ReadToEnd();
            if (word != null)
            {
                value = word;
            }

            return new SDP_Attribute(name, value);
        }

        /// <summary>
        /// Converts this to valid "a" string.
        /// </summary>
        /// <returns></returns>
        public string ToValue()
        {
            // a=<attribute>
            // a=<attribute>:<value>

            // a=<attribute>
            if (string.IsNullOrEmpty(m_Value))
            {
                return "a=" + m_Name + "\r\n";
            }
                // a=<attribute>:<value>
            else
            {
                return "a=" + m_Name + ":" + m_Value + "\r\n";
            }
        }

        #endregion
    }
}