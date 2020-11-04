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
    /// <summary>
    /// Represent the method what will handle Error event.
    /// </summary>
    /// <param name="sender">Delegate caller.</param>
    /// <param name="e">Event data.</param>
    public delegate void ErrorEventHandler(object sender, Error_EventArgs e);

    /// <summary>
    /// To be supplied.
    /// </summary>
    public delegate void LogEventHandler(object sender, Log_EventArgs e);

    /// <summary>
    /// Represents the method that will handle the <see href="LumiSoftMailServerSMTPSMTP_ServerValidateIPAddressFieldOrEvent.html">SMTP_Server.ValidateIPAddress</see> and <see href="LumiSoftMailServerPOP3POP3_ServerValidateIPAddressFieldOrEvent.html">POP3_Server.ValidateIPAddress</see>event.
    /// </summary>
    /// <param name="sender">The source of the event. </param>
    /// <param name="e">A <see href="LumiSoftMailServerValidateIP_EventArgs.html">ValidateIP_EventArgs</see> that contains the event data.</param>
    public delegate void ValidateIPHandler(object sender, ValidateIP_EventArgs e);
}