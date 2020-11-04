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


namespace ASC.Mail.Net.Mime.vCard
{
    /// <summary>
    /// vCal delivery address type. Note this values may be flagged !
    /// </summary>
    public enum DeliveryAddressType_enum
    {
        /// <summary>
        /// Delivery address type not specified.
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// Preferred delivery address.
        /// </summary>
        Preferred = 1,

        /// <summary>
        /// Domestic delivery address.
        /// </summary>
        Domestic = 2,

        /// <summary>
        /// International delivery address.
        /// </summary>
        Ineternational = 4,

        /// <summary>
        /// Postal delivery address.
        /// </summary>
        Postal = 8,

        /// <summary>
        /// Parcel delivery address.
        /// </summary>
        Parcel = 16,

        /// <summary>
        /// Delivery address for a residence.
        /// </summary>
        Home = 32,

        /// <summary>
        /// Address for a place of work.
        /// </summary>
        Work = 64,
    }
}