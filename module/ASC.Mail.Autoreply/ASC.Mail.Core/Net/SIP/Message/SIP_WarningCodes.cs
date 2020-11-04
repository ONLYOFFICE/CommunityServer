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


namespace ASC.Mail.Net.SIP.Message
{
    /// <summary>
    /// SIP Warning Codes. Defined in RFC 3261 27.2.
    /// </summary>
    public class SIP_WarningCodes
    {
        #region Members

        /// <summary>
        /// One or more network protocols contained in the session description are not available.
        /// </summary>
        public const int x300_Incompatible_network_protocol = 300;

        /// <summary>
        /// One or more network address formats contained in the session description are not available.
        /// </summary>
        public const int x301_Incompatible_network_address_formats = 301;

        /// <summary>
        /// One or more transport protocols described in the session description are not available.
        /// </summary>
        public const int x302_Incompatible_network_address_formats = 302;

        /// <summary>
        /// One or more bandwidth measurement units contained in the session description were not understood.
        /// </summary>
        public const int x303_Incompatible_bandwidth_units = 303;

        /// <summary>
        /// One or more media types contained in the session description are not available.
        /// </summary>
        public const int x304_Media_type_not_available = 304;

        /// <summary>
        /// One or more media formats contained in the session description are not available.
        /// </summary>
        public const int x305_Incompatible_media_format = 305;

        /// <summary>
        /// One or more of the media attributes in the session description are not supported.
        /// </summary>
        public const int x306_Attribute_not_understood = 306;

        /// <summary>
        /// A parameter other than those listed above was not understood.
        /// </summary>
        public const int x307_Session_description_parameter_not_understood = 307;

        /// <summary>
        /// The site where the user is located does not support multicast.
        /// </summary>
        public const int x330_Multicast_not_available = 330;

        /// <summary>
        /// The site where the user is located does not support unicast communication 
        /// (usually due to the presence of a firewall).
        /// </summary>
        public const int x331_Unicast_not_available = 331;

        /// <summary>
        /// The bandwidth specified in the session description or defined by the media 
        /// exceeds that known to be available.
        /// </summary>
        public const int x370_Insufficient_bandwidth = 370;

        /// <summary>
        /// The warning text can include arbitrary information to be presented to a human user or logged. 
        /// A system receiving this warning MUST NOT take any automated action.
        /// </summary>
        public const int x399_Miscellaneous_warning = 399;

        #endregion
    }
}