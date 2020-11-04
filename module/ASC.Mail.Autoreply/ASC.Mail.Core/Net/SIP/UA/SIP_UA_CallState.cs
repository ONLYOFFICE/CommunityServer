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


namespace ASC.Mail.Net.SIP.UA
{
    /// <summary>
    /// This enum specifies SIP UA call states.
    /// </summary>
    public enum SIP_UA_CallState
    {
        /// <summary>
        /// Outgoing call waits to be started.
        /// </summary>
        WaitingForStart,

        /// <summary>
        /// Outgoing calling is in progress.
        /// </summary>
        Calling,

        /// <summary>
        /// Outgoing call remote end party is ringing.
        /// </summary>
        Ringing,

        /// <summary>
        /// Outgoing call remote end pary queued a call.
        /// </summary>
        Queued,

        /// <summary>
        /// Incoming call waits to be accepted.
        /// </summary>
        WaitingToAccept,

        /// <summary>
        /// Call is active.
        /// </summary>
        Active,

        /// <summary>
        /// Call is terminating.
        /// </summary>
        Terminating,

        /// <summary>
        /// Call is terminated.
        /// </summary>
        Terminated,

        /// <summary>
        /// Call has disposed.
        /// </summary>
        Disposed
    }
}