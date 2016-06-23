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

    using System.Text;

    #endregion

    /// <summary>
    /// This base class for SIP data types what has parameters support.
    /// </summary>
    public abstract class SIP_t_ValueWithParams : SIP_t_Value
    {
        #region Members

        private readonly SIP_ParameterCollection m_pParameters;

        #endregion

        #region Properties

        /// <summary>
        /// Gets via parameters.
        /// </summary>
        public SIP_ParameterCollection Parameters
        {
            get { return m_pParameters; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_ValueWithParams()
        {
            m_pParameters = new SIP_ParameterCollection();
        }

        #endregion

        /// <summary>
        /// Parses parameters from specified reader. Reader position must be where parameters begin.
        /// </summary>
        /// <param name="reader">Reader from where to read parameters.</param>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        protected void ParseParameters(StringReader reader)
        {
            // Remove all old parameters.
            m_pParameters.Clear();

            // Parse parameters
            while (reader.Available > 0)
            {
                reader.ReadToFirstChar();

                // We have parameter
                if (reader.SourceString.StartsWith(";"))
                {
                    reader.ReadSpecifiedLength(1);
                    string paramString = reader.QuotedReadToDelimiter(new[] {';', ','}, false);
                    if (paramString != "")
                    {
                        string[] name_value = paramString.Split(new[] {'='}, 2);
                        if (name_value.Length == 2)
                        {
                            Parameters.Add(name_value[0], name_value[1]);
                        }
                        else
                        {
                            Parameters.Add(name_value[0], null);
                        }
                    }
                }
                    // Next value
                else if (reader.SourceString.StartsWith(","))
                {
                    break;
                }
                    // Unknown data
                else
                {
                    throw new SIP_ParseException("Unexpected value '" + reader.SourceString + "' !");
                }
            }
        }

        /// <summary>
        /// Convert parameters to valid parameters string.
        /// </summary>
        /// <returns>Returns parameters string.</returns>
        protected string ParametersToString()
        {
            StringBuilder retVal = new StringBuilder();
            foreach (SIP_Parameter parameter in m_pParameters)
            {
                if (!string.IsNullOrEmpty(parameter.Value))
                {
                    retVal.Append(";" + parameter.Name + "=" + parameter.Value);
                }
                else
                {
                    retVal.Append(";" + parameter.Name);
                }
            }

            return retVal.ToString();
        }
    }
}