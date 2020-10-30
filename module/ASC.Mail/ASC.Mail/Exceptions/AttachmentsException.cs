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


using System;

namespace ASC.Mail.Exceptions
{
    public class AttachmentsException : Exception
    {
        public enum Types
        {
            Unknown = 0,
            BadParams = 1,
            EmptyFile = 2,
            MessageNotFound = 3,
            TotalSizeExceeded = 4,
            DocumentNotFound = 5,
            DocumentAccessDenied = 6
        }
        public Types ErrorType { get; set; }

        public AttachmentsException(Types type, string message) : base(message) 
        {
            ErrorType = type;
        }
    }
}
