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
    /// This class represents known SIP request methods.
    /// </summary>
    public class SIP_Methods
    {
        #region Constants

        /// <summary>
        /// ACK method. Defined in RFC 3261.
        /// </summary>
        public const string ACK = "ACK";

        /// <summary>
        /// BYE method. Defined in RFC 3261.
        /// </summary>
        public const string BYE = "BYE";

        /// <summary>
        /// CANCEL method. Defined in RFC 3261.
        /// </summary>
        public const string CANCEL = "CANCEL";

        /// <summary>
        /// INFO method. Defined in RFC 2976.
        /// </summary>
        public const string INFO = "INFO";

        /// <summary>
        /// INVITE method. Defined in RFC 3261.
        /// </summary>
        public const string INVITE = "INVITE";

        /// <summary>
        /// MESSAGE method. Defined in RFC 3428.
        /// </summary>
        public const string MESSAGE = "MESSAGE";

        /// <summary>
        /// NOTIFY method. Defined in RFC 3265.
        /// </summary>
        public const string NOTIFY = "NOTIFY";

        /// <summary>
        /// OPTIONS method. Defined in RFC 3261.
        /// </summary>
        public const string OPTIONS = "OPTIONS";

        /// <summary>
        /// PRACK method. Defined in RFC 3262.
        /// </summary>
        public const string PRACK = "PRACK";

        /// <summary>
        /// PUBLISH method. Defined in RFC 3903.
        /// </summary>
        public const string PUBLISH = "PUBLISH";

        /// <summary>
        /// REFER method. Defined in RFC 3515.
        /// </summary>
        public const string REFER = "REFER";

        /// <summary>
        /// REGISTER method. Defined in RFC 3261.
        /// </summary>
        public const string REGISTER = "REGISTER";

        /// <summary>
        /// SUBSCRIBE method. Defined in RFC 3265.
        /// </summary>
        public const string SUBSCRIBE = "SUBSCRIBE";

        /// <summary>
        /// UPDATE method. Defined in RFC 3311.
        /// </summary>
        public const string UPDATE = "UPDATE";

        #endregion
    }
}