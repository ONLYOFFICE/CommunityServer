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


namespace ASC.Mail.Net.IMAP
{
    using System;

    /// <summary>
    /// IMAP message flags.
    /// </summary>
    [Flags]
    public enum IMAP_MessageFlags
    {
        /// <summary>
        /// No flags defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// Message has been read.
        /// </summary>
        Seen = 2,

        /// <summary>
        /// Message has been answered.
        /// </summary>
        Answered = 4,

        /// <summary>
        /// Message is "flagged" for urgent/special attention.
        /// </summary>
        Flagged = 8,

        /// <summary>
        /// Message is "deleted" for removal by later EXPUNGE.
        /// </summary>
        Deleted = 16,

        /// <summary>
        /// Message has not completed composition.
        /// </summary>
        Draft = 32,

        /// <summary>
        /// Message is "recently" arrived in this mailbox.
        /// </summary>
        Recent = 64,
    }
}