/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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