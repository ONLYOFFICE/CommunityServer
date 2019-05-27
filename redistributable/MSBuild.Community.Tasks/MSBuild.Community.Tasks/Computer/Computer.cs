#region Copyright © 2007 Pinal Patel. All rights reserved.
/*
Copyright © 2007 Pinal Patel. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Provides information about the build computer.
    /// </summary>
    /// <example>Get build computer information.
    /// <code><![CDATA[
    /// <Computer>
    ///   <Output TaskParameter="Name" PropertyName="BuildMachineName" />
    ///   <Output TaskParameter="IPAddress" PropertyName="BuildMachineIPAddress" />
    ///   <Output TaskParameter="IPAddressV4" PropertyName="BuildMachineIPAddressV4" />
    ///   <Output TaskParameter="OSPlatform" PropertyName="BuildMachineOSPlatform" />
    ///   <Output TaskParameter="OSVersion" PropertyName="BuildMachineOSVersion" />
    /// </Computer>
    /// ]]></code>
    /// </example>
    public class Computer : Task
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Computer"/> class.
        /// </summary>        
        public Computer()
        {
        }
        #endregion Constructor

        #region Output Parameters
        private string _name = "";
        /// <summary>
        /// Gets the host name of the build computer.
        /// </summary>
        [Output]
        public string Name
        {
            get { return _name; }
        }

        private string _ipAddress = "";
        /// <summary>
        /// Gets the IP address of the build computer.
        /// </summary>
        [Output]
        public string IPAddress
        {
            get { return _ipAddress; }
        }

        private string _ipAddressV4 = "";
        /// <summary>
        /// Gets the IP v4 address of the build computer.
        /// </summary>
        [Output]
        public string IPAddressV4
        {
            get { return _ipAddressV4; }
        }

        private string _osPlatform = "";
        /// <summary>
        /// Gets the platform identifier of the build computer's operating system .
        /// </summary>
        [Output]
        public string OSPlatform
        {
            get { return _osPlatform; }
        }

        private string _osVersion = "";
        /// <summary>
        /// Gets the version number of the build computer's operating system.
        /// </summary>
        [Output]
        public string OSVersion
        {
            get { return _osVersion; }
        }
        #endregion Output Parameters

        #region Task Overrides
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the task ran successfully; otherwise <see langword="false"/>.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                _name = Dns.GetHostName();
                IPAddress[] ipAddresses = Dns.GetHostAddresses(_name);
                IPAddress ipAddress = ipAddresses.FirstOrDefault();
                _ipAddress = ipAddress != null ? ipAddress.ToString() : "";
                IPAddress ipAddressV4 = ipAddresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                _ipAddressV4 = ipAddressV4 != null ? ipAddressV4.ToString() : "";
                _osPlatform = Environment.OSVersion.Platform.ToString();
                _osVersion = Environment.OSVersion.Version.ToString(2);

                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }
        #endregion Task Overrides
    }
}
