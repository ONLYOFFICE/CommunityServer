/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
