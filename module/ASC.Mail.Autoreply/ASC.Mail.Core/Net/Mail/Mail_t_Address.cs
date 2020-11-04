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


namespace ASC.Mail.Net.Mail
{
    #region usings

    using MIME;

    #endregion

    /// <summary>
    /// This class represents RFC 5322 3.4 Address class. 
    /// This class is base class for <see cref="Mail_t_Mailbox">mailbox address</see> and <see cref="Mail_t_Group">group address</see>.
    /// </summary>
    public abstract class Mail_t_Address
    {
        #region Methods

        /// <summary>
        /// Returns address as string value.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <returns>Returns address as string value.</returns>
        public abstract string ToString(MIME_Encoding_EncodedWord wordEncoder);

        #endregion
    }
}