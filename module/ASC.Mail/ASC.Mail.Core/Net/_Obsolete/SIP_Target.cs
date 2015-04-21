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
