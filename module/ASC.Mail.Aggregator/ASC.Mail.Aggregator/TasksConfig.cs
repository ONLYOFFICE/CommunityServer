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

namespace ASC.Mail.Aggregator
{
    public class TasksConfig
    {
        public enum AggregateModeType
        {
            All,
            External,
            Internal
        };

        private readonly TimeSpan _activeInterval;
        private readonly List<string> _workOnUsersOnly;
        private readonly AggregateModeType _aggregateMode;
        private readonly bool _enableSignalr;
        private readonly int _chunkOfUidl;
        private readonly int _maxMessagesPerSession;
        private readonly int _maxTasksAtOnce;
        private readonly TimeSpan _overdueAccountDelay;
        private readonly TimeSpan _quotaEndedDelay;
        private readonly TimeSpan _tenantCachingPeriod;
        private readonly TimeSpan _queueLifetimeInteravl;
        private readonly bool _showActiveUpLogs;
        private readonly double _inactiveMailboxesRatio;
        private readonly TimeSpan _authErrorWarningTimeout;
        private readonly TimeSpan _authErrorDisableMailboxTimeout;
        private readonly int _tenantOverdueDays;
        private readonly long _minQuotaBalance;
        private readonly bool _saveOriginalMessage;

        public TimeSpan ActiveInterval {
            get { return _activeInterval; }
        }

        public List<string> WorkOnUsersOnly
        {
            get { return _workOnUsersOnly; }
        }

        public AggregateModeType AggregateMode
        {
            get { return _aggregateMode; }
        }

        public bool EnableSignalr
        {
            get { return _enableSignalr; }
        }

        public int ChunkOfPop3Uidl {
            get { return _chunkOfUidl; }
        }

        public int MaxMessagesPerSession
        {
            get { return _maxMessagesPerSession; }
        }

        public int MaxTasksAtOnce
        {
            get { return _maxTasksAtOnce; }
        }

        public TimeSpan OverdueAccountDelay {
            get { return _overdueAccountDelay; }
        }

        public TimeSpan QuotaEndedDelay
        {
            get { return _quotaEndedDelay; }
        }

        public TimeSpan TenantCachingPeriod
        {
            get { return _tenantCachingPeriod; }
        }

        public TimeSpan QueueLifetime
        {
            get { return _queueLifetimeInteravl; }
        }

        public bool ShowActiveUpLogs
        {
            get { return _showActiveUpLogs; }
        }

        public double InactiveMailboxesRatio
        {
            get { return _inactiveMailboxesRatio; }
        }

        public TimeSpan AuthErrorWarningTimeout
        {
            get { return _authErrorWarningTimeout; }
        }

        public TimeSpan AuthErrorDisableMailboxTimeout
        {
            get { return _authErrorDisableMailboxTimeout; }
        }

        public int TenantOverdueDays
        {
            get { return _tenantOverdueDays; }
        }

        public long MinQuotaBalance
        {
            get { return _minQuotaBalance; }
        }

        public bool SaveOriginalMessage
        {
            get { return _saveOriginalMessage; }
        }

        public class Builder
        {
            internal TimeSpan activeInterval;
            internal List<string> workOnUsersOnly;
            internal AggregateModeType aggregateMode;
            internal int chunkOfUidl;
            internal bool enableSignalr;
            internal int maxMessagesPerSession;
            internal int maxTasksAtOnce;
            internal TimeSpan overdueAccountDelay;
            internal TimeSpan quotaEndedDelay;
            internal TimeSpan tenantCachingPeriod;
            internal TimeSpan queueLifetime;
            internal bool showActiveUpLogs;
            internal int inactiveMailboxesRatio;
            internal TimeSpan authErrorWarningTimeout;
            internal TimeSpan authErrorDisableMailboxTimeout;
            internal int tenantOverdueDays;
            internal long minQuotaBalance;
            internal bool saveOriginalMessage;

            public virtual Builder SetActiveInterval(TimeSpan activeIntervalObj)
            {
                activeInterval = activeIntervalObj;
                return this;
            }

            public virtual Builder SetUsersToWorkOn(List<string> workOnUsersOnlyObj)
            {
                workOnUsersOnly = workOnUsersOnlyObj;
                return this;
            }

            public virtual Builder SetAggregateMode(string aggregateModeObj)
            {
                switch (aggregateModeObj)
                {
                    case "external":
                        aggregateMode = AggregateModeType.External;
                        break;
                    case "internal":
                        aggregateMode = AggregateModeType.Internal;
                        break;
                    default:
                        aggregateMode = AggregateModeType.All;
                        break;
                }
                return this;
            }

            public virtual Builder SetEnableSignalr(bool enableSignalrObj)
            {
                enableSignalr = enableSignalrObj;
                return this;
            }

            public virtual Builder SetChunkOfPop3CheckUidLinDb(int chunkOfUidlObj)
            {
                chunkOfUidl = chunkOfUidlObj;
                return this;
            }

            public virtual Builder SetMaxMessagesPerSession(int maxMessagesPerSessionObj)
            {
                maxMessagesPerSession = maxMessagesPerSessionObj;
                return this;
            }
            public virtual Builder SetMaxTasksAtOnce(int maxTasksAtOnceObj)
            {
                maxTasksAtOnce = maxTasksAtOnceObj;
                return this;
            }

            public virtual Builder SetOverdueAccountDelay(TimeSpan overdueAccountDelayObj)
            {
                overdueAccountDelay = overdueAccountDelayObj;
                return this;
            }

            public virtual Builder SetQuotaEndedDelay(TimeSpan quotaEndedDelayObj)
            {
                quotaEndedDelay = quotaEndedDelayObj;
                return this;
            }

            public virtual Builder SetTenantCachingPeriod(TimeSpan tenantCachingPeriodObj)
            {
                tenantCachingPeriod = tenantCachingPeriodObj;
                return this;
            }

            public virtual Builder SetQueueLifetime(TimeSpan queueLifetimeObj)
            {
                queueLifetime = queueLifetimeObj;
                return this;
            }

            public virtual Builder SetShowActiveUpLogs(bool showActiveUpLogsObj)
            {
                showActiveUpLogs = showActiveUpLogsObj;
                return this;
            }

            public virtual Builder SetInactiveMailboxesRatio(int inactiveMailboxesRatioObj)
            {
                inactiveMailboxesRatio = inactiveMailboxesRatioObj;
                return this;
            }

            public virtual Builder SetAuthErrorWarningTimeout(TimeSpan authErrorWarningTimeoutObj)
            {
                authErrorWarningTimeout = authErrorWarningTimeoutObj;
                return this;
            }

            public virtual Builder SetAuthErrorDisableMailboxTimeout(TimeSpan authErrorDisableMailboxTimeoutObj)
            {
                authErrorDisableMailboxTimeout = authErrorDisableMailboxTimeoutObj;
                return this;
            }

            public virtual Builder SetTenantOverdueDays(int tenantOverdueDaysObj)
            {
                tenantOverdueDays = tenantOverdueDaysObj;
                return this;
            }

            public virtual Builder SetMinQuotaBalance(long minQuotaBalanceObj)
            {
                minQuotaBalance = minQuotaBalanceObj;
                return this;
            }

            public virtual Builder SetSaveOriginalMessageFlag(bool saveOriginalMessageObj)
            {
                saveOriginalMessage = saveOriginalMessageObj;
                return this;
            }

            public TasksConfig Build()
            {
                return new TasksConfig(this);
            }

        }

        private TasksConfig(Builder builder)
        {
            _activeInterval = builder.activeInterval;
            _workOnUsersOnly = builder.workOnUsersOnly ?? new List<string>();
            _aggregateMode = builder.aggregateMode;
            _enableSignalr = builder.enableSignalr;
            _chunkOfUidl = builder.chunkOfUidl > 0 ? builder.chunkOfUidl : 100;
            _maxMessagesPerSession = builder.maxMessagesPerSession > 0 ? builder.maxMessagesPerSession : 1;
            _maxTasksAtOnce = builder.maxTasksAtOnce > 0 ? builder.maxTasksAtOnce : 1;
            _overdueAccountDelay = builder.overdueAccountDelay;
            _quotaEndedDelay = builder.quotaEndedDelay;
            _tenantCachingPeriod = builder.tenantCachingPeriod;
            _queueLifetimeInteravl = builder.queueLifetime == TimeSpan.MinValue ? TimeSpan.FromSeconds(30) : builder.queueLifetime;
            _showActiveUpLogs = builder.showActiveUpLogs;

            if (builder.inactiveMailboxesRatio < 0)
                builder.inactiveMailboxesRatio = 0;
            if (builder.inactiveMailboxesRatio > 100)
                builder.inactiveMailboxesRatio = 100;

            _inactiveMailboxesRatio = builder.inactiveMailboxesRatio;
            _authErrorWarningTimeout = builder.authErrorWarningTimeout;
            _authErrorDisableMailboxTimeout = builder.authErrorDisableMailboxTimeout;
            _tenantOverdueDays = builder.tenantOverdueDays;
            _minQuotaBalance = builder.minQuotaBalance;
            _saveOriginalMessage = builder.saveOriginalMessage;
        }
    }
}
