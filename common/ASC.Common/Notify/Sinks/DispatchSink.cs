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
using ASC.Notify.Engine;
using ASC.Notify.Messages;

namespace ASC.Notify.Sinks
{
    class DispatchSink : Sink
    {
        private readonly string senderName;
        private readonly DispatchEngine dispatcher;

        public DispatchSink(string senderName, DispatchEngine dispatcher)
        {
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            
            this.dispatcher = dispatcher;
            this.senderName = senderName;
        }

        public override SendResponse ProcessMessage(INoticeMessage message)
        {
            return dispatcher.Dispatch(message, senderName);
        }

        public override void ProcessMessageAsync(INoticeMessage message)
        {
            dispatcher.Dispatch(message, senderName);
        }
    }
}