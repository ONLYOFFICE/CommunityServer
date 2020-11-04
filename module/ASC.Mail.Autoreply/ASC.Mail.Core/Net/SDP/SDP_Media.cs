/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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