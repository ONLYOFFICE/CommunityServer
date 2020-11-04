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
using System.Runtime.Remoting.Messaging;

namespace ASC.Notify.Engine
{
    class InterceptorStorage
    {
        private readonly string CallContext_Prefix = "InterceptorStorage.CALLCONTEXT_KEY." + Guid.NewGuid();
        private readonly object syncRoot = new object();
        private readonly Dictionary<string, ISendInterceptor> globalInterceptors = new Dictionary<string, ISendInterceptor>(10);

        
        private Dictionary<string, ISendInterceptor> callInterceptors
        {
            get
            {
                var storage = CallContext.GetData(CallContext_Prefix) as Dictionary<string, ISendInterceptor>;
                if (storage == null)
                {
                    storage = new Dictionary<string, ISendInterceptor>(10);
                    CallContext.SetData(CallContext_Prefix, storage);
                }
                return storage;
            }
        }


        public void Add(ISendInterceptor interceptor)
        {
            if (interceptor == null) throw new ArgumentNullException("interceptor");
            if (String.IsNullOrEmpty(interceptor.Name)) throw new ArgumentException("empty name property", "interceptor");

            switch (interceptor.Lifetime)
            {
                case InterceptorLifetime.Call:
                    AddInternal(interceptor, callInterceptors);
                    break;
                case InterceptorLifetime.Global:
                    AddInternal(interceptor, globalInterceptors);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public ISendInterceptor Get(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("empty name", "name");

            ISendInterceptor result = null;
            result = GetInternal(name, callInterceptors);
            if (result == null) result = GetInternal(name, globalInterceptors);
            return result;
        }

        public void Remove(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("empty name", "name");

            RemoveInternal(name, callInterceptors);
            RemoveInternal(name, globalInterceptors);
        }

        public void Clear()
        {
            Clear(InterceptorLifetime.Call | InterceptorLifetime.Global);
        }

        public void Clear(InterceptorLifetime lifetime)
        {
            lock (syncRoot)
            {
                if ((lifetime & InterceptorLifetime.Call) == InterceptorLifetime.Call) callInterceptors.Clear();
                if ((lifetime & InterceptorLifetime.Global) == InterceptorLifetime.Global) globalInterceptors.Clear();
            }
        }

        public List<ISendInterceptor> GetAll()
        {
            var result = new List<ISendInterceptor>();
            result.AddRange(callInterceptors.Values);
            result.AddRange(globalInterceptors.Values);
            return result;
        }


        private void AddInternal(ISendInterceptor interceptor, Dictionary<string, ISendInterceptor> storage)
        {
            lock (syncRoot)
            {
                storage[interceptor.Name] = interceptor;
            }
        }

        private ISendInterceptor GetInternal(string name, Dictionary<string, ISendInterceptor> storage)
        {
            ISendInterceptor interceptor = null;
            lock (syncRoot)
            {
                storage.TryGetValue(name, out interceptor);
            }
            return interceptor;
        }

        private void RemoveInternal(string name, Dictionary<string, ISendInterceptor> storage)
        {
            lock (syncRoot)
            {
                storage.Remove(name);
            }
        }
    }
}