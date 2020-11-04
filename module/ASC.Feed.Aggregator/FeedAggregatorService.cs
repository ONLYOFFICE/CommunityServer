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
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Notify.Signalr;
using ASC.Feed.Aggregator.Config;
using ASC.Feed.Aggregator.Modules;
using ASC.Feed.Aggregator.Modules.Community;
using ASC.Feed.Aggregator.Modules.CRM;
using ASC.Feed.Aggregator.Modules.Documents;
using ASC.Feed.Aggregator.Modules.Projects;
using ASC.Feed.Data;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Core;
using ASC.Web.Studio.Utility;


namespace ASC.Feed.Aggregator
{
    public class FeedAggregatorService
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Feed.Agregator");
        private static readonly SignalrServiceClient signalrServiceClient = new SignalrServiceClient("counters");

        public static readonly List<IFeedModule> Modules = new List<IFeedModule>
            {
                new BlogsModule(),
                new BookmarksModule(),
                new ForumTopicsModule(),
                new ForumPostsModule(),
                new EventsModule(),
                new ContactsModule(),
                new CrmTasksModule(),
                new DealsModule(),
                new CasesModule(),
                new ProjectsModule(),
                new ParticipantsModule(),
                new MilestonesModule(),
                new DiscussionsModule(),
                new TasksModule(),
                new FilesModule(),
                new FoldersModule()
            };

        private Timer aggregateTimer;
        private Timer removeTimer;

        private volatile bool isStopped;
        private readonly object aggregateLock = new object();
        private readonly object removeLock = new object();


        public void Start()
        {
            var cfg = FeedConfigurationSection.GetFeedSection();

            isStopped = false;
            DbRegistry.Configure();
            CommonLinkUtility.Initialize(cfg.ServerRoot);
            WebItemManager.Instance.LoadItems();

            aggregateTimer = new Timer(AggregateFeeds, cfg.AggregateInterval, TimeSpan.Zero, cfg.AggregatePeriod);
            removeTimer = new Timer(RemoveFeeds, cfg.AggregateInterval, cfg.RemovePeriod, cfg.RemovePeriod);
        }

        public void Stop()
        {
            isStopped = true;

            if (aggregateTimer != null)
            {
                aggregateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                aggregateTimer.Dispose();
                aggregateTimer = null;
            }

            if (removeTimer != null)
            {
                removeTimer.Change(Timeout.Infinite, Timeout.Infinite);
                removeTimer.Dispose();
                removeTimer = null;
            }
        }

        private void AggregateFeeds(object interval)
        {
            if (!Monitor.TryEnter(aggregateLock)) return;

            try
            {
                var start = DateTime.UtcNow;
                log.DebugFormat("Start of collecting feeds...");

                var unreadUsers = new Dictionary<int, Dictionary<Guid, int>>();

                foreach (var module in Modules)
                {
                    var result = new List<FeedRow>();
                    var fromTime = FeedAggregateDataProvider.GetLastTimeAggregate(module.GetType().Name);
                    if (fromTime == default(DateTime)) fromTime = DateTime.UtcNow.Subtract((TimeSpan)interval);
                    var toTime = DateTime.UtcNow;

                    var tenants = Attempt(10, () => module.GetTenantsWithFeeds(fromTime)).ToList();
                    log.DebugFormat("Find {1} tenants for module {0}.", module.GetType().Name, tenants.Count());

                    foreach (var tenant in tenants)
                    {
                        // Warning! There is hack here!
                        // clearing the cache to get the correct acl
                        var cache = AscCache.Memory;
                        cache.Remove("acl" + tenant);
                        cache.Remove("/webitemsecurity/" + tenant);
                        cache.Remove(string.Format("sub/{0}/{1}/{2}", tenant, "6045b68c-2c2e-42db-9e53-c272e814c4ad", NotifyConstants.Event_NewCommentForMessage.ID));

                        HttpContext.Current = null;
                        try
                        {
                            if (CoreContext.TenantManager.GetTenant(tenant) == null)
                            {
                                continue;
                            }

                            CoreContext.TenantManager.SetCurrentTenant(tenant);
                            var users = CoreContext.UserManager.GetUsers();
                            // fake httpcontext break configuration manager for mono
                            if (!WorkContext.IsMono)
                            {
                                HttpContext.Current = new HttpContext(
                                new HttpRequest("hack", CommonLinkUtility.GetFullAbsolutePath("/"), string.Empty),
                                new HttpResponse(new StringWriter()));
                            }

                            var feeds = Attempt(10, () => module.GetFeeds(new FeedFilter(fromTime, toTime) {Tenant = tenant}).Where(r=> r.Item1 != null).ToList());
                            log.DebugFormat("{0} feeds in {1} tenant.", feeds.Count, tenant);

                            var tenant1 = tenant;
                            var module1 = module;
                            var feedsRow = feeds
                                .Select(tuple => new Tuple<FeedRow, object>(new FeedRow(tuple.Item1)
                                {
                                    Tenant = tenant1,
                                    ProductId = module1.Product
                                }, tuple.Item2))
                                .ToList();

                            foreach (var u in users)
                            {
                                if (isStopped)
                                {
                                    return;
                                }
                                if (!TryAuthenticate(u.ID))
                                {
                                    continue;
                                }

                                module.VisibleFor(feedsRow, u.ID);
                            }

                            result.AddRange(feedsRow.Select(r=> r.Item1));
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Tenant: {0}, {1}", tenant, ex);
                        }
                        finally
                        {
                            // fake httpcontext break configuration manager for mono
                            if (!WorkContext.IsMono)
                            {
                                if (HttpContext.Current != null)
                                {
                                    new DisposableHttpContext(HttpContext.Current).Dispose();
                                    HttpContext.Current = null;
                                }
                            }
                        }
                    }

                    FeedAggregateDataProvider.SaveFeeds(result, module.GetType().Name, toTime);

                    foreach(var res in result)
                    {
                        foreach (var userGuid in res.Users.Where(userGuid => !userGuid.Equals(res.ModifiedById)))
                        {
                            Dictionary<Guid, int> dictionary;
                            if (!unreadUsers.TryGetValue(res.Tenant, out dictionary))
                            {
                                dictionary = new Dictionary<Guid, int>();
                            }
                            if (dictionary.ContainsKey(userGuid))
                            {
                                ++dictionary[userGuid];
                            }
                            else
                            {
                                dictionary.Add(userGuid, 1);
                            }

                            unreadUsers[res.Tenant] = dictionary;
                        }
                    }
                }

                signalrServiceClient.SendUnreadUsers(unreadUsers);

                log.DebugFormat("Time of collecting news: {0}", DateTime.UtcNow - start);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                Monitor.Exit(aggregateLock);
            }
        }

        private void RemoveFeeds(object interval)
        {
            if (!Monitor.TryEnter(removeLock)) return;

            try
            {
                log.DebugFormat("Start of removing old news");
                FeedAggregateDataProvider.RemoveFeedAggregate(DateTime.UtcNow.Subtract((TimeSpan)interval));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                Monitor.Exit(removeLock);
            }
        }

        private static T Attempt<T>(int count, Func<T> action)
        {
            var counter = 0;
            while (true)
            {
                try
                {
                    return action();
                }
                catch
                {
                    if (count < ++counter)
                    {
                        throw;
                    }
                }
            }
        }

        private static bool TryAuthenticate(Guid userid)
        {
            try
            {
                SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(userid));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}