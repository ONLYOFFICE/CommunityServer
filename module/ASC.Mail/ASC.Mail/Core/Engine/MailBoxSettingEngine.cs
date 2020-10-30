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
using System.Linq;
using ASC.Common.Logging;
using ASC.Common.Utils;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Core.Engine
{
    public class MailBoxSettingEngine
    {
        public ILog Log { get; private set; }

        public MailBoxSettingEngine(ILog log = null)
        {
            Log = log ?? LogManager.GetLogger("ASC.Mail.MailBoxSettingEngine");
        }

        public bool SetMailBoxSettings(ClientConfig config, bool isUserData)
        {
            try
            {
                if (string.IsNullOrEmpty(config.EmailProvider.Id) ||
                    !config.EmailProvider.Domain.Any() ||
                    config.EmailProvider.IncomingServer == null ||
                    !config.EmailProvider.IncomingServer.Any() ||
                    config.EmailProvider.OutgoingServer == null ||
                    !config.EmailProvider.OutgoingServer.Any())
                    throw new Exception("Incorrect config");

                using (var daoFactory = new DaoFactory())
                {
                    using (var tx = daoFactory.DbManager.BeginTransaction())
                    {
                        var daoMbProvider = daoFactory.CreateMailboxProviderDao();

                        var provider = daoMbProvider.GetProvider(config.EmailProvider.Id);

                        if (provider == null)
                        {
                            provider = new MailboxProvider
                            {
                                Id = 0,
                                Name = config.EmailProvider.Id,
                                DisplayName = config.EmailProvider.DisplayName,
                                DisplayShortName = config.EmailProvider.DisplayShortName,
                                Url = config.EmailProvider.Documentation.Url
                            };

                            provider.Id = daoMbProvider.SaveProvider(provider);

                            if (provider.Id < 0)
                            {
                                tx.Rollback();
                                throw new Exception("id_provider not saved into DB");
                            }
                        }

                        var daoMbDomain = daoFactory.CreateMailboxDomainDao();

                        foreach (var domainName in config.EmailProvider.Domain)
                        {
                            var domain = daoMbDomain.GetDomain(domainName);

                            if (domain != null) 
                                continue;

                            domain = new MailboxDomain
                            {
                                Id = 0,
                                ProviderId = provider.Id,
                                Name = domainName
                            };

                            domain.Id = daoMbDomain.SaveDomain(domain);

                            if (domain.Id < 0)
                            {
                                tx.Rollback();
                                throw new Exception("id_domain not saved into DB");
                            }
                        }

                        var daoMbServer = daoFactory.CreateMailboxServerDao();

                        var existingServers = daoMbServer.GetServers(provider.Id);

                        var newServers = config.EmailProvider
                            .IncomingServer
                            .ConvertAll(s => new MailboxServer
                            {
                                Id = 0,
                                Username = s.Username,
                                Type = s.Type,
                                ProviderId = provider.Id,
                                Hostname = s.Hostname,
                                Port = s.Port,
                                SocketType = s.SocketType,
                                Authentication = s.Authentication,
                                IsUserData = isUserData
                            });

                        newServers.AddRange(config.EmailProvider
                            .OutgoingServer
                            .ConvertAll(s => new MailboxServer
                            {
                                Id = 0,
                                Username = s.Username,
                                Type = s.Type,
                                ProviderId = provider.Id,
                                Hostname = s.Hostname,
                                Port = s.Port,
                                SocketType = s.SocketType,
                                Authentication = s.Authentication,
                                IsUserData = isUserData
                            }));

                        foreach (var s in newServers)
                        {
                            var existing =
                                existingServers.FirstOrDefault(
                                    es =>
                                        es.Type.Equals(s.Type) && es.Port == s.Port &&
                                        es.SocketType.Equals(s.SocketType));

                            if (existing != null)
                            {
                                if (existing.Equals(s))
                                    continue;

                                s.Id = existing.Id;
                            }

                            s.Id = daoMbServer.SaveServer(s);

                            if (s.Id < 0)
                            {
                                tx.Rollback();
                                throw new Exception("id_server not saved into DB");
                            }
                        }

                        tx.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("SetMailBoxSettings failed", ex);

                return false;
            }

            return true;
        }

        public ClientConfig GetMailBoxSettings(string host)
        {
            var config = GetStoredMailBoxSettings(host);
            return config ?? SearchBusinessVendorsSettings(host);
        }

        private static ClientConfig GetStoredMailBoxSettings(string host)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMbDomain = daoFactory.CreateMailboxDomainDao();

                var domain = daoMbDomain.GetDomain(host);

                if (domain == null)
                    return null;

                var daoMbProvider = daoFactory.CreateMailboxProviderDao();

                var provider = daoMbProvider.GetProvider(domain.ProviderId);

                if (provider == null)
                    return null;

                var daoMbServer = daoFactory.CreateMailboxServerDao();

                var existingServers = daoMbServer.GetServers(provider.Id);

                if (!existingServers.Any())
                    return null;

                var config = new ClientConfig();

                config.EmailProvider.Domain.Add(host);
                config.EmailProvider.Id = provider.Name;
                config.EmailProvider.DisplayName = provider.DisplayName;
                config.EmailProvider.DisplayShortName = provider.DisplayShortName;
                config.EmailProvider.Documentation.Url = provider.Url;

                existingServers.ForEach(serv =>
                {
                    if (serv.Type == "smtp")
                    {
                        config.EmailProvider.OutgoingServer.Add(
                            new ClientConfigEmailProviderOutgoingServer
                            {
                                Type = serv.Type,
                                SocketType = serv.SocketType,
                                Hostname = serv.Hostname,
                                Port = serv.Port,
                                Username = serv.Username,
                                Authentication = serv.Authentication
                            });
                    }
                    else
                    {
                        config.EmailProvider.IncomingServer.Add(
                            new ClientConfigEmailProviderIncomingServer
                            {
                                Type = serv.Type,
                                SocketType = serv.SocketType,
                                Hostname = serv.Hostname,
                                Port = serv.Port,
                                Username = serv.Username,
                                Authentication = serv.Authentication
                            });
                    }

                });

                if (!config.EmailProvider.IncomingServer.Any() || !config.EmailProvider.OutgoingServer.Any())
                    return null;

                return config;
            }
        }

        private ClientConfig SearchBusinessVendorsSettings(string domain)
        {
            ClientConfig settingsFromDb = null;

            try
            {
                var dnsLookup = new DnsLookup();

                var mxRecords = dnsLookup.GetDomainMxRecords(domain);

                if (!mxRecords.Any())
                {
                    return null;
                }

                var knownBusinessMxs =
                    Defines.MxToDomainBusinessVendorsList.Where(
                        mx =>
                            mxRecords.FirstOrDefault(
                                r => r.ExchangeDomainName.ToString().ToLowerInvariant().Contains(mx.Key.ToLowerInvariant())) != null)
                        .ToList();

                foreach (var mxXdomain in knownBusinessMxs)
                {
                    settingsFromDb = GetStoredMailBoxSettings(mxXdomain.Value);

                    if (settingsFromDb != null)
                        return settingsFromDb;
                }
            }
            catch (Exception ex)
            {
                Log.Error("SearchBusinessVendorsSettings failed", ex);
            }

            return settingsFromDb;
        }
    }
}
