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


namespace ASC.Mail.Net.IMAP.Client
{
    /// <summary>
    /// Specifies what data is requested from IMAP server FETCH command.
    /// Fetch items are flags and can be combined. For example: IMAP_FetchItem_Flags.MessageFlags | IMAP_FetchItem_Flags.Header.
    /// </summary>
    public enum IMAP_FetchItem_Flags
    {
        /// <summary>
        /// Message UID value.
        /// </summary>
        UID = 1,

        /// <summary>
        /// Message size in bytes.
        /// </summary>
        Size = 2,

        /// <summary>
        /// Message IMAP server INTERNALDATE.
        /// </summary>
        InternalDate = 4,

        /// <summary>
        /// Fetches message flags. (\SEEN \ANSWERED ...)
        /// </summary>
        MessageFlags = 8,

        /// <summary>
        /// Fetches message header.
        /// </summary>
        Header = 16,

        /// <summary>
        /// Fetches full message.
        /// </summary>
        Message = 32,

        /// <summary>
        /// Fetches message ENVELOPE structure.
        /// </summary>
        Envelope = 64,

        /// <summary>
        /// Fetches message BODYSTRUCTURE structure.
        /// </summary>
        BodyStructure = 128,

        /// <summary>
        /// Fetches all info.
        /// </summary>
        All = 0xFFFF
    }
}