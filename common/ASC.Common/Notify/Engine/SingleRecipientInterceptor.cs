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
using ASC.Notify.Recipients;

namespace ASC.Notify.Engine
{
    class SingleRecipientInterceptor : ISendInterceptor
    {
        private const string prefix = "__singlerecipientinterceptor";
        private readonly List<IRecipient> sendedTo = new List<IRecipient>(10);


        public string Name { get; private set; }

        public InterceptorPlace PreventPlace { get { return InterceptorPlace.GroupSend | InterceptorPlace.DirectSend; } }

        public InterceptorLifetime Lifetime { get { return InterceptorLifetime.Call; } }


        internal SingleRecipientInterceptor(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("name");
            Name = name;
        }

        public bool PreventSend(NotifyRequest request, InterceptorPlace place)
        {
            var sendTo = request.Recipient;
            if (!sendedTo.Exists(rec => Equals(rec, sendTo)))
            {
                sendedTo.Add(sendTo);
                return false;
            }
            return true;
        }
    }
}