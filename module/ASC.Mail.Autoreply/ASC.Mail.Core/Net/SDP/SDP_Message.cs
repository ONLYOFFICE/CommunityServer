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

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    #endregion

    /// <summary>
    /// Session Description Protocol. Defined in RFC 4566.
    /// </summary>
    public class SDP_Message
    {
        #region Members

        private readonly List<SDP_Attribute> m_pAttributes;
        private readonly List<SDP_Media> m_pMedias;
        private readonly List<SDP_Time> m_pTimes;
        private string m_Origin = "";
        private string m_RepeatTimes = "";
        private string m_SessionDescription = "";
        private string m_SessionName = "";
        private string m_Uri = "";
        private string m_Version = "0";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets version of the Session Description Protocol.
        /// </summary>
        public string Version
        {
            get { return m_Version; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Version can't be null or empty !");
                }

                m_Version = value;
            }
        }

        /// <summary>
        /// Gets originator and session identifier.
        /// </summary>
        public string Originator
        {
            get { return m_Origin; }

            set { m_Origin = value; }
        }

        /// <summary>
        /// Gets or sets textual session name.
        /// </summary>
        public string SessionName
        {
            get { return m_SessionName; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property SessionName can't be null or empty !");
                }

                m_SessionName = value;
            }
        }

        /// <summary>
        /// Gets or sets textual information about the session. This is optional value, null means not specified.
        /// </summary>
        public string SessionDescription
        {
            get { return m_SessionDescription; }

            set { m_SessionDescription = value; }
        }

        /// <summary>
        /// Gets or sets Uniform Resource Identifier. The URI should be a pointer to additional information 
        /// about the session. This is optional value, null means not specified.
        /// </summary>
        public string Uri
        {
            get { return m_Uri; }

            set { m_Uri = value; }
        }

        /// <summary>
        /// Gets or sets connection data. This is optional value if each media part specifies this value,
        /// null means not specified.
        /// </summary>
        public SDP_ConnectionData ConnectionData { get; set; }

        /// <summary>
        /// Gets start and stop times for a session. If Count = 0, t field not written dot SDP data.
        /// </summary>
        public List<SDP_Time> Times
        {
            get { return m_pTimes; }
        }

        /// <summary>
        /// Gets or sets repeat times for a session. This is optional value, null means not specified.
        /// </summary>
        public string RepeatTimes
        {
            get { return m_RepeatTimes; }

            set { m_RepeatTimes = value; }
        }

        /// <summary>
        /// Gets attributes collection. This is optional value, Count == 0 means not specified.
        /// </summary>
        public List<SDP_Attribute> Attributes
        {
            get { return m_pAttributes; }
        }

        /// <summary>
        /// Gets media parts collection.
        /// </summary>
        public List<SDP_Media> Media
        {
            get { return m_pMedias; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SDP_Message()
        {
            m_pTimes = new List<SDP_Time>();
            m_pAttributes = new List<SDP_Attribute>();
            m_pMedias = new List<SDP_Media>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses SDP from raw data.
        /// </summary>
        /// <param name="data">Raw SDP data.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>data</b> is null reference.</exception>
        public static SDP_Message Parse(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            SDP_Message sdp = new SDP_Message();

            StringReader r = new StringReader(data);

            string line = r.ReadLine();

            //--- Read global fields ---------------------------------------------       
            while (line != null)
            {
                line = line.Trim();

                // We reached to media descriptions
                if (line.ToLower().StartsWith("m"))
                {
                    /*
                        m=  (media name and transport address)
                        i=* (media title)
                        c=* (connection information -- optional if included at session level)
                        b=* (zero or more bandwidth information lines)
                        k=* (encryption key)
                        a=* (zero or more media attribute lines)
                    */

                    SDP_Media media = new SDP_Media();
                    media.MediaDescription = SDP_MediaDescription.Parse(line);
                    sdp.Media.Add(media);
                    line = r.ReadLine();
                    // Pasrse media fields and attributes
                    while (line != null)
                    {
                        line = line.Trim();

                        // Next media descrition, just stop active media description parsing, 
                        // fall through main while, allow next while loop to process it.
                        if (line.ToLower().StartsWith("m"))
                        {
                            break;
                        }
                            // i media title
                        else if (line.ToLower().StartsWith("i"))
                        {
                            media.Title = line.Split(new[] {'='}, 2)[1].Trim();
                        }
                            // c connection information
                        else if (line.ToLower().StartsWith("c"))
                        {
                            media.ConnectionData = SDP_ConnectionData.Parse(line);
                        }
                            // a Attributes
                        else if (line.ToLower().StartsWith("a"))
                        {
                            media.Attributes.Add(SDP_Attribute.Parse(line));
                        }

                        line = r.ReadLine();
                    }
                    break;
                }
                    // v Protocol Version
                else if (line.ToLower().StartsWith("v"))
                {
                    sdp.Version = line.Split(new[] {'='}, 2)[1].Trim();
                }
                    // o Origin
                else if (line.ToLower().StartsWith("o"))
                {
                    sdp.Originator = line.Split(new[] {'='}, 2)[1].Trim();
                }
                    // s Session Name
                else if (line.ToLower().StartsWith("s"))
                {
                    sdp.SessionName = line.Split(new[] {'='}, 2)[1].Trim();
                }
                    // i Session Information
                else if (line.ToLower().StartsWith("i"))
                {
                    sdp.SessionDescription = line.Split(new[] {'='}, 2)[1].Trim();
                }
                    // u URI
                else if (line.ToLower().StartsWith("u"))
                {
                    sdp.Uri = line.Split(new[] {'='}, 2)[1].Trim();
                }
                    // c Connection Data
                else if (line.ToLower().StartsWith("c"))
                {
                    sdp.ConnectionData = SDP_ConnectionData.Parse(line);
                }
                    // t Timing
                else if (line.ToLower().StartsWith("t"))
                {
                    sdp.Times.Add(SDP_Time.Parse(line));
                }
                    // a Attributes
                else if (line.ToLower().StartsWith("a"))
                {
                    sdp.Attributes.Add(SDP_Attribute.Parse(line));
                }

                line = r.ReadLine().Trim();
            }

            return sdp;
        }

        /// <summary>
        /// Stores SDP data to specified file. Note: official suggested file extention is .sdp.
        /// </summary>
        /// <param name="fileName">File name with path where to store SDP data.</param>
        public void ToFile(string fileName)
        {
            File.WriteAllText(fileName, ToStringData());
        }

        /// <summary>
        /// Returns SDP as string data.
        /// </summary>
        /// <returns></returns>
        public string ToStringData()
        {
            StringBuilder retVal = new StringBuilder();

            // v Protocol Version
            retVal.AppendLine("v=" + Version);
            // o Origin
            if (!string.IsNullOrEmpty(Originator))
            {
                retVal.AppendLine("o=" + Originator);
            }
            // s Session Name
            if (!string.IsNullOrEmpty(SessionName))
            {
                retVal.AppendLine("s=" + SessionName);
            }
            // i Session Information
            if (!string.IsNullOrEmpty(SessionDescription))
            {
                retVal.AppendLine("i=" + SessionDescription);
            }
            // u URI
            if (!string.IsNullOrEmpty(Uri))
            {
                retVal.AppendLine("u=" + Uri);
            }
            // c Connection Data
            if (ConnectionData != null)
            {
                retVal.Append(ConnectionData.ToValue());
            }
            // t Timing
            foreach (SDP_Time time in Times)
            {
                retVal.Append(time.ToValue());
            }
            // a Attributes
            foreach (SDP_Attribute attribute in Attributes)
            {
                retVal.Append(attribute.ToValue());
            }
            // m media description(s)
            foreach (SDP_Media media in Media)
            {
                retVal.Append(media.ToValue());
            }

            return retVal.ToString();
        }

        #endregion
    }
}