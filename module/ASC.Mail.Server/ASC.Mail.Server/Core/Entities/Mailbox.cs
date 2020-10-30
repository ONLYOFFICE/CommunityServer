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

namespace ASC.Mail.Server.Core.Entities
{
    public class Mailbox
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Maldir { get; set; }
        public long Quota { get; set; }
        public string LocalPart { get; set; }
        public string Domain { get; set; }
        public bool Active { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}