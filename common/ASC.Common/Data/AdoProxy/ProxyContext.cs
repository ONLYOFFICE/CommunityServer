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
using System.Data;

namespace ASC.Common.Data.AdoProxy
{
    class ProxyContext
    {
        private readonly Action<ExecutedEventArgs> executedEvent;


        public ProxyContext(Action<ExecutedEventArgs> executedEvent)
        {
            if (executedEvent == null)
            {
                throw new ArgumentNullException("executedEvent");
            }
            this.executedEvent = executedEvent;
        }


        public void FireExecuteEvent(DbCommandProxy cmd, string method, TimeSpan duration, int sqlThread)
        {
            executedEvent(new ExecutedEventArgs("Command." + method, duration, cmd, sqlThread));
        }

        public void FireExecuteEvent(string method, TimeSpan duration, int sqlThread)
        {
            executedEvent(new ExecutedEventArgs("Connection." + method, duration, sqlThread));
        }

        public void FireExecuteEvent(IDbTransaction tx, string method, TimeSpan duration, int sqlThread)
        {
            executedEvent(new ExecutedEventArgs("Transaction." + method, duration, sqlThread));
        }
    }
}
