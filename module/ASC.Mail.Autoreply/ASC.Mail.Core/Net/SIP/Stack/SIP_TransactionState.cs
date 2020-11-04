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


namespace ASC.Mail.Net.SIP.Stack
{
    /// <summary>
    /// This enum holds SIP transaction states. Defined in RFC 3261.
    /// </summary>
    public enum SIP_TransactionState
    {
        /// <summary>
        /// Client transaction waits <b>Start</b> method to be called.
        /// </summary>
        WaitingToStart,

        /// <summary>
        /// Calling to recipient. This is used only by INVITE client transaction.
        /// </summary>
        Calling,

        /// <summary>
        /// This is transaction initial state. Used only in Non-INVITE transaction.
        /// </summary>
        Trying,

        /// <summary>
        /// This is INVITE server transaction initial state. Used only in INVITE server transaction.
        /// </summary>
        Proceeding,

        /// <summary>
        /// Transaction has got final response.
        /// </summary>
        Completed,

        /// <summary>
        /// Transation has got ACK from request maker. This is used only by INVITE server transaction.
        /// </summary>
        Confirmed,

        /// <summary>
        /// Transaction has terminated and waits disposing.
        /// </summary>
        Terminated,

        /// <summary>
        /// Transaction has disposed.
        /// </summary>
        Disposed,
    }
}