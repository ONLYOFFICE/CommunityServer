/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

        public Dictionary<string, string> MxToDomainBusinessVendorsList
        {
            get
            {
                var list = new Dictionary<string, string>
                {
                    {".outlook.", "office365.com"},
                    {".google.", "gmail.com"},
                    {".yandex.", "yandex.ru"},
                    {".mail.ru", "mail.ru"},
                    {".yahoodns.", "yahoo.com"}
                };

                try
                {
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["mail.busines-vendors-mx-domains"]))
                    // ".outlook.:office365.com|.google.:gmail.com|.yandex.:yandex.ru|.mail.ru:mail.ru|.yahoodns.:yahoo.com"
                    {
                        list = ConfigurationManager.AppSettings["mail.busines-vendors-mx-domains"]
                            .Split('|')
                            .Select(s => s.Split(':'))
                            .ToDictionary(s => s[0], s => s[1]);
                    }
                }
                catch(Exception ex)
                {
                    Log.Error("MxToDomainBusinessVendorsList failed", ex);
                }

                return list;
            }

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
                    MxToDomainBusinessVendorsList.Where(
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
