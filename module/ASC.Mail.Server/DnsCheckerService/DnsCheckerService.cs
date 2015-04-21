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
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.ServiceProcess;
using System.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using System.Configuration;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Dal;
using ASC.Mail.Server.DnsChecker;
using ASC.Mail.Server.PostfixAdministration;
using log4net.Config;
using ServerType = ASC.Mail.Server.Dal.ServerType;

namespace ASC.MailServer.DnsCheckerService
{
    partial class DnsCheckerService : ServiceBase
    {
        #region - Declaration -
        
        readonly ILogger _log;
        readonly int _checkVerifiedInMinutes;
        readonly int _checkUnverifiedInMinutes;
        readonly int _checkTasksLimit;
        private readonly DalDomainDnsCheck _dnsCheckDal;
        private readonly TimeSpan _tenantCachingPeriod;
        private readonly int _disableUnpaidDomainDays;
        readonly int _waitBeforeNextTaskCheckInMilliseconds;

        private Timer _intervalTimer;
        readonly TimeSpan _tsInterval;
        readonly ManualResetEvent _mreStop;

        private static MemoryCache _tenantMemCache;

        private const int TENANT_OVERDUE_DAYS = 30;

        #endregion

        #region - Constructor -

        public DnsCheckerService()
        {
            InitializeComponent();
            CanStop = true;
            AutoLog = true;

            XmlConfigurator.Configure();

            _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "DnsChecker");

            _mreStop = new ManualResetEvent(false);

            _tsInterval = TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["mailserver.check-timeout-in-minutes"]));

            _checkVerifiedInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["mailserver.check-verified-in-minutes"]);
            _checkUnverifiedInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["mailserver.check-unverified-in-minutes"]);
            _checkTasksLimit = Convert.ToInt32(ConfigurationManager.AppSettings["mailserver.check-tasks-limit"]);
            _tenantCachingPeriod = TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["mailserver.tenant-caching-period-in-minutes"]));
            _disableUnpaidDomainDays = Convert.ToInt32(ConfigurationManager.AppSettings["mailserver.disable-unpaid-domain-check-in-days"]);
            _waitBeforeNextTaskCheckInMilliseconds = Convert.ToInt32(ConfigurationManager.AppSettings["mailserver.wait-before-next-task-check-in-milliseconds"]);

            _log.Info("\r\nConfiguration:\r\n" +
                      "\t- check dns-records of all domains in every {0} minutes;\r\n" +
                      "\t- check no more then {1} domain's dns-records at the same time;\r\n" +
                      "\t- check unverified dns-records every {2} minutes;\r\n" +
                      "\t- check verified dns-records every {3} minutes;\r\n" +
                      "\t- tenant caching period {4} minutes\r\n" +
                      "\t- disable unpaid domain checks {5} days\r\n" +
                      "\t- wait before next task check {6} milliseconds\r\n",
                      _tsInterval.TotalMinutes,
                      _checkTasksLimit,
                      _checkUnverifiedInMinutes,
                      _checkVerifiedInMinutes,
                      _tenantCachingPeriod.TotalMinutes,
                      _disableUnpaidDomainDays,
                      _waitBeforeNextTaskCheckInMilliseconds);

            _dnsCheckDal = new DalDomainDnsCheck();

            _tenantMemCache = new MemoryCache("TenantCache");

            if (ConfigurationManager.AppSettings["mail.default-api-scheme"] != null)
            {
                var defaultApiScheme = ConfigurationManager.AppSettings["mail.default-api-scheme"];

                ApiHelper.SetupScheme(defaultApiScheme);
            }
        }

        #endregion

        #region - Start / Stop -

        public void StartConsole()
        {
            OnStart(null);
            Console.CancelKeyPress += (sender, e) => OnStop();
            _mreStop.WaitOne();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _log.Info("Start service\r\n");
                _intervalTimer = new Timer(IntervalTimer_Elapsed, _mreStop, 0, Timeout.Infinite);
            }
            catch (Exception)
            {
                OnStop();
            }
        }

        protected override void OnStop()
        {
            //Service will be stoped in future 30 seconds!
            // signal to tell the worker process to stop
            _mreStop.Set();

            _log.Info("Stop service\r\n");

            if (_intervalTimer == null) return;

            _intervalTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _intervalTimer.Dispose();
            _intervalTimer = null;
        }
        #endregion

        #region - Threads -

        private void IntervalTimer_Elapsed(object state)
        {
            try
            {
                var resetEvent = (ManualResetEvent) state;

                _intervalTimer.Change(Timeout.Infinite, Timeout.Infinite);

                CheckDnsSettings(resetEvent);

            }
            catch (Exception ex)
            {
                _log.Error("IntervalTimer_Elapsed Exception:\r\n{0}]r]n", ex.ToString());
            }
            finally
            {
                _log.Info("Waiting for {0} minutes for next check...", _tsInterval.TotalMinutes);
                _intervalTimer.Change(_tsInterval, _tsInterval);
            }
        }

        #endregion

        #region - methods -

        private void CheckDnsSettings(ManualResetEvent resetEvent)
        {
            var dnsTasks = GetTasks();

            foreach (var dnsTask in dnsTasks)
            {
                try
                {
                    if (!_tenantMemCache.Contains(dnsTask.tenant.ToString(CultureInfo.InvariantCulture)))
                    {
                        _log.Debug("Tenant {0} isn't in cache", dnsTask.tenant);

                        CoreContext.TenantManager.SetCurrentTenant(dnsTask.tenant);

                        var tenantInfo = CoreContext.TenantManager.GetCurrentTenant();

                        SecurityContext.AuthenticateMe(tenantInfo.OwnerId);

                        Defines.TariffType type;

                        try
                        {
                            type = ApiHelper.GetTenantTariff(TENANT_OVERDUE_DAYS);
                        }
                        catch (Exception ex)
                        {
                            _log.Error("GetTenantStatus() Exception:\r\n{0}\r\n", ex.ToString());
                            type = Defines.TariffType.Active;
                        }

                        switch (type)
                        {
                            case Defines.TariffType.LongDead:
                                _log.Info("Tenant {0} is not paid too long. Removing domain with mailboxes, aliases and groups.",
                                          dnsTask.tenant);
                                RemoveDomain(dnsTask.tenant, dnsTask.user, dnsTask.domain_id);
                                continue;
                            case Defines.TariffType.Overdue:
                                _log.Info("Tenant {0} is not paid. Stop processing domain.", dnsTask.tenant);
                                _dnsCheckDal.SetDomainDisabled(dnsTask.domain_id, _disableUnpaidDomainDays);
                                continue;
                            default:
                                _log.Info("Tenant {0} is paid.", dnsTask.tenant);

                                var cacheItem = new CacheItem(dnsTask.tenant.ToString(CultureInfo.InvariantCulture),
                                                              type);
                                var cacheItemPolicy = new CacheItemPolicy
                                    {
                                        RemovedCallback = CacheEntryRemove,
                                        AbsoluteExpiration =
                                            DateTimeOffset.UtcNow.Add(_tenantCachingPeriod)
                                    };
                                _tenantMemCache.Add(cacheItem, cacheItemPolicy);
                                break;
                        }
                    }
                    else
                    {
                        _log.Debug("Tenant {0} is in cache", dnsTask.tenant);
                    }

                    var isVerified = CheckDomainDns(dnsTask);
                    if (isVerified != dnsTask.domain_is_verified)
                    {
                        _log.Info("Domain '{0}' dns-records changed: they are {1} now.",
                                  dnsTask.domain_name, isVerified ? "verified" : "unverified");
                        _dnsCheckDal.SetDomainVerifiedAndChecked(dnsTask.domain_id, isVerified);
                    }
                    else
                    {
                        _log.Info("Domain '{0}' dns-records not changed.", dnsTask.domain_name);
                        _dnsCheckDal.SetDomainChecked(dnsTask.domain_id);
                    }

                    if (resetEvent.WaitOne(_waitBeforeNextTaskCheckInMilliseconds))
                        break;

                }
                catch (Exception ex)
                {
                    _log.Error("Unable to check tasks. Exception:\r\n{0}", ex.ToString());
                }
            }

        }

        private void CacheEntryRemove(CacheEntryRemovedArguments arguments)
        {
            _log.Info("Tenant {0} payment cache is expired.", Convert.ToInt32(arguments.CacheItem.Key));
        }

        private IEnumerable<DnsCheckTaskDto> GetTasks()
        {
            var dnsTasks = new List<DnsCheckTaskDto>();

            try
            {
                dnsTasks = _dnsCheckDal.GetOldUnverifiedTasks(_checkUnverifiedInMinutes, _checkTasksLimit);

                _log.Info("Found {0} unverified tasks to check.", dnsTasks.Count);

                if (!dnsTasks.Any() || dnsTasks.Count < _checkTasksLimit)
                {
                    var dnsVerifiedTasks = _dnsCheckDal.GetOldVerifiedTasks(_checkVerifiedInMinutes,
                                                                            _checkTasksLimit - dnsTasks.Count);

                    _log.Info("Found {0} verified tasks to check.", dnsVerifiedTasks.Count);

                    if (dnsVerifiedTasks.Any())
                        dnsTasks.AddRange(dnsVerifiedTasks);
                }
            }
            catch (Exception ex)
            {
                _log.Error("GetTasks() Exception: \r\n {0} \r\n", ex.ToString());
            }

            return dnsTasks;
        }

        private bool CheckDomainDns(DnsCheckTaskDto taskDto)
        {
            if (string.IsNullOrEmpty(taskDto.domain_name))
                return false;

            var mxVerified = DnsChecker.IsMxRecordCorrect(taskDto.domain_name, taskDto.mx_record,
                                                                                _log);

            var spfVerified = DnsChecker.IsTxtRecordCorrect(taskDto.domain_name, taskDto.spf, _log);

            var dkimVerified = DnsChecker.IsDkimRecordCorrect(taskDto.domain_name,
                                                              taskDto.dkim_selector,
                                                              taskDto.dkim_public_key, _log);

            _log.Info("Domain '{0}' MX={1} SPF={2} DKIM={3}", taskDto.domain_name, mxVerified, spfVerified, dkimVerified);

            return mxVerified && spfVerified && dkimVerified;
        }

        private void RemoveDomain(int tenant, string user, int domainId)
        {
            var serverDal = new ServerDal(tenant);
            var serverData = serverDal.GetTenantServer();

            if ((ServerType) serverData.type != ServerType.Postfix) return;

            var mailserverfactory = new PostfixFactory();

            var setup = new ServerSetup
                .Builder(serverData.id, tenant, user)
                .SetConnectionString(serverData.connection_string)
                .SetLogger(_log)
                .Build();

            var mailServer = mailserverfactory.CreateServer(setup);

            var domain = mailServer.GetWebDomain(domainId, mailserverfactory);
            mailServer.DeleteWebDomain(domain, mailserverfactory);
        }

        #endregion
    }
}
