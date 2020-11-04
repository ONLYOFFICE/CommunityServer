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
    /// vCal email address type. Note this values may be flagged !
    /// </summary>
    public enum EmailAddressType_enum
    {
        /// <summary>
        /// Email address type not specified.
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// Preferred email address.
        /// </summary>
        Preferred = 1,

        /// <summary>
        /// Internet addressing type.
        /// </summary>
        Internet = 2,

        /// <summary>
        /// X.400 addressing type.
        /// </summary>
        X400 = 4,
    }
}