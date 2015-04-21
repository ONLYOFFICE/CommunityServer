/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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