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
using System.Collections.Generic;

namespace ASC.VoipService
{
    public class VoipCall
    {
        public string Id { get; set; }

        public string ParentID { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public Guid AnsweredBy { get; set; }

        public DateTime DialDate { get; set; }

        public int DialDuration { get; set; }

        public VoipCallStatus? Status { get; set; }

        public decimal Price { get; set; }

        public int ContactId { get; set; }

        public bool ContactIsCompany { get; set; }
        
        public string ContactTitle { get; set; }

        public DateTime Date { get; set; }

        public DateTime EndDialDate { get; set; }

        public VoipRecord VoipRecord { get; set; }

        public List<VoipCall> ChildCalls { get; set; }

        public VoipCall()
        {
            ChildCalls = new List<VoipCall>();
            VoipRecord = new VoipRecord();
        }
    }

    public enum VoipCallStatus
    {
        Incoming,
        Outcoming,
        Answered,
        Missed
    }
}