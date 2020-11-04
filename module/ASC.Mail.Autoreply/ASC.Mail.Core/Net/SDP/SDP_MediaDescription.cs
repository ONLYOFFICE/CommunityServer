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

    #endregion

    /// <summary>
    /// A SDP_MediaDescription represents an <B>m=</B> SDP message field. Defined in RFC 4566 5.14. Media Descriptions.
    /// </summary>
    public class SDP_MediaDescription
    {
        #region Members

        private string m_MediaFormat = "";
        private string m_MediaType = "";
        private int m_NumberOfPorts = 1;
        private int m_Port;
        private string m_Protocol = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets meadia type. Currently defined media are "audio", "video", "text", 
        /// "application", and "message", although this list may be extended in the future.
        /// </summary>
        public string MediaType
        {
            get { return m_MediaType; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Protocol can't be null or empty !");
                }

                m_MediaType = value;
            }
        }

        /// <summary>
        /// Gets or sets the transport port to which the media stream is sent.
        /// </summary>
        public int Port
        {
            get { return m_Port; }

            set { m_Port = value; }
        }

        /// <summary>
        /// Gets or sets number of continuos media ports.
        /// </summary>
        public int NumberOfPorts
        {
            get { return m_NumberOfPorts; }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("Property NumberOfPorts must be >= 1 !");
                }

                m_NumberOfPorts = value;
            }
        }

        /// <summary>
        /// Gets or sets transport protocol. Currently known protocols: UDP;RTP/AVP;RTP/SAVP.
        /// </summary>
        public string Protocol
        {
            get { return m_Protocol; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Protocol cant be null or empty !");
                }

                m_Protocol = value;
            }
        }

        /// <summary>
        /// Gets or sets media format description. The interpretation of the media 
        /// format depends on the value of the "proto" sub-field.
        /// </summary>
        public string MediaFormatDescription
        {
            get { return m_MediaFormat; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentException("Property Protocol cant be null !");
                }

                m_MediaFormat = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses media from "m" SDP message field.
        /// </summary>
        /// <param name="mValue">"m" SDP message field.</param>
        /// <returns></returns>
        public static SDP_MediaDescription Parse(string mValue)
        {
            SDP_MediaDescription media = new SDP_MediaDescription();

            // m=<media> <port>/<number of ports> <proto> <fmt> ...
            StringReader r = new StringReader(mValue);
            r.QuotedReadToDelimiter('=');

            //--- <media> ------------------------------------------------------------
            string word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("SDP message \"m\" field <media> value is missing !");
            }
            media.m_MediaType = word;

            //--- <port>/<number of ports> -------------------------------------------
            word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("SDP message \"m\" field <port> value is missing !");
            }
            if (word.IndexOf('/') > -1)
            {
                string[] port_nPorts = word.Split('/');
                media.m_Port = Convert.ToInt32(port_nPorts[0]);
                media.m_NumberOfPorts = Convert.ToInt32(port_nPorts[1]);
            }
            else
            {
                media.m_Port = Convert.ToInt32(word);
                media.m_NumberOfPorts = 1;
            }

            //--- <proto> --------------------------------------------------------------
            word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("SDP message \"m\" field <proto> value is missing !");
            }
            media.m_Protocol = word;

            //--- <fmt> ----------------------------------------------------------------
            word = r.ReadWord();
            if (word == null)
            {
                media.m_MediaFormat = "";
            }
            else
            {
                media.m_MediaFormat = word;
            }

            return media;
        }

        /// <summary>
        /// Converts this to valid media string.
        /// </summary>
        /// <returns></returns>
        public string ToValue()
        {
            // m=<media> <port>/<number of ports> <proto> <fmt> ...

            return "m=" + MediaType + " " + Port + "/" + NumberOfPorts + " " + Protocol + " " +
                   MediaFormatDescription + "\r\n";
        }

        #endregion
    }
}