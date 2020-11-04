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
    /// This base class for all SIP data types.
    /// </summary>
    public abstract class SIP_t_Value
    {
        #region Methods

        /// <summary>
        /// Parses single value from specified reader.
        /// </summary>
        /// <param name="reader">Reader what contains </param>
        public abstract void Parse(StringReader reader);

        /// <summary>
        /// Convert this to string value.
        /// </summary>
        /// <returns>Returns this as string value.</returns>
        public abstract string ToStringValue();

        #endregion
    }
}