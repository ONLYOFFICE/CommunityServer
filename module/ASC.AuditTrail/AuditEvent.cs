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


using ASC.MessagingSystem;

namespace ASC.AuditTrail
{
    public class AuditEvent : BaseEvent
    {
        public string Initiator { get; set; }

        [Event("ActionIdCol", 33)]
        public int Action { get; set; }

        [Event("ActionTypeCol", 30)]
        public string ActionTypeText { get; set; }

        [Event("ProductCol", 31)]
        public string Product { get; set; }

        [Event("ModuleCol", 32)]
        public string Module { get; set; }

        [Event("TargetIdCol", 34)]
        public MessageTarget Target { get; set; }
    }
}