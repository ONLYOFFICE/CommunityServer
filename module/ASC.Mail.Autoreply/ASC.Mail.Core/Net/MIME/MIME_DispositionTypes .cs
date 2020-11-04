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


namespace ASC.Mail.Net.MIME
{
    /// <summary>
    /// This class holds MIME content disposition types. Defined in RFC 2183.
    /// </summary>
    public class MIME_DispositionTypes
    {
        #region Members

        /// <summary>
        /// Bodyparts can be designated `attachment' to indicate that they are separate from the main body of the mail message, 
        /// and that their display should not be automatic, but contingent upon some further action of the user.
        /// </summary>
        public static readonly string Attachment = "attachment";

        /// <summary>
        /// A bodypart should be marked `inline' if it is intended to be displayed automatically upon display of the message. 
        /// Inline bodyparts should be presented in the order in which they occur, subject to the normal semantics of multipart messages.
        /// </summary>
        public static readonly string Inline = "inline";

        #endregion
    }
}