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


using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SIP.Stack
{
    /// <summary>
    /// This class represents remote SIP target info.
    /// </summary>
    public class SIP_Target
    {
        private SIP_EndPointInfo m_pLocalEndPoint = null;
        private string           m_Transport      = SIP_Transport.UDP;
        private string           m_Host           = "";
        private int              m_Port           = 5060;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="transport">Transport to use.</param>
        /// <param name="host">Host name or IP.</param>
        /// <param name="port">Host port.</param>
        public SIP_Target(string transport,string host,int port)
        {
            m_Transport = transport;
            m_Host      = host;
            m_Port      = port;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="destination">Remote destination URI.</param>
        public SIP_Target(SIP_Uri destination) : this(null,destination)
        {            
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="localEP">Local end point to use to connect remote destination.</param>
        /// <param name="destination">Remote destination URI.</param>
        public SIP_Target(SIP_EndPointInfo localEP,SIP_Uri destination)
        {
            m_pLocalEndPoint = localEP;

            if(destination.Param_Transport != null){
                m_Transport = destination.Param_Transport;
            }
            else{
                // If SIPS, use always TLS.
                if(destination.IsSecure){
                    m_Transport = SIP_Transport.TLS;
                }
                else{
                    m_Transport = SIP_Transport.UDP;
                }
            }
            m_Host = destination.Host;
            if(destination.Port != -1){
                m_Port = destination.Port;
            }
            else{
                if(m_Transport == SIP_Transport.TLS){
                    m_Port = 5061;
                }
                else{
                    m_Port = 5060;
                }
            }
        }


        #region Properties Implementation

        /// <summary>
        /// Gets local destination to use to connect remote destination. 
        /// Value null means not specified. NOTE This is used only by UDP.
        /// </summary>
        public SIP_EndPointInfo LocalEndPoint
        {
            get{ return m_pLocalEndPoint; }
        }
        
        /// <summary>
        /// Gets transport to use.
        /// </summary>
        public string Transport
        {
            get{ return m_Transport; }
        }

        /// <summary>
        /// Gets remote host name or IP address.
        /// </summary>
        public string Host
        {
            get{ return m_Host; }
        }

        /// <summary>
        /// Gets remote host port.
        /// </summary>
        public int Port
        {
            get{ return m_Port; }
        }

        #endregion

    }
}
