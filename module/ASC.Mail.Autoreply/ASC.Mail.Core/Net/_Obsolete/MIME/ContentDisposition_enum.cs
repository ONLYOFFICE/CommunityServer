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


namespace ASC.Mail.Net.Mime
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Rfc 2183 Content-Disposition.
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public enum ContentDisposition_enum
    {
        /// <summary>
        /// Content is attachment.
        /// </summary>
        Attachment = 0,

        /// <summary>
        /// Content is embbed resource.
        /// </summary>
        Inline = 1,

        /// <summary>
        /// Content-Disposition header field isn't available or isn't written to mime message.
        /// </summary>
        NotSpecified = 30,

        /// <summary>
        /// Content is unknown.
        /// </summary>
        Unknown = 40
    }
}