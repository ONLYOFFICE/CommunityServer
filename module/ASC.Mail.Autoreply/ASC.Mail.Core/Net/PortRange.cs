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


namespace ASC.Mail.Net
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class holds UDP or TCP port range.
    /// </summary>
    public class PortRange
    {
        #region Members

        private readonly int m_End = 1100;
        private readonly int m_Start = 1000;

        #endregion

        #region Properties

        /// <summary>
        /// Gets start port.
        /// </summary>
        public int Start
        {
            get { return m_Start; }
        }

        /// <summary>
        /// Gets end port.
        /// </summary>
        public int End
        {
            get { return m_End; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="start">Start port.</param>
        /// <param name="end">End port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when any of the aruments value is out of range.</exception>
        public PortRange(int start, int end)
        {
            if (start < 1 || start > 0xFFFF)
            {
                throw new ArgumentOutOfRangeException("Argument 'start' value must be > 0 and << 65 535.");
            }
            if (end < 1 || end > 0xFFFF)
            {
                throw new ArgumentOutOfRangeException("Argument 'end' value must be > 0 and << 65 535.");
            }
            if (start > end)
            {
                throw new ArgumentOutOfRangeException(
                    "Argumnet 'start' value must be >= argument 'end' value.");
            }

            m_Start = start;
            m_End = end;
        }

        #endregion
    }
}