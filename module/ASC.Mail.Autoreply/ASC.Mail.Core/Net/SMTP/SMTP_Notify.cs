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


namespace ASC.Mail.Net.SMTP
{
    /// <summary>
    /// This value implements SMTP Notify value. Defined in RFC 1891.
    /// </summary>
    public enum SMTP_Notify
    {
        /// <summary>
        /// Notify value not specified.
        /// </summary>
        /// <remarks>
        /// For compatibility with SMTP clients that do not use the NOTIFY
        /// facility, the absence of a NOTIFY parameter in a RCPT command may be
        /// interpreted as either NOTIFY=FAILURE or NOTIFY=FAILURE,DELAY.
        /// </remarks>
        NotSpecified = 0,

        /// <summary>
        /// DSN should not be returned to the sender under any conditions.
        /// </summary>
        Never = 0xFF,

        /// <summary>
        /// DSN should be sent on successful delivery.
        /// </summary>
        Success = 2,

        /// <summary>
        /// DSN should be sent on delivery failure.
        /// </summary>
        Failure = 4,

        /// <summary>
        /// This value indicates the sender's willingness to receive "delayed" DSNs.
        /// </summary>
        Delay = 8,
    }
}