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
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Web;
using System.Web.Caching;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common.Logging;
using System.Configuration;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Dal;
using ASC.Mail.Server.PostfixAdministration;
using ServerType = ASC.Mail.Server.Dal.ServerType;

namespace ASC.MailServer.DnsCheckerService
{
    partial class DnsCheckerService : ServiceBase
    {
        #region - Declaration -

        readonly ManualResetEvent _mreStop;
        readonly ILogger _log;
        Thread _thread;
        readonly int _waitTimeInMinutes;
        readonly int _checkVerifiedInMinutes;
        readonly int _checkUnverifiedInMinutes;
        readonly int _checkTasksLimit;
        private readonly DalDomainDnsCheck _dnsCheckDal;
        private TimeSpan _tenantCachingPeriod;
        private readonly int _disableUnpaidDomainDays;
        private readonly MailBoxManager _manager;
        readonly int _waitBeforeNextTaskCheckInMilliseconds;

        #endregion

        #region - Constructor -
        public DnsCheckerService()
        {
            InitializeComponent();
            CanStop = true;
            AutoLog = true;
            _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Nlog, "DnsChecker");
            _mreStop = new ManualResetEvent(false);
            _waitTimeInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["check_timeout_in_minutes"]);
            _checkVerifiedInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["check_verified_in_minutes"]);
            _checkUnverifiedInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["check_unverified_in_minutes"]);
            _checkTasksLimit = Convert.ToInt32(ConfigurationManager.AppSettings["check_tasks_limit"]);
            _tenantCachingPeriod = TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["tenant_caching_period_in_minutes"]));
            _disableUnpaidDomainDays = Convert.ToInt32(ConfigurationManager.AppSettings["disable_unpaid_domain_check_in_days"]);
            _waitBeforeNextTaskCheckInMilliseconds = Convert.ToInt32(ConfigurationManager.AppSettings["wait_before_next_task_check_in_milliseconds"]);

            _dnsCheckDal = new DalDomainDnsCheck();
            _manager = new MailBoxManager(0, _log);
        }
        #endregion

        #region - Start / Stop -
        public void StartDaemon()
        {
            OnStart(null);
            Console.CancelKeyPress += delegate {
                    _mreStop.Set();
                };
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                //Start service

                _mreStop.Reset();

                ThreadStart pts = MainThread;

                // Setup thread.
                _thread = new Thread(pts);
                // Start it.
                _thread.Start();
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
            _log.Info("Administrator has stopped the service.");
            // give it a little time to finish any pending work
            if (_thread == null) return;
            _thread.Join(new TimeSpan(0, 0, 30));
            _thread.Abort();
        }
        #endregion

        #region - Threads -
        void MainThread()
        {
            try
            {
                _log.Info("Start service\r\n" +
                          "Configuration: \r\n" +
                          "\t\t- check dns-records of all domains in every {0} minutes;\r\n" +
                          "\t\t- check no more then {1} domain's dns-records at the same time;\r\n" +
                          "\t\t- check unverified dns-records every {2} minutes;\r\n" +
                          "\t\t- check verified dns-records every {3} minutes;\r\n" +
                          "\t\t- tenant caching period {4} minutes\r\n" +
                          "\t\t- disable unpaid domain checks {5} days\r\n" +
                          "\t\t- wait before next task check {6} milliseconds\r\n", 
                          _waitTimeInMinutes, 
                          _checkTasksLimit,
                          _checkUnverifiedInMinutes,
                          _checkVerifiedInMinutes,
                          _tenantCachingPeriod.Minutes,
                          _disableUnpaidDomainDays,
                          _waitBeforeNextTaskCheckInMilliseconds);


                var wait_time = TimeSpan.FromMinutes(_waitTimeInMinutes);

                while (true)
                {
                    try
                    {
                        var dns_tasks = _dnsCheckDal.GetOldUnverifiedTasks(_checkUnverifiedInMinutes, _checkTasksLimit);

                        _log.Info("Found {0} unverified tasks to check.", dns_tasks.Count);

                        if (!dns_tasks.Any() || dns_tasks.Count < _checkTasksLimit)
                        {
                            var dns_verified_tasks = _dnsCheckDal.GetOldVerifiedTasks(_checkVerifiedInMinutes, _checkTasksLimit - dns_tasks.Count);

                            _log.Info("Found {0} verified tasks to check.", dns_verified_tasks.Count);

                            if (dns_verified_tasks.Any())
                                dns_tasks.AddRange(dns_verified_tasks);
                        }

                        if (dns_tasks.Any())
                        {
                            foreach (var dns_task in dns_tasks)
                            {
                                var absence = false;
                                var type = HttpRuntime.Cache.Get(dns_task.tenant.ToString(CultureInfo.InvariantCulture));
                                if (type == null)
                                {
                                    _log.Info("Tenant {0} isn't in cache", dns_task.tenant);
                                    try
                                    {
                                        type = _manager.GetTariffType(dns_task.tenant);
                                        absence = true;
                                    }
                                    catch (Exception e)
                                    {
                                        _log.Error("GetTariffType Exception: {0}", e.ToString());
                                        type = MailBoxManager.TariffType.Active;
                                    }
                                }
                                else
                                {
                                    _log.Info("Tenant {0} is in cache", dns_task.tenant);
                                }

                                switch ((MailBoxManager.TariffType)type)
                                {
                                    case MailBoxManager.TariffType.LongDead:
                                        _log.Info("Tenant {0} is not paid. Removing domain with mailboxes and groups.", dns_task.tenant);
                                        RemoveDomain(dns_task.tenant, dns_task.user, dns_task.domain_id);
                                        break;
                                    case MailBoxManager.TariffType.Overdue:
                                        _log.Info("Tenant {0} is not paid. Stop processing domain.", dns_task.tenant);
                                        _dnsCheckDal.SetDomainDisabled(dns_task.domain_id, _disableUnpaidDomainDays);
                                        break;
                                    default:
                                        if (absence)
                                        {
                                            var tenant_key = dns_task.tenant.ToString(CultureInfo.InvariantCulture);
                                            HttpRuntime.Cache.Remove(tenant_key);
                                            HttpRuntime.Cache.Insert(tenant_key, type, null,
                                                                     DateTime.UtcNow.Add(_tenantCachingPeriod), Cache.NoSlidingExpiration);
                                        }

                                                                                var is_verified = CheckDomainDns(dns_task);
                                        if (is_verified != dns_task.domain_is_verified)
                                        {
                                            _log.Info("Domain '{0}' dns-records changed: they are {1} now.", dns_task.domain_name, is_verified ? "verified" : "unverified");
                                            _dnsCheckDal.SetDomainVerifiedAndChecked(dns_task.domain_id, is_verified);
                                        }
                                        else
                                        {
                                            _log.Info("Domain '{0}' dns-records not changed.", dns_task.domain_name);
                                            _dnsCheckDal.SetDomainChecked(dns_task.domain_id);
                                        }

                                        break;
                                }

                                if (_mreStop.WaitOne(_waitBeforeNextTaskCheckInMilliseconds))
                                    break;
                            }
                        }
                        else
                        {
                            _log.Info("Waiting for {0} minutes for next check...", _waitTimeInMinutes);
                            if (_mreStop.WaitOne(wait_time))
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        _log.Error("Unable to check tasks. Exception:\r\n{0}", ex.ToString());
                        _log.Info("Waiting for {0} minutes for next check...", _waitTimeInMinutes);
                        if (_mreStop.WaitOne(wait_time))
                            break;
                    }

                    if (_mreStop.WaitOne(_waitBeforeNextTaskCheckInMilliseconds))
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Fatal("Main thread Exception:\r\n{0}", ex.ToString());
            }
            finally
            {
                OnStop();
            }
        }
        #endregion

        #region - methods -

        private bool CheckDomainDns(DnsCheckTaskDto task_dto)
        {
            if (string.IsNullOrEmpty(task_dto.domain_name))
                return false;

            var mx_verified = DnsChecker.DnsChecker.IsMxSettedUpCorrectForDomain(task_dto.domain_name, task_dto.mx_record,
                                                                                _log);

            var spf_verified = DnsChecker.DnsChecker.IsTxtRecordCorrect(task_dto.domain_name, task_dto.spf, _log);

            var dkim_verified = DnsChecker.DnsChecker.IsDkimSettedUpCorrectForDomain(task_dto.domain_name,
                                                                                    task_dto.dkim_selector,
                                                                                    task_dto.dkim_public_key, _log);

            return mx_verified && spf_verified && dkim_verified;
        }

        private void RemoveDomain(int tenant, string user, int domain_id)
        {
            var server_dal = new ServerDal(tenant);
            var server_data = server_dal.GetTenantServer();

            if ((ServerType) server_data.type != ServerType.Postfix) return;

            var mailserverfactory = new PostfixFactory();

            var setup = new ServerSetup
                .Builder(server_data.id, tenant, user)
                .SetConnectionString(server_data.connection_string)
                .SetLogger(_log)
                .Build();

            var mail_server = mailserverfactory.CreateServer(setup);

            var domain = mail_server.GetWebDomain(domain_id, mailserverfactory);
            mail_server.DeleteWebDomain(domain, mailserverfactory);
        }

        #endregion
    }
}
