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
using System.Globalization;
using System.Linq;
using System.Security;
using System.Threading;
using System.Web.Routing;
using ASC.Api.Attributes;
using ASC.Api.Logging;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;
using Microsoft.Practices.ServiceLocation;

namespace ASC.Api.GloabalFilters
{


    public class RateLimitingFilter : ApiCallFilter
    {
        private readonly int _cooldown;
        private readonly bool _sliding;
        private readonly string _basedomain;
        private readonly ILog _log;

        private ICacheItemExpiration Time
        {
            get
            {
                if (_sliding)
                {
                    return new SlidingTime(TimeSpan.FromMilliseconds(_cooldown));
                }
                return new AbsoluteTime(TimeSpan.FromMilliseconds(_cooldown));
            }
        }

        private class CallCount
        {
            private int _count;

            internal CallCount(int count)
            {
                _count = count;
            }

            internal int AddCall()
            {
                return Interlocked.Add(ref _count, 1);
            }
        }

        public int MaxRate { get; set; }

        public RateLimitingFilter(int maxRate, int cooldown, bool sliding, string basedomain)
        {
            _cooldown = cooldown;
            _sliding = sliding;
            _basedomain = basedomain;
            _log = ServiceLocator.Current.GetInstance<ILog>();

            MaxRate = maxRate;
        }

        public override void PreMethodCall(Interfaces.IApiMethodCall method, Impl.ApiContext context, System.Collections.Generic.IEnumerable<object> arguments)
        {
            //Store to cache
            try
            {
                if (!IsNeededToThrottle(context.RequestContext))//Local server requests
                {
                    return;
                } 

                //Try detect referer if it's from site call
                var cache = ServiceLocator.Current.GetInstance<ICacheManager>();
                var callCount = cache[method + context.RequestContext.HttpContext.Request.UserHostAddress] as CallCount;
                if (callCount == null)
                {
                    //This means it's not in cache
                    cache.Add(method + context.RequestContext.HttpContext.Request.UserHostAddress, new CallCount(1), CacheItemPriority.Normal, null, Time);
                }
                else
                {
                    if (callCount.AddCall() > MaxRate)
                    {
                        context.RequestContext.HttpContext.Response.AddHeader("Retry-After", ((int)TimeSpan.FromMilliseconds(_cooldown).TotalSeconds).ToString(CultureInfo.InvariantCulture));
                        context.RequestContext.HttpContext.Response.StatusCode = 503;
                        context.RequestContext.HttpContext.Response.StatusDescription = "Limit reached";
                        _log.Warn("limiting requests for {0} to cd:{1}", method + context.RequestContext.HttpContext.Request.UserHostAddress, _cooldown);
                        throw new SecurityException("Rate limit reached. Try again after " + _cooldown + " ms");
                    }
                }
            }
            catch (SecurityException e)
            {
                _log.Error(e,"limit requests");
                throw;//Throw this exceptions
            }
            catch (Exception)
            {

            }
        }

        protected virtual bool IsNeededToThrottle(RequestContext request)
        {
            if (request.HttpContext.Request.IsLocal)
                return false;
            if (request.HttpContext.Request.UrlReferrer != null &&
                request.HttpContext.Request.UrlReferrer.Host.EndsWith(_basedomain))
            {
                return false;
            }
            return true;
        }
    }
}