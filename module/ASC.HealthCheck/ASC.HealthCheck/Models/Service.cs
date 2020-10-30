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
using System.Configuration;
using System.Net;
using System.Reflection;
using System.Threading;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Common.Contracts;
using ASC.Core.Configuration;
using ASC.Core.Notify;
using ASC.Core.Notify.Jabber;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;
using ASC.Feed.Aggregator.Config;
using ASC.Feed.Data;
using ASC.HealthCheck.Classes;
using ASC.HealthCheck.Resources;
using ASC.Notify.Messages;
using ASC.Web.CRM.Core;
using Autofac;
using log4net;

namespace ASC.HealthCheck.Models
{
    public abstract class Service : ICloneable
    {
        public int Attempt { get; set; }
        public ServiceEnum ServiceName { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public abstract string Title { get; }

        protected ILog log;

        protected readonly string fakeUserId = HealthCheckRunner.FakeUserId;
        protected readonly string hubUrl = ConfigurationManager.AppSettings["web.hub"] ?? "http://localhost:9899/";
        protected const int StateOnline = 1;

        protected Service(ServiceEnum serviceName)
        {
            ServiceName = serviceName;
            Message = string.Empty;
            Status = ServiceStatus.Running.GetStringStatus();
            log = LogManager.GetLogger(typeof(Service));
        }

        public abstract string Check(int tenantId);

        public object Clone()
        {
            return MemberwiseClone();
        }

        public static Service CreateNewService(ServiceEnum serviceEnum)
        {
            switch (serviceEnum)
            {
                case ServiceEnum.OnlyofficeJabber:
                    return new OnlyofficeJabberService();
                case ServiceEnum.OnlyofficeSignalR:
                    return new OnlyofficeSignalRService();
                case ServiceEnum.OnlyofficeNotify:
                    return new OnlyofficeNotifyService();
                case ServiceEnum.OnlyofficeBackup:
                    return new OnlyofficeBackupService();
                case ServiceEnum.OnlyofficeFeed:
                    return new OnlyofficeFeedService();
                case ServiceEnum.OnlyofficeMailAggregator:
                    return new OnlyofficeMailAggregatorService();
                case ServiceEnum.OnlyofficeMailWatchdog:
                    return new OnlyofficeMailWatchdogService();
                case ServiceEnum.OnlyofficeAutoreply:
                    return new OnlyofficeAutoreplyService();
                case ServiceEnum.OnlyofficeIndex:
                    return new OnlyofficeIndexService();
                case ServiceEnum.EditorsFileConverter:
                    return new EditorsFileConverterService();
                case ServiceEnum.EditorsCoAuthoring:
                    return new EditorsCoAuthoringService();
                case ServiceEnum.EditorsSpellChecker:
                    return new EditorsSpellCheckerService();
                case ServiceEnum.MiniChat:
                    return new MiniChatService();
            }
            return null;
        }
    }

    public class OnlyofficeJabberService : Service
    {
        public OnlyofficeJabberService() : base(ServiceEnum.OnlyofficeJabber)
        {
        }

        public override string Title { get { return HealthCheckResource.JabberServiceTitle; } }

        public override string Check(int tenantId)
        {
            try
            {
                log.Debug("CheckJabberState");
                using (var jabberServiceClient = new JabberServiceClientWcf())
                {
                    jabberServiceClient.Open();
                    var userGuid = new Guid(fakeUserId);
                    var user = CoreContext.UserManager.GetUsers(userGuid);
                    var status = jabberServiceClient.HealthCheck(user.UserName, tenantId);
                    if (status == string.Empty)
                    {
                        log.Debug("Jabber is OK!");
                        return string.Empty;
                    }

                    log.ErrorFormat("Jabber is failed! {0}", status);
                    return status;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Jabber is failed! {0} {1} {2}", ex.Message, ex.StackTrace,
                    ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return HealthCheckResource.ServiceCheckFailed;
            }
        }
    }

    public class OnlyofficeSignalRService : Service
    {
        public OnlyofficeSignalRService() : base(ServiceEnum.OnlyofficeSignalR)
        {
        }

        public override string Title { get { return HealthCheckResource.SignalRServiceTitle; } }

        public override string Check(int tenantId)
        {
            return "";
            //HubConnection hubConnection = null;
            //try
            //{
            //    log.Debug("CheckMiniChatState");
            //    IDictionary<string, string> queryString = new Dictionary<string, string>();
            //    var fakeTenant = CoreContext.TenantManager.GetTenant(tenantId);
            //    queryString["token"] =
            //        Signature.Create(string.Join(",", fakeTenant.TenantId, fakeUserId, fakeTenant.TenantAlias));
            //    hubConnection = new HubConnection(hubUrl, queryString);
            //    var hubProxy = hubConnection.CreateHubProxy("c"); // Chat
            //    ServicePointManager.DefaultConnectionLimit = 10;
            //    hubConnection.Start(new LongPollingTransport()).Wait();
            //    // initDataRetrieved
            //    hubProxy.On<string, string, UserClass[], int, string>("idr",
            //        (uName, displayUserName, users, userTenant, domain) => hubConnection.Stop());

            //    hubProxy.Invoke("cu", StateOnline).Wait(); // ConnectUser
            //    // hubProxy.Invoke("gid").Wait(); // GetInitData

            //    log.Debug("MiniChat is OK!");
            //    return string.Empty;
            //}
            //catch (Exception ex)
            //{
            //    log.ErrorFormat("SignalR is failed! {0} {1} {2}", ex.Message, ex.StackTrace,
            //        ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            //    return HealthCheckResource.ServiceCheckFailed;
            //}
            //finally
            //{
            //    if (hubConnection != null)
            //    {
            //        hubConnection.Stop();
            //    }
            //}
        }
    }

    public class OnlyofficeNotifyService : Service
    {
        public OnlyofficeNotifyService() : base(ServiceEnum.OnlyofficeNotify) { }

        public override string Title { get { return HealthCheckResource.NotifyServiceTitle; } }

        public override string Check(int tenantId)
        {
            try
            {
                log.Debug("CheckNotifyState");
                using (var notifyServiceClient = new NotifyServiceClient())
                {
                    var userGuid = new Guid(fakeUserId);
                    var user = CoreContext.UserManager.GetUsers(userGuid);
                    notifyServiceClient.SendNotifyMessage(new NotifyMessage
                    {
                        To = user.UserName,
                        Subject = "Subject",
                        ContentType = "ContentType",
                        Content = "Content",
                        Sender = Constants.NotifyMessengerSenderSysName,
                        CreationDate = DateTime.UtcNow
                    });
                }
                log.Debug("Notify is OK!");
                return string.Empty;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Notify is failed! {0} {1} {2}", ex.Message, ex.StackTrace,
                    ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return HealthCheckResource.ServiceCheckFailed;
            }
        }
    }

    public class OnlyofficeBackupService : Service
    {
        public OnlyofficeBackupService() : base(ServiceEnum.OnlyofficeBackup) { }

        public override string Title { get { return HealthCheckResource.BackupServiceTitle; } }

        public override string Check(int tenantId)
        {
            try
            {
                log.Debug("CheckBackupState");
                using (var backupServiceClient = new BackupServiceClient())
                {
                    var status = backupServiceClient.StartBackup(new StartBackupRequest
                    {
                        TenantId = tenantId,
                        StorageType = BackupStorageType.DataStore
                    });
                    try
                    {
                        while (!status.IsCompleted)
                        {
                            Thread.Sleep(1000);
                            status = backupServiceClient.GetBackupProgress(tenantId);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Backup is failed! {0} {1} {2}", status.Error, ex.Message, ex.StackTrace);
                        return HealthCheckResource.BackupServiceWorksIncorrectMsg;
                    }

                    log.Debug("Backup is OK!");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Backup is failed! {0} {1} {2}", ex.Message, ex.StackTrace,
                    ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return HealthCheckResource.BackupServiceWorksIncorrectMsg;
            }
        }
    }

    public class OnlyofficeFeedService : Service
    {
        public OnlyofficeFeedService() : base(ServiceEnum.OnlyofficeFeed) { }

        public override string Title { get { return HealthCheckResource.FeedServiceTitle; } }

        public override string Check(int tenantId)
        {
            try
            {
                log.Debug("CheckFeedState");
                var userGuid = new Guid(fakeUserId);
                CoreContext.TenantManager.SetCurrentTenant(tenantId);
                SecurityContext.AuthenticateMe(userGuid);

                var person = new Person
                {
                    FirstName = "Homer",
                    LastName = "Simpson",
                    JobTitle = "Software engineer",
                    About = "Cool dude"
                };

                int personId;

                using (var scope = DIHelper.Resolve(tenantId))
                {
                    var dao = scope.Resolve<DaoFactory>().ContactDao;
                    personId = dao.SaveContact(person);

                    CRMSecurity.SetAccessTo(person, new List<Guid> {userGuid});

                    // waiting while service is collecting news
                    var feedCfg = FeedConfigurationSection.GetFeedSection();

                    Thread.Sleep(feedCfg.AggregatePeriod + TimeSpan.FromSeconds(30));

                    var feedItemId = string.Format("person_{0}", personId);

                    var feedItem = FeedAggregateDataProvider.GetFeedItem(feedItemId);
                    if (feedItem == null)
                    {
                        log.ErrorFormat("Error! Feed Item is null, feedItemId = {0}", feedItemId);
                        dao.DeleteContact(personId);
                        FeedAggregateDataProvider.RemoveFeedItem(feedItemId);
                        return HealthCheckResource.FeedService_NewsGenerationError;
                    }

                    dao.DeleteContact(personId);
                    FeedAggregateDataProvider.RemoveFeedItem(feedItemId);
                    log.Debug("Feed is OK!");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Feed is failed! {0}, innerException = {1}", ex,
                    ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return HealthCheckResource.ServiceCheckFailed;
            }
        }
    }

    public class OnlyofficeMailAggregatorService : Service
    {
        public OnlyofficeMailAggregatorService() : base(ServiceEnum.OnlyofficeMailAggregator) { }

        public override string Title { get { return HealthCheckResource.MailAggregatorServiceTitle; } }

        public override string Check(int tenantId)
        {
            try
            {
                log.Debug("CheckMailAggregatorState");
                /*var userGuid = new Guid(fakeUserId);
                var user = CoreContext.UserManager.GetUsers(userGuid);
                var asc_auth_key = SecurityContext.AuthenticateMe(userGuid);
                string portalUrl = "http://localhost/";
                var client = new RestClient(portalUrl);
                var request = new RestRequest("/api/2.0/mail/messages/send.json", Method.PUT);
                request.AddCookie("asc_auth_key", asc_auth_key);
                request.AddParameter("id", 0, ParameterType.QueryString);
                request.AddParameter("from", user.Email, ParameterType.QueryString);
                request.AddParameter("to[]", "<" + user.Email + ">", ParameterType.QueryString);
                request.AddParameter("subject", "test-subject", ParameterType.QueryString);
                request.AddParameter("body", "<p>test-body</p>", ParameterType.QueryString);
                request.AddParameter("streamId", Guid.NewGuid().ToString("N").ToLower(), ParameterType.QueryString);
                request.AddParameter("mimeMessageId", string.Empty, ParameterType.QueryString);
                request.AddParameter("mimeReplyToId", string.Empty, ParameterType.QueryString);
                request.AddParameter("importance", false, ParameterType.QueryString);

                // execute the request
                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Can't send mail using API. Response code = " + response.StatusCode, response.ErrorException);
                }
                var jObject = JObject.Parse(response.Content);

                // waiting for the end of the MailAggregator service activity
                Thread.Sleep(TimeSpan.FromSeconds(30));

                request = new RestRequest(string.Format("/api/2.0/mail/messages/{0}.json", jObject["response"]), Method.GET);
                request.AddCookie("asc_auth_key", asc_auth_key);
                response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Can't get mail information using API. Response code = "
                                        + response.StatusCode, response.ErrorException);
                }
                jObject = JObject.Parse(response.Content);
                var folderObject = jObject["response"]["folder"];
                if (folderObject == null)
                {
                    throw new Exception("Can't get mail information using API. Wrong Json content: " + response.Content);
                }
                var folder = folderObject.ToString();

                if (folder != INBOX_FOLDER)
                {
                    throw new Exception("Can't get mail information from inbox folder. Wrong Json content: " + response.Content);
                }
                log.Debug("MailAggregator is OK!");*/
                return string.Empty;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("MailAggregator is failed! {0} {1} {2}", ex.Message, ex.StackTrace,
                    ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return HealthCheckResource.ServiceCheckFailed;
            }
        }
    }

    public class OnlyofficeMailWatchdogService : Service
    {
        public OnlyofficeMailWatchdogService() : base(ServiceEnum.OnlyofficeMailWatchdog) { }

        public override string Title { get { return HealthCheckResource.MailWatchdogServiceTitle; } }

        public override string Check(int tenantId)
        {
            return string.Empty;
        }
    }

    public class OnlyofficeAutoreplyService : Service
    {
        public OnlyofficeAutoreplyService() : base(ServiceEnum.OnlyofficeAutoreply)
        {
        }

        public override string Title { get { return HealthCheckResource.AutoreplyServiceTitle; } }

        public override string Check(int tenantId)
        {
            return "";
        }
    }

    public class OnlyofficeIndexService : Service
    {
        public OnlyofficeIndexService() : base(ServiceEnum.OnlyofficeIndex) { }

        public override string Title { get { return HealthCheckResource.IndexServiceTitle; } }

        public override string Check(int tenantId)
        {
            try
            {
                return FactoryIndexer.CheckState()
                    ? HealthCheckResource.FullTextIndexServiceWorksCorrectMsg
                    : HealthCheckResource.FullTextIndexServiceWorksIncorrectMsg;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TextIndexer is failed! {0}, innerException = {1}", ex,
                    ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                ReflectionTypeLoadException reflectionTypeLoadException = ex as ReflectionTypeLoadException;
                if (reflectionTypeLoadException != null)
                {
                    foreach (var loaderException in reflectionTypeLoadException.LoaderExceptions)
                    {
                        log.ErrorFormat("loaderException = {0}", loaderException.ToString());
                    }
                }
                return HealthCheckResource.FullTextIndexServiceWorksIncorrectMsg;
            }
        }
    }

    public class EditorsFileConverterService : Service
    {
        public EditorsFileConverterService() : base(ServiceEnum.EditorsFileConverter) { }

        public override string Title { get { return HealthCheckResource.FileConverterServiceTitle; } }

        public override string Check(int tenantId)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var html = client.DownloadString(ConfigurationManager.AppSettings["editors-canvas-service-url"]);

                    return !string.IsNullOrWhiteSpace(html)
                        ? string.Empty
                        : HealthCheckResource.EditorsFileConverter_AccessError;
                }
            }
            catch (WebException)
            {
                return HealthCheckResource.EditorsFileConverter_AccessError;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("EditorsFileConverter is failed! {0} {1} {2}", ex.Message, ex.StackTrace,
                    ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return HealthCheckResource.ServiceCheckFailed;
            }
        }
    }

    public class EditorsCoAuthoringService : Service
    {
        public EditorsCoAuthoringService() : base(ServiceEnum.EditorsCoAuthoring) { }

        public override string Title { get { return HealthCheckResource.CoAuthoringServiceTitle; } }

        public override string Check(int tenantId)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var html = client.DownloadString(ConfigurationManager.AppSettings["editors-coauthoring-service-url"]);
                    return html.IndexOf("Server is functioning normally", StringComparison.OrdinalIgnoreCase) > -1
                        ? string.Empty
                        : HealthCheckResource.EditorsCoAuthoring_AccessError;
                }
            }
            catch (WebException)
            {
                return HealthCheckResource.EditorsCoAuthoring_AccessError;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("EditorsCoAuthoring is failed! {0} {1} {2}", ex.Message, ex.StackTrace,
                    ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return HealthCheckResource.ServiceCheckFailed;
            }
        }
    }

    public class EditorsSpellCheckerService : Service
    {
        public EditorsSpellCheckerService() : base(ServiceEnum.EditorsSpellChecker) { }

        public override string Title { get { return HealthCheckResource.SpellCheckerServiceTitle; } }

        public override string Check(int tenantId)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var html =
                        client.DownloadString(ConfigurationManager.AppSettings["editors-spellchecker-service-url"]);
                    return html.IndexOf("Server is functioning normally", StringComparison.OrdinalIgnoreCase) > -1
                        ? string.Empty
                        : HealthCheckResource.EditorsSpellchecker_AccessError;
                }
            }
            catch (WebException)
            {
                return HealthCheckResource.EditorsSpellchecker_AccessError;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("EditorsSpellchecker is failed! {0} {1} {2}", ex.Message, ex.StackTrace,
                    ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return HealthCheckResource.ServiceCheckFailed;
            }
        }
    }

    public class MiniChatService : Service
    {
        public MiniChatService() : base(ServiceEnum.MiniChat) { }

        public override string Title { get { return HealthCheckResource.MiniChatServiceTitle; } }

        public override string Check(int tenantId)
        {
            return new OnlyofficeSignalRService().Check(tenantId);
        }
    }
}