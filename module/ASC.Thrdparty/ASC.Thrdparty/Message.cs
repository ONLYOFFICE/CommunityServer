/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System;

namespace ASC.Thrdparty
{
    /// <summary>
    /// Represents an user activity message
    /// </summary>
    public class Message
    {
        /// <summary>
        /// User name
        /// </summary>
        /// <example>Name user</example>
        public string UserName { get; set; }

        /// <summary>
        /// Message text
        /// </summary>
        /// <example>text</example>
        public string Text { get; set; }

        /// <summary>
        /// The date of message post
        /// </summary>
        /// <example>2008-04-10T06-30-00.000Z</example>
        public DateTime PostedOn { get; set; }

        /// <summary>
        /// Social network
        /// </summary>
        /// <example type="int">0</example>
        public SocialNetworks Source { get; set; }
    }
}
