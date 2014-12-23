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

namespace ASC.Mail.Net.FTP.Client
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// FTP client exception.
    /// </summary>
    public class FTP_ClientException : Exception
    {
        #region Members

        private readonly string m_ResponseText = "";
        private readonly int m_StatusCode = 500;

        #endregion

        #region Properties

        /// <summary>
        /// Gets FTP status code.
        /// </summary>
        public int StatusCode
        {
            get { return m_StatusCode; }
        }

        /// <summary>
        /// Gets FTP server response text after status code.
        /// </summary>
        public string ResponseText
        {
            get { return m_ResponseText; }
        }

        /// <summary>
        /// Gets if it is permanent FTP(5xx) error.
        /// </summary>
        public bool IsPermanentError
        {
            get
            {
                if (m_StatusCode >= 500 && m_StatusCode <= 599)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="responseLine">FTP server response line.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>responseLine</b> is null.</exception>
        public FTP_ClientException(string responseLine) : base(responseLine)
        {
            if (responseLine == null)
            {
                throw new ArgumentNullException("responseLine");
            }

            string[] code_text = responseLine.Split(new[] {' '}, 2);
            try
            {
                m_StatusCode = Convert.ToInt32(code_text[0]);
            }
            catch {}
            if (code_text.Length == 2)
            {
                m_ResponseText = code_text[1];
            }
        }

        #endregion
    }
}