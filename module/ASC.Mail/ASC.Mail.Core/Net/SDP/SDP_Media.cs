/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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

    using System.Collections.Generic;
    using System.Text;

    #endregion

    /// <summary>
    /// SDP media.
    /// </summary>
    public class SDP_Media
    {
        #region Members

        private readonly List<SDP_Attribute> m_pAttributes;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets media description.
        /// </summary>
        public SDP_MediaDescription MediaDescription { get; set; }

        /// <summary>
        /// Gets or sets media title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets connection data. This is optional value if SDP message specifies this value,
        /// null means not specified.
        /// </summary>
        public SDP_ConnectionData ConnectionData { get; set; }

        /// <summary>
        /// Gets or sets media encryption key info.
        /// </summary>
        public string EncryptionKey { get; set; }

        /// <summary>
        /// Gets media attributes collection. This is optional value, Count == 0 means not specified.
        /// </summary>
        public List<SDP_Attribute> Attributes
        {
            get { return m_pAttributes; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SDP_Media()
        {
            m_pAttributes = new List<SDP_Attribute>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts media entity to corresponding media lines. Attributes included.
        /// </summary>
        /// <returns></returns>
        public string ToValue()
        {
            /*
                m=  (media name and transport address)
                i=* (media title)
                c=* (connection information -- optional if included at session level)
                b=* (zero or more bandwidth information lines)
                k=* (encryption key)
                a=* (zero or more media attribute lines)
            */

            StringBuilder retVal = new StringBuilder();

            // m Media description
            if (MediaDescription != null)
            {
                retVal.Append(MediaDescription.ToValue());
            }
            // i media title
            if (!string.IsNullOrEmpty(Title))
            {
                retVal.AppendLine("i=" + Title);
            }
            // c Connection Data
            if (ConnectionData != null)
            {
                retVal.Append(ConnectionData.ToValue());
            }
            // a Attributes
            foreach (SDP_Attribute attribute in Attributes)
            {
                retVal.Append(attribute.ToValue());
            }

            return retVal.ToString();
        }

        #endregion
    }
}