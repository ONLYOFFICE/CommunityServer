/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// This class represents MIME headers provider.
    /// </summary>
    public class MIME_h_Provider
    {
        #region Members

        private readonly Dictionary<string, Type> m_pHeadrFields;
        private Type m_pDefaultHeaderField;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets default header field what is used to reperesent unknown header fields.
        /// </summary>
        /// <remarks>This property value value must be based on <see cref="MIME_h"/> class.</remarks>
        /// <exception cref="ArgumentNullException">Is raised when null reference passed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid value is passed.</exception>
        public Type DefaultHeaderField
        {
            get { return m_pDefaultHeaderField; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("DefaultHeaderField");
                }
                if (!value.GetType().IsSubclassOf(typeof (MIME_h)))
                {
                    throw new ArgumentException(
                        "Property 'DefaultHeaderField' value must be based on MIME_h class.");
                }

                m_pDefaultHeaderField = value;
            }
        }

        /// <summary>
        /// Gets header fields parsers collection.
        /// </summary>
        public Dictionary<string, Type> HeaderFields
        {
            get { return m_pHeadrFields; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MIME_h_Provider()
        {
            m_pDefaultHeaderField = typeof (MIME_h_Unstructured);

            m_pHeadrFields = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase);
            m_pHeadrFields.Add("content-type", typeof (MIME_h_ContentType));
            m_pHeadrFields.Add("content-disposition", typeof (MIME_h_ContentDisposition)); //BUG: was there
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses specified header field.
        /// </summary>
        /// <param name="field">Header field string (Name: value).</param>
        /// <returns>Returns parsed header field.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>field</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when header field parsing errors.</exception>
        public MIME_h Parse(string field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            MIME_h headerField = null;
            string[] name_value = field.Split(new[] {':'}, 2);
            string name = name_value[0].Trim().ToLowerInvariant();
            if (name == string.Empty)
            {
                throw new ParseException("Invalid header field value '" + field + "'.");
            }
            else
            {
                try
                {
                    if (m_pHeadrFields.ContainsKey(name))
                    {
                        headerField =
                            (MIME_h)
                            m_pHeadrFields[name].GetMethod("Parse").Invoke(null, new object[] {field});
                    }
                    else
                    {
                        headerField =
                            (MIME_h)
                            m_pDefaultHeaderField.GetMethod("Parse").Invoke(null, new object[] {field});
                    }
                }
                catch (Exception x)
                {
                    headerField = new MIME_h_Unparsed(field, x.InnerException);
                }
            }

            return headerField;
        }

        #endregion
    }
}