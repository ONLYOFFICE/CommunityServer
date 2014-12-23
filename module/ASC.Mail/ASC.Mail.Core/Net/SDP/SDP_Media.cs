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