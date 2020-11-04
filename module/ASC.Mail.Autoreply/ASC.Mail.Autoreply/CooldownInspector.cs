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
using System.Threading;
using log4net;

namespace ASC.Mail.Autoreply
{
    internal class CooldownInspector
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(CooldownInspector));

        private readonly object _syncRoot = new object();
        private readonly Dictionary<Guid, List<DateTime>> _lastUsagesByUser = new Dictionary<Guid, List<DateTime>>();

        private readonly int _allowedRequests;
        private readonly TimeSpan _duringTime;
        private readonly TimeSpan _cooldownLength;

        private Timer _clearTimer;

        public CooldownInspector(CooldownConfigurationElement config)
        {
            _allowedRequests = config.AllowedRequests;
            _duringTime = config.DuringTimeInterval;
            _cooldownLength = config.Length;
        }

        public void Start()
        {
            if (!IsDisabled() && _clearTimer == null)
            {
                _clearTimer = new Timer(x => ClearExpiredRecords(), null, _cooldownLength, _cooldownLength);
            }
        }

        public void Stop()
        {
            if (_clearTimer != null)
            {
                _clearTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _clearTimer.Dispose();
                _clearTimer = null;
                _lastUsagesByUser.Clear();
            }
        }

        public TimeSpan GetCooldownRemainigTime(Guid userId)
        {
            if (IsDisabled()) return TimeSpan.Zero;

            lock (_syncRoot)
            {
                var lastUsages = GetLastUsages(userId);
                return lastUsages.Count >= _allowedRequests ? _cooldownLength - (DateTime.UtcNow - lastUsages.Max()) : TimeSpan.Zero;
            }
        }

        public void RegisterServiceUsage(Guid userId)
        {
            if (IsDisabled()) return;

            lock (_syncRoot)
            {
                var lastUsages = GetLastUsages(userId);
                lastUsages.Add(DateTime.UtcNow);
                _lastUsagesByUser[userId] = lastUsages;
            }
        }

        private void ClearExpiredRecords()
        {
            lock (_syncRoot)
            {
                _log.Debug("start clearing expired usage records");
                try
                {
                    foreach (var userId in _lastUsagesByUser.Keys.ToList())
                    {
                        _lastUsagesByUser[userId] = GetLastUsages(userId);
                        if (_lastUsagesByUser[userId].Count == 0)
                            _lastUsagesByUser.Remove(userId);
                    }
                }
                catch (Exception error)
                {
                    _log.Error("error while clearing expired usage records", error);
                }
            }
        }

        private List<DateTime> GetLastUsages(Guid userId)
        {
            if (!_lastUsagesByUser.ContainsKey(userId))
                return new List<DateTime>();

            return _lastUsagesByUser[userId]
                .Where(timestamp => timestamp > DateTime.UtcNow - _duringTime)
                .OrderByDescending(timestamp => timestamp)
                .Take(_allowedRequests)
                .ToList();
        }

        private bool IsDisabled()
        {
            return _allowedRequests <= 0 || _duringTime <= TimeSpan.Zero || _cooldownLength <= TimeSpan.Zero;
        }
    }
}
