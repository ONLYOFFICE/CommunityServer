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


namespace ASC.Mail.Net
{
    #region usings

    using System;
    using System.Text;
    using MIME;
    using SIP.Message;

    #endregion

    /// <summary>
    /// Implements SIP-URI. Defined in 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     SIP-URI  = "sip:" [ userinfo ] hostport uri-parameters [ headers ]
    ///     SIPS-URI = "sips:" [ userinfo ] hostport uri-parameters [ headers ]
    ///     userinfo = ( user / telephone-subscriber ) [ ":" password ] "@")
    ///     hostport = host [ ":" port ]
    ///     host     = hostname / IPv4address / IPv6reference
    /// </code>
    /// </remarks>
    public class SIP_Uri : AbsoluteUri
    {
        #region Members

        private readonly SIP_ParameterCollection m_pParameters;
        private string m_Header;
        private string m_Host = "";
        private int m_Port = -1;
        private string m_User;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_Uri()
        {
            m_pParameters = new SIP_ParameterCollection();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets address from SIP URI. Examples: ivar@lumisoft.ee,ivar@195.222.10.1.
        /// </summary>
        public string Address
        {
            get { return m_User + "@" + m_Host; }
        }

        /// <summary>
        /// Gets or sets header.
        /// </summary>
        public string Header
        {
            get { return m_Header; }

            set { m_Header = value; }
        }

        /// <summary>
        /// Gets or sets host name or IP.
        /// </summary>
        public string Host
        {
            get { return m_Host; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Host value can't be null or '' !");
                }

                m_Host = value;
            }
        }

        /// <summary>
        /// Gets host with optional port. If port specified returns Host:Port, otherwise Host.
        /// </summary>
        public string HostPort
        {
            get
            {
                if (m_Port == -1)
                {
                    return m_Host;
                }
                else
                {
                    return m_Host + ":" + m_Port;
                }
            }
        }

        /// <summary>
        /// Gets or sets if secure SIP. If true then sips: uri, otherwise sip: uri.
        /// </summary>
        public bool IsSecure { get; set; }

        /// <summary>
        /// Gets or sets 'cause' parameter value. Value -1 means not specified.
        /// Cause is a URI parameter that is used to indicate the service that
        /// the User Agent Server (UAS) receiving the message should perform.
        /// Defined in RFC 4458.
        /// </summary>
        public int Param_Cause
        {
            get
            {
                SIP_Parameter parameter = Parameters["cause"];
                if (parameter != null)
                {
                    return Convert.ToInt32(parameter.Value);
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (value == -1)
                {
                    Parameters.Remove("cause");
                }
                else
                {
                    Parameters.Set("cause", value.ToString());
                }
            }
        }

        /// <summary>
        /// Gets or sets 'comp' parameter value. Value null means not specified. Defined in RFC 3486.
        /// </summary>
        public string Param_Comp
        {
            get
            {
                SIP_Parameter parameter = Parameters["comp"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    Parameters.Remove("comp");
                }
                else
                {
                    Parameters.Set("comp", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'content-type' parameter value. Value null means not specified. Defined in RFC 4240.
        /// </summary>
        public string Param_ContentType
        {
            get
            {
                SIP_Parameter parameter = Parameters["content-type"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    Parameters.Remove("content-type");
                }
                else
                {
                    Parameters.Set("content-type", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'delay' prameter value. Value -1 means not specified. 
        /// Specifies a delay interval between announcement repetitions. The delay is measured in milliseconds.
        /// Defined in RFC 4240.
        /// </summary>
        public int Param_Delay
        {
            get
            {
                SIP_Parameter parameter = Parameters["delay"];
                if (parameter != null)
                {
                    return Convert.ToInt32(parameter.Value);
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (value == -1)
                {
                    Parameters.Remove("delay");
                }
                else
                {
                    Parameters.Set("delay", value.ToString());
                }
            }
        }

        /// <summary>
        /// Gets or sets 'duration' prameter value. Value -1 means not specified. 
        /// Specifies the maximum duration of the announcement. The media server will discontinue 
        /// the announcement and end the call if the maximum duration has been reached. The duration 
        /// is measured in milliseconds. Defined in RFC 4240.
        /// </summary>
        public int Param_Duration
        {
            get
            {
                SIP_Parameter parameter = Parameters["duration"];
                if (parameter != null)
                {
                    return Convert.ToInt32(parameter.Value);
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (value == -1)
                {
                    Parameters.Remove("duration");
                }
                else
                {
                    Parameters.Set("duration", value.ToString());
                }
            }
        }

        /// <summary>
        /// Gets or sets 'locale' prameter value. Specifies the language and optionally country 
        /// variant of the announcement sequence named in the "play=" parameter. Defined in RFC 4240.
        /// </summary>
        public string Param_Locale
        {
            get
            {
                SIP_Parameter parameter = Parameters["locale"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    Parameters.Remove("locale");
                }
                else
                {
                    Parameters.Set("locale", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'lr' parameter. The lr parameter, when present, indicates that the element
        /// responsible for this resource implements the routing mechanisms
        /// specified in this document. Defined in RFC 3261.
        /// </summary>
        public bool Param_Lr
        {
            get
            {
                SIP_Parameter parameter = Parameters["lr"];
                if (parameter != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (!value)
                {
                    Parameters.Remove("lr");
                }
                else
                {
                    Parameters.Set("lr", null);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'maddr' parameter value. Value null means not specified. 
        /// <a style="font-weight: bold; color: red">NOTE: This value is deprecated in since SIP 2.0.</a>
        /// The maddr parameter indicates the server address to be contacted for this user, 
        /// overriding any address derived from the host field. Defined in RFC 3261.
        /// </summary>
        public string Param_Maddr
        {
            get
            {
                SIP_Parameter parameter = Parameters["maddr"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    Parameters.Remove("maddr");
                }
                else
                {
                    Parameters.Set("maddr", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'method' prameter value. Value null means not specified. Defined in RFC 3261.
        /// </summary>
        public string Param_Method
        {
            get
            {
                SIP_Parameter parameter = Parameters["method"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    Parameters.Remove("method");
                }
                else
                {
                    Parameters.Set("method", value);
                }
            }
        }

        //  param[n]           No                    [RFC4240]

        /// <summary>
        /// Gets or sets 'play' parameter value. Value null means not specified. 
        /// Specifies the resource or announcement sequence to be played. Defined in RFC 4240.
        /// </summary>
        public string Param_Play
        {
            get
            {
                SIP_Parameter parameter = Parameters["play"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    Parameters.Remove("play");
                }
                else
                {
                    Parameters.Set("play", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'repeat' parameter value. Value -1 means not specified, value int.MaxValue means 'forever'.
        /// Specifies how many times the media server should repeat the announcement or sequence named by 
        /// the "play=" parameter. Defined in RFC 4240.
        /// </summary>
        public int Param_Repeat
        {
            get
            {
                SIP_Parameter parameter = Parameters["ttl"];
                if (parameter != null)
                {
                    if (parameter.Value.ToLower() == "forever")
                    {
                        return int.MaxValue;
                    }
                    else
                    {
                        return Convert.ToInt32(parameter.Value);
                    }
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (value == -1)
                {
                    Parameters.Remove("ttl");
                }
                else if (value == int.MaxValue)
                {
                    Parameters.Set("ttl", "forever");
                }
                else
                {
                    Parameters.Set("ttl", value.ToString());
                }
            }
        }

        /// <summary>
        /// Gets or sets 'target' parameter value. Value null means not specified. Defined in RFC 4240.
        /// </summary>
        public string Param_Target
        {
            get
            {
                SIP_Parameter parameter = Parameters["target"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    Parameters.Remove("target");
                }
                else
                {
                    Parameters.Set("target", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'transport' parameter value. Value null means not specified. 
        /// The transport parameter determines the transport mechanism to
        /// be used for sending SIP messages. Defined in RFC 3261.
        /// </summary>
        public string Param_Transport
        {
            get
            {
                SIP_Parameter parameter = Parameters["transport"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    Parameters.Remove("transport");
                }
                else
                {
                    Parameters.Set("transport", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'ttl' parameter value. Value -1 means not specified.
        /// <a style="font-weight: bold; color: red">NOTE: This value is deprecated in since SIP 2.0.</a>
        /// The ttl parameter determines the time-to-live value of the UDP
        /// multicast packet and MUST only be used if maddr is a multicast
        /// address and the transport protocol is UDP. Defined in RFC 3261.
        /// </summary>
        public int Param_Ttl
        {
            get
            {
                SIP_Parameter parameter = Parameters["ttl"];
                if (parameter != null)
                {
                    return Convert.ToInt32(parameter.Value);
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (value == -1)
                {
                    Parameters.Remove("ttl");
                }
                else
                {
                    Parameters.Set("ttl", value.ToString());
                }
            }
        }

        /// <summary>
        /// Gets or sets 'user' parameter value. Value null means not specified. Defined in RFC 3261.
        /// </summary>
        public string Param_User
        {
            get
            {
                SIP_Parameter parameter = Parameters["user"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    Parameters.Remove("user");
                }
                else
                {
                    Parameters.Set("user", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'voicexml' parameter value. Value null means not specified. Defined in RFC 4240.
        /// </summary>
        public string Param_Voicexml
        {
            get
            {
                SIP_Parameter parameter = Parameters["voicexml"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    Parameters.Remove("voicexml");
                }
                else
                {
                    Parameters.Set("voicexml", value);
                }
            }
        }

        /// <summary>
        /// Gets URI parameters.
        /// </summary>
        public SIP_ParameterCollection Parameters
        {
            get { return m_pParameters; }
        }

        /// <summary>
        /// Gets or sets host port. Value -1 means not specified.
        /// </summary>
        public int Port
        {
            get { return m_Port; }

            set { m_Port = value; }
        }

        /// <summary>
        /// Gets URI scheme.
        /// </summary>
        public override string Scheme
        {
            get
            {
                if (IsSecure)
                {
                    return "sips";
                }
                else
                {
                    return "sip";
                }
            }
        }

        /// <summary>
        /// Gets or sets user name. Value null means not specified.
        /// </summary>
        public string User
        {
            get { return m_User; }

            set { m_User = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parse SIP or SIPS URI from string value.
        /// </summary>
        /// <param name="value">String URI value.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when <b>value</b> is not valid SIP or SIPS URI.</exception>
        public new static SIP_Uri Parse(string value)
        {
            AbsoluteUri uri = AbsoluteUri.Parse(value);
            if (uri is SIP_Uri)
            {
                return (SIP_Uri) uri;
            }
            else
            {
                throw new ArgumentException("Argument 'value' is not valid SIP or SIPS URI.");
            }
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>Returns true if two objects are equal.</returns>
        public override bool Equals(object obj)
        {
            /* RFC 3261 19.1.4 URI Comparison.
                SIP and SIPS URIs are compared for equality according to the following rules:
                    o  A SIP and SIPS URI are never equivalent.
             
                    o  Comparison of the userinfo of SIP and SIPS URIs is case-
                       sensitive.  This includes userinfo containing passwords or
                       formatted as telephone-subscribers.  Comparison of all other
                       components of the URI is case-insensitive unless explicitly
                       defined otherwise.

                    o  The ordering of parameters and header fields is not significant
                       in comparing SIP and SIPS URIs.

                    o  Characters other than those in the "reserved" set (see RFC 2396
                       [5]) are equivalent to their ""%" HEX HEX" encoding.

                    o  An IP address that is the result of a DNS lookup of a host name
                       does not match that host name.

                    o  For two URIs to be equal, the user, password, host, and port
                       components must match.

                       A URI omitting the user component will not match a URI that
                       includes one.  A URI omitting the password component will not
                       match a URI that includes one.

                       A URI omitting any component with a default value will not
                       match a URI explicitly containing that component with its
                       default value.  For instance, a URI omitting the optional port
                       component will not match a URI explicitly declaring port 5060.
                       The same is true for the transport-parameter, ttl-parameter,
                       user-parameter, and method components.
              
                    o  URI uri-parameter components are compared as follows:
                        -  Any uri-parameter appearing in both URIs must match.

                        -  A user, ttl, or method uri-parameter appearing in only one
                           URI never matches, even if it contains the default value.

                        -  A URI that includes an maddr parameter will not match a URI
                           that contains no maddr parameter.

                        -  All other uri-parameters appearing in only one URI are
                           ignored when comparing the URIs.
            
                    o  URI header components are never ignored.  Any present header
                       component MUST be present in both URIs and match for the URIs
                       to match. 

            */

            if (obj == null)
            {
                return false;
            }
            if (!(obj is SIP_Uri))
            {
                return false;
            }

            SIP_Uri sipUri = (SIP_Uri) obj;

            if (IsSecure && !sipUri.IsSecure)
            {
                return false;
            }

            if (User != sipUri.User)
            {
                return false;
            }

            /*
            if(this.Password != sipUri.Password){
                return false;
            }*/

            if (Host.ToLower() != sipUri.Host.ToLower())
            {
                return false;
            }

            if (Port != sipUri.Port)
            {
                return false;
            }

            // TODO: prameters compare
            // TODO: header fields compare

            return true;
        }

        /// <summary>
        /// Returns the hash code.
        /// </summary>
        /// <returns>Returns the hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Converts SIP_Uri to valid SIP-URI string.
        /// </summary>
        /// <returns>Returns SIP-URI string.</returns>
        public override string ToString()
        {
            // Syntax: sip:/sips: username@host *[;parameter] [?header *[&header]]

            StringBuilder retVal = new StringBuilder();
            if (IsSecure)
            {
                retVal.Append("sips:");
            }
            else
            {
                retVal.Append("sip:");
            }
            if (User != null)
            {
                retVal.Append(User + "@");
            }

            retVal.Append(Host);
            if (Port > -1)
            {
                retVal.Append(":" + Port);
            }

            // Add URI parameters.
            foreach (SIP_Parameter parameter in m_pParameters)
            {
                /*
                 * If value is token value is not quoted(quoted-string).
                 * If value contains `tspecials', value should be represented as quoted-string.
                 * If value is empty string, only parameter name is added.
                */
                if (parameter.Value != null)
                {
                    if (MIME_Reader.IsToken(parameter.Value))
                    {
                        retVal.Append(";" + parameter.Name + "=" + parameter.Value);
                    }
                    else
                    {
                        retVal.Append(";" + parameter.Name + "=" + TextUtils.QuoteString(parameter.Value));
                    }
                }
                else
                {
                    retVal.Append(";" + parameter.Name);
                }
            }

            if (Header != null)
            {
                retVal.Append("?" + Header);
            }

            return retVal.ToString();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Parses SIP_Uri from SIP-URI string.
        /// </summary>
        /// <param name="value">SIP-URI  string.</param>
        /// <returns>Returns parsed SIP_Uri object.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        protected override void ParseInternal(string value)
        {
            // Syntax: sip:/sips: username@host:port *[;parameter] [?header *[&header]]

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            value = Uri.UnescapeDataString(value);

            if (!(value.ToLower().StartsWith("sip:") || value.ToLower().StartsWith("sips:")))
            {
                throw new SIP_ParseException("Specified value is invalid SIP-URI !");
            }

            StringReader r = new StringReader(value);

            // IsSecure
            IsSecure = r.QuotedReadToDelimiter(':').ToLower() == "sips";

            // Get username
            if (r.SourceString.IndexOf('@') > -1)
            {
                User = r.QuotedReadToDelimiter('@');
            }

            // Gets host[:port]
            string[] host_port = r.QuotedReadToDelimiter(new[] {';', '?'}, false).Split(':');
            Host = host_port[0];
            // Optional port specified
            if (host_port.Length == 2)
            {
                Port = Convert.ToInt32(host_port[1]);
            }

            // We have parameters and/or header
            if (r.Available > 0)
            {
                // Get parameters
                string[] parameters = TextUtils.SplitQuotedString(r.QuotedReadToDelimiter('?'), ';');
                foreach (string parameter in parameters)
                {
                    if (parameter.Trim() != "")
                    {
                        string[] name_value = parameter.Trim().Split(new[] {'='}, 2);
                        if (name_value.Length == 2)
                        {
                            Parameters.Add(name_value[0], TextUtils.UnQuoteString(name_value[1]));
                        }
                        else
                        {
                            Parameters.Add(name_value[0], null);
                        }
                    }
                }

                // We have header
                if (r.Available > 0)
                {
                    m_Header = r.ReadToEnd();
                }
            }
        }

        #endregion
    }
}