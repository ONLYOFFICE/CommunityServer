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


using ASC.Common.Caching;
using ASC.Core;
using ASC.Web.Files.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ASC.Web.Files.Utils
{
    [DataContract]
    public class FileTracker
    {
        private const string TRACKER = "filesTracker";
        private static readonly ICache cache = AscCache.Default;

        public static readonly TimeSpan TrackTimeout = TimeSpan.FromSeconds(12);
        public static readonly TimeSpan CacheTimeout = TimeSpan.FromSeconds(60);
        public static readonly TimeSpan CheckRightTimeout = TimeSpan.FromMinutes(1);

        [DataMember] private readonly Dictionary<Guid, TrackInfo> _editingBy;


        private FileTracker()
        {
        }

        private FileTracker(Guid tabId, Guid userId, bool newScheme, bool editingAlone)
        {
            _editingBy = new Dictionary<Guid, TrackInfo> {{tabId, new TrackInfo(userId, newScheme, editingAlone)}};
        }


        public static Guid Add(object fileId)
        {
            var tabId = Guid.NewGuid();
            ProlongEditing(fileId, tabId, SecurityContext.CurrentAccount.ID);
            return tabId;
        }

        public static bool ProlongEditing(object fileId, Guid tabId, Guid userId, bool editingAlone = false)
        {
            var checkRight = true;
            var tracker = GetTracker(fileId);
            if (tracker != null && IsEditing(fileId))
            {
                if (tracker._editingBy.Keys.Contains(tabId))
                {
                    tracker._editingBy[tabId].TrackTime = DateTime.UtcNow;
                    checkRight = (DateTime.UtcNow - tracker._editingBy[tabId].CheckRightTime > CheckRightTimeout);
                }
                else
                {
                    tracker._editingBy.Add(tabId, new TrackInfo(userId, tabId == userId, editingAlone));
                }
            }
            else
            {
                tracker = new FileTracker(tabId, userId, tabId == userId, editingAlone);
            }

            SetTracker(fileId, tracker);

            return checkRight;
        }

        public static void Remove(object fileId, Guid tabId = default(Guid), Guid userId = default(Guid))
        {
            var tracker = GetTracker(fileId);
            if (tracker != null)
            {
                if (tabId != default(Guid))
                {
                    tracker._editingBy.Remove(tabId);
                    SetTracker(fileId, tracker);
                    return;
                }
                if (userId != default(Guid))
                {
                    var listForRemove = tracker._editingBy
                                               .Where(b => tracker._editingBy[b.Key].UserId == userId)
                                               .ToList();
                    foreach (var editTab in listForRemove)
                    {
                        tracker._editingBy.Remove(editTab.Key);
                    }
                    SetTracker(fileId, tracker);
                    return;
                }
            }

            SetTracker(fileId, null);
        }

        public static void RemoveAllOther(object fileId)
        {
            var tracker = GetTracker(fileId);
            if (tracker != null)
            {
                var listForRemove = tracker._editingBy
                                           .Where(b => b.Value.UserId != SecurityContext.CurrentAccount.ID)
                                           .ToList();
                if (listForRemove.Count() != tracker._editingBy.Count)
                {
                    foreach (var forRemove in listForRemove)
                    {
                        tracker._editingBy.Remove(forRemove.Key);
                    }
                    SetTracker(fileId, tracker);
                    return;
                }
            }
            SetTracker(fileId, null);
        }

        public static bool IsEditing(object fileId)
        {
            var tracker = GetTracker(fileId);
            if (tracker != null)
            {
                var listForRemove = tracker._editingBy
                                           .Where(e => !e.Value.NewScheme && (DateTime.UtcNow - e.Value.TrackTime).Duration() > TrackTimeout)
                                           .ToList();
                foreach (var editTab in listForRemove)
                {
                    tracker._editingBy.Remove(editTab.Key);
                }

                if (tracker._editingBy.Count == 0)
                {
                    SetTracker(fileId, null);
                    return false;
                }

                SetTracker(fileId, tracker);
                return true;
            }
            SetTracker(fileId, null);
            return false;
        }

        public static bool IsEditingAlone(object fileId)
        {
            var tracker = GetTracker(fileId);
            return tracker != null && tracker._editingBy.Count == 1 && tracker._editingBy.FirstOrDefault().Value.EditingAlone;
        }

        public static void ChangeRight(object fileId, Guid userId, bool check)
        {
            var tracker = GetTracker(fileId);
            if (tracker != null)
            {

                tracker._editingBy.Values
                       .ToList()
                       .ForEach(i =>
                           {
                               if (i.UserId == userId || userId == Guid.Empty)
                               {
                                   i.CheckRightTime = check ? DateTime.MinValue : DateTime.UtcNow;
                               }
                           });
                SetTracker(fileId, tracker);
            }
            else
            {
                SetTracker(fileId, null);
            }
        }

        public static List<Guid> GetEditingBy(object fileId)
        {
            var tracker = GetTracker(fileId);
            return tracker != null && IsEditing(fileId) ? tracker._editingBy.Values.Select(i => i.UserId).Distinct().ToList() : new List<Guid>();
        }

        private static FileTracker GetTracker(object fileId)
        {
            if (fileId != null)
            {
                return cache.Get<FileTracker>(TRACKER + fileId);
            }
            return null;
        }

        private static void SetTracker(object fileId, FileTracker tracker)
        {
            if (fileId != null)
            {
                if (tracker != null)
                {
                    cache.Insert(TRACKER + fileId, tracker, CacheTimeout);
                }
                else
                {
                    cache.Remove(TRACKER + fileId);
                }
            }
        }


        [DataContract]
        internal class TrackInfo
        {
            [DataMember] public DateTime CheckRightTime;

            [DataMember] public DateTime TrackTime;

            [DataMember] public Guid UserId;

            [DataMember] public bool NewScheme;

            [DataMember] public bool EditingAlone;

            public TrackInfo()
            {
            }

            public TrackInfo(Guid userId, bool newScheme, bool editingAlone)
            {
                CheckRightTime = DateTime.UtcNow;
                TrackTime = DateTime.UtcNow;
                NewScheme = newScheme;
                UserId = userId;
                EditingAlone = editingAlone;
            }
        }
    }
}