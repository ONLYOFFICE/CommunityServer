/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ASC.Api.Interfaces;
using ASC.Api.Interfaces.Cache;
using ASC.Api.Logging;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;
using Microsoft.Practices.Unity;

namespace ASC.Api.Impl.Invokers
{
    internal class ApiCachedMethodInvoker : ApiSimpleMethodInvoker
    {
        [Dependency]
        public IUnityContainer Container { get; set; }

        [Dependency]
        public ILog Log { get; set; }


        public override object InvokeMethod(IApiMethodCall methodToCall, object instance, IEnumerable<object> callArg, ApiContext apicontext)
        {
            string cacheKey = null;
            object result = null;
            IEnumerable<Type> knownTypes = null;
            

            var cacheManager = Container.Resolve<ICacheManager>();
            if (ShouldCacheMethod(methodToCall) && cacheManager != null)
            {
                cacheKey = BuildCacheKey(methodToCall, callArg, apicontext);
                result = cacheManager[cacheKey];
                knownTypes = cacheManager[cacheKey + "_knowntypes"] as IEnumerable<Type>;
                apicontext.FromCache = result != null;
                Log.Debug(apicontext.FromCache ? "hit from cache: {0}" : "miss from cache: {0}",
                          methodToCall);
            }
            if (result == null) //if not null than it's from cache
            {
                //Call api method
                var sw = new Stopwatch();
                sw.Start();
                result = base.InvokeMethod(methodToCall, instance, callArg, apicontext);
                sw.Stop();
                long cacheTime;
                bool shouldCache;
                GetCachingPolicy(methodToCall, sw.Elapsed, out cacheTime, out shouldCache);
                Log.Debug("cache policy: {0}",
                          cacheTime > 0 ? TimeSpan.FromMilliseconds(cacheTime).ToString() : "no-cache");
                knownTypes = apicontext.GetKnownTypes().ToList();

                if (cacheKey != null && cacheTime>0)
                {
                    cacheManager.Add(cacheKey,
                                     result,
                                     CacheItemPriority.Normal,
                                     null,
                                     new SlidingTime(TimeSpan.FromMilliseconds(cacheTime)));
                    if (knownTypes!=null)
                    {
                        cacheManager.Add(cacheKey + "_knowntypes",
                                     knownTypes,
                                     CacheItemPriority.Normal,
                                     null,
                                     new SlidingTime(TimeSpan.FromMilliseconds(cacheTime * 2)));
                    }
                    Log.Debug("added to cache: {0}. Key: {1}", methodToCall, cacheKey);

                }
            }
            //Get cached known types
            if (knownTypes!=null)
            {
                apicontext.RegisterTypes(knownTypes);
            }
            return result;
        }

        protected virtual void GetCachingPolicy(IApiMethodCall methodToCall, TimeSpan elapsed, out long cacheTime, out bool shouldCache)
        {
            cacheTime = methodToCall.CacheTime;
            shouldCache = ShouldCacheMethod(methodToCall);
        }

        protected string BuildCacheKey(IApiMethodCall methodToCall, IEnumerable<object> callArg, ApiContext context)
        {
            return Container.Resolve<IApiCacheMethodKeyBuilder>().BuildCacheKeyForMethodCall(methodToCall, callArg, context);
        }

        protected virtual bool ShouldCacheMethod(IApiMethodCall methodToCall)
        {
            return "get".Equals(methodToCall.HttpMethod, StringComparison.OrdinalIgnoreCase) && methodToCall.ShouldCache;
        }
    }
}