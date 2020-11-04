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

namespace ASC.VoipService
{
    public class VoipPhone
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string Alias { get; set; }
        public VoipSettings Settings { get; set; }
        public Agent Caller
        {
            get { return Settings.Caller; }
        }

        public VoipPhone()
        {
            Settings = new VoipSettings();
        }

        public virtual VoipCall Call(string to, string contactId = null)
        {
            throw new NotImplementedException();
        }

        public virtual VoipCall LocalCall(string to)
        {
            throw new NotImplementedException();
        }

        public virtual VoipCall RedirectCall(string callId, string to)
        {
            throw new NotImplementedException();
        }

        public virtual VoipCall HoldUp(string callId)
        {
            throw new NotImplementedException();
        }

        public virtual void AnswerQueueCall(string callId)
        {
            throw new NotImplementedException();
        }

        public virtual void RejectQueueCall(string callId)
        {
            throw new NotImplementedException();
        }
    }

    public class VoipRecord
    {
        public string Id { get; set; }

        public string Uri { get; set; }

        public int Duration { get; set; }

        public decimal Price { get; set; }
    }
}
