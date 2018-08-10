/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;

namespace ASC.VoipService
{
    public class Agent
    {
        public Guid Id { get; set; }

        public AnswerType Answer { get; set; }

        public string ClientID { get { return PhoneNumber + PostFix; } }

        public bool Record { get; set; }

        public int TimeOut { get; set; }

        public AgentStatus Status { get; set; }

        public bool AllowOutgoingCalls { get; set; }

        public string PostFix { get; set; }

        public string PhoneNumber { get; set; }

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
        public string Id { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public string WaitUrl { get; set; }
        public int WaitTime { get; set; }

        public Queue(){}

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
        public bool Enabled { get; set; }
        public TimeSpan? From { get; set; }
        public TimeSpan? To { get; set; }

        public WorkingHours(){}

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
        public string Name { get; set; }
        public string Path { get; set; }
        public AudioType AudioType { get; set; }
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
