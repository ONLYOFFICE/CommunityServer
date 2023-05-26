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

namespace ASC.VoipService
{
    public class Agent
    {
        ///<example>92eb52c8-bb93-4caf-87fb-46ea11530899</example>
        public Guid Id { get; set; }

        ///<example type="int">1</example>
        public AnswerType Answer { get; set; }

        ///<example>ClientID</example>
        public string ClientID { get { return PhoneNumber + PostFix; } }

        ///<example>true</example>
        public bool Record { get; set; }

        ///<example type="int">1</example>
        public int TimeOut { get; set; }

        ///<example type="int">1</example>
        public AgentStatus Status { get; set; }

        ///<example>true</example>
        public bool AllowOutgoingCalls { get; set; }

        ///<example>PostFix</example>
        public string PostFix { get; set; }

        ///<example>PhoneNumber</example>
        public string PhoneNumber { get; set; }

        ///<example>RedirectToNumber</example>
        public string RedirectToNumber { get; set; }

        public Agent()
        {
            Status = AgentStatus.Offline;
            TimeOut = 30;
            AllowOutgoingCalls = true;
            Record = true;
        }

        public Agent(Guid id, AnswerType answer, VoipPhone phone, string postFix)
            : this()
        {
            Id = id;
            Answer = answer;
            PhoneNumber = phone.Number;
            AllowOutgoingCalls = phone.Settings.AllowOutgoingCalls;
            Record = phone.Settings.Record;
            PostFix = postFix;
        }
    }

    public class Queue
    {
        ///<example>id</example>
        public string Id { get; set; }

        ///<example>Name</example>
        public string Name { get; set; }

        ///<example type="int">123</example>
        public int Size { get; set; }

        ///<example>WaitUrl</example>
        public string WaitUrl { get; set; }

        ///<example type="int">4</example>
        public int WaitTime { get; set; }

        public Queue() { }

        public Queue(string id, string name, int size, string waitUrl, int waitTime)
        {
            Id = id;
            Name = name;
            WaitUrl = waitUrl;
            WaitTime = waitTime;
            Size = size;
        }
    }

    public class WorkingHours
    {
        ///<example>true</example>
        public bool Enabled { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public TimeSpan? From { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public TimeSpan? To { get; set; }

        public WorkingHours() { }

        public WorkingHours(TimeSpan from, TimeSpan to)
        {
            From = from;
            To = to;
        }


        protected bool Equals(WorkingHours other)
        {
            return Enabled.Equals(other.Enabled) && From.Equals(other.From) && To.Equals(other.To);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((WorkingHours)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Enabled.GetHashCode();
                hashCode = (hashCode * 397) ^ From.GetHashCode();
                hashCode = (hashCode * 397) ^ To.GetHashCode();
                return hashCode;
            }
        }
    }

    public class VoipUpload
    {
        ///<example>Name</example>
        public string Name { get; set; }

        ///<example>Path</example>
        public string Path { get; set; }

        ///<example type="int">1</example>
        public AudioType AudioType { get; set; }

        ///<example>true</example>
        public bool IsDefault { get; set; }
    }

    #region Enum

    public enum AnswerType
    {
        Number,
        Sip,
        Client
    }

    public enum GreetingMessageVoice
    {
        Man,
        Woman,
        Alice
    }

    public enum AgentStatus
    {
        Online,
        Paused,
        Offline
    }

    public enum AudioType
    {
        Greeting,
        HoldUp,
        VoiceMail,
        Queue
    }

    #endregion
}
