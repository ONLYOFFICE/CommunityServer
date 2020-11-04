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
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Xml;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Feed;
using ASC.Feed.Data;
using ASC.Web.Core;
using ASC.Web.Core.Helpers;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Services.WhatsNew
{
    [WebService(Namespace = "http://www.teamlab.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.None)]
    public class feed : IHttpHandler
    {
        public const string HandlerBasePath = "~/services/whatsnew/";
        public const string HandlerPath = "~/services/whatsnew/feed.ashx";
        private const string TitleParam = "c";
        private const string ProductParam = "pid";


        public void ProcessRequest(HttpContext context)
        {
            if (!ProcessAuthorization(context))
            {
                AccessDenied(context);
                return;
            }

            var productId = ParseGuid(context.Request[ProductParam]);
            var product = WebItemManager.Instance[productId.GetValueOrDefault()];
            var products = WebItemManager.Instance.GetItemsAll<IProduct>().ToDictionary(p => p.GetSysName());
            var lastModified = GetLastModified(context);

            var feeds = FeedAggregateDataProvider.GetFeeds(new FeedApiFilter
            {
                Product = product != null ? product.GetSysName() : null,
                From = lastModified ?? DateTime.UtcNow.AddDays(-14),
                To = DateTime.UtcNow,
                OnlyNew = true
            })
                .OrderByDescending(f => f.CreatedDate)
                .Take(100)
                .Select(f => f.ToFeedMin())
                .ToList();

            if (lastModified != null && feeds.Count == 0)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                context.Response.StatusDescription = "Not Modified";
            }

            var feedItems = feeds.Select(f =>
            {
                var item = new SyndicationItem(
                    HttpUtility.HtmlDecode((products.ContainsKey(f.Product) ? products[f.Product].Name + ": " : string.Empty) + f.Title),
                    string.Empty,
                    new Uri(CommonLinkUtility.GetFullAbsolutePath(f.ItemUrl)),
                    f.Id,
                    new DateTimeOffset(TenantUtil.DateTimeToUtc(f.CreatedDate)))
                    {
                        PublishDate = f.CreatedDate,
                    };
                if (f.Author != null && f.Author.UserInfo != null)
                {
                    var u = f.Author.UserInfo;
                    item.Authors.Add(new SyndicationPerson(u.Email, u.DisplayUserName(false), CommonLinkUtility.GetUserProfile(u.ID)));
                }
                return item;
            });

            var lastUpdate = DateTime.UtcNow;
            if (feeds.Count > 0)
            {
                lastUpdate = feeds.Max(x => x.ModifiedDate);
            }

            var feed = new SyndicationFeed(
                CoreContext.TenantManager.GetCurrentTenant().Name,
                string.Empty,
                new Uri(context.Request.GetUrlRewriter(), VirtualPathUtility.ToAbsolute("~/Feed.aspx")),
                TenantProvider.CurrentTenantID.ToString(),
                new DateTimeOffset(lastUpdate),
                feedItems);

            var rssFormatter = new Atom10FeedFormatter(feed);
            var settings = new XmlWriterSettings
            {
                CheckCharacters = false,
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = Encoding.UTF8,
                Indent = true,
            };
            using (var writer = XmlWriter.Create(context.Response.Output, settings))
            {
                rssFormatter.WriteTo(writer);
            }
            context.Response.Charset = Encoding.UTF8.WebName;
            context.Response.ContentType = "application/atom+xml";
            context.Response.AddHeader("ETag", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            context.Response.AddHeader("Last-Modified", DateTime.UtcNow.ToString("R"));
        }

        public static string RenderRssMeta(string title, string productId)
        {
            var urlparams = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(productId))
            {
                urlparams.Add(ProductParam, productId);
            }
            if (!String.IsNullOrEmpty(title))
            {
                urlparams.Add(TitleParam, title);
            }

            var queryString = new StringBuilder("?");
            foreach (var urlparam in urlparams)
            {
                queryString.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(urlparam.Key), HttpUtility.UrlEncode(urlparam.Value));
            }

            return string.Format(@"<link rel=""alternate"" type=""application/atom+xml"" title=""{0}"" href=""{1}"" />",
                title, CommonLinkUtility.GetFullAbsolutePath(HandlerPath) + queryString.ToString().TrimEnd('&'));
        }

        private static bool ProcessAuthorization(HttpContext context)
        {
            if (!SecurityContext.IsAuthenticated)
            {
                try
                {
                    var cookiesKey = CookiesManager.GetCookies(CookiesType.AuthKey);
                    if (!SecurityContext.AuthenticateMe(cookiesKey))
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
                catch (Exception)
                {
                    return AuthorizationHelper.ProcessBasicAuthorization(context);
                }
            }
            return SecurityContext.IsAuthenticated;
        }

        private static void AccessDenied(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.StatusDescription = "Access Denied";
            string realm = String.Format("Basic Realm=\"{0}\"", context.Request.GetUrlRewriter().Host);
            context.Response.AppendHeader("WWW-Authenticate", realm);
            context.Response.Write("401 Access Denied");
        }

        private static Guid? ParseGuid(string guid)
        {
            try
            {
                return new Guid(guid);
            }
            catch
            {
                return null;
            }
        }

        private DateTime? GetLastModified(HttpContext context)
        {
            if (context.Request.Headers["If-Modified-Since"] != null)
            {
                try
                {
                    return DateTime.ParseExact(context.Request.Headers["If-Modified-Since"], "R", null);
                }
                catch { }
            }
            if (context.Request.Headers["If-None-Match"] != null)
            {
                try
                {
                    return DateTime.ParseExact(context.Request.Headers["If-None-Match"], "yyyyMMddHHmmss", null);
                }
                catch { }
            }
            return null;
        }


        public bool IsReusable
        {
            get { return false; }
        }
    }
}
