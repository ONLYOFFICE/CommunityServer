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
using System.Collections.Generic;
using System.Linq;
using ASC.Core;

namespace ASC.Web.Files.Utils
{
    public class FileTracker
    {
        internal class TrackInfo
        {
            public DateTime CheckRightTime;
            public DateTime TrackTime;
            public Guid UserId;
            public bool NewScheme;
            public bool EditingAlone;

            public TrackInfo(Guid userId, bool newScheme, bool editingAlone)
            {
                CheckRightTime = DateTime.UtcNow;
                TrackTime = DateTime.UtcNow;
                NewScheme = newScheme;
                UserId = userId;
                EditingAlone = editingAlone;
            }
        }

        private static readonly Dictionary<string, FileTracker> NowEditing = new Dictionary<string, FileTracker>();
        public static readonly TimeSpan TrackTimeout = TimeSpan.FromSeconds(12);
        public static readonly TimeSpan CheckRightTimeout = TimeSpan.FromMinutes(1);

        private readonly Dictionary<Guid, TrackInfo> _editingBy;
        private bool _fixedVersion;

        private FileTracker(Guid tabId, Guid userId, bool newScheme, bool editingAlone)
        {
            _fixedVersion = false;
            _editingBy = new Dictionary<Guid, TrackInfo> { { tabId, new TrackInfo(userId, newScheme, editingAlone) } };
        }

        public static Guid Add(object fileId, bool fixedVersion)
        {
            var tabId = Guid.NewGuid();
            ProlongEditing(fileId, tabId, fixedVersion, SecurityContext.CurrentAccount.ID);
            return tabId;
        }

        public static bool ProlongEditing(object fileId, Guid tabId, bool fixedVersion, Guid userId, bool editingAlone = false)
        {
            var checkRight = true;
            lock (NowEditing)
            {
                if (IsEditing(fileId))
                {
                    if (NowEditing[fileId.ToString()]._editingBy.Keys.Contains(tabId))
                    {
                        NowEditing[fileId.ToString()]._editingBy[tabId].TrackTime = DateTime.UtcNow;
                        checkRight = (DateTime.UtcNow - NowEditing[fileId.ToString()]._editingBy[tabId].CheckRightTime > CheckRightTimeout);
                    }
                    else
                    {
                        NowEditing[fileId.ToString()]._editingBy.Add(tabId, new TrackInfo(userId, tabId == userId, editingAlone));
                    }
                }
                else
                {
                    NowEditing[fileId.ToString()] = new FileTracker(tabId, userId, tabId == userId, editingAlone);
                }

                if (fixedVersion)
                    NowEditing[fileId.ToString()]._fixedVersion = true;
            }
            return checkRight;
        }

        public static void Remove(object fileId, Guid tabId = default(Guid), Guid userId = default (Guid))
        {
            lock (NowEditing)
            {
                if (NowEditing.ContainsKey(fileId.ToString()))
                {
                    if (tabId != default(Guid)
                        && NowEditing[fileId.ToString()]._editingBy.ContainsKey(tabId))
                    {
                        NowEditing[fileId.ToString()]._editingBy.Remove(tabId);
                        return;
                    }

                    if (userId != default(Guid))
                    {
                        var listForRemove = NowEditing[fileId.ToString()]
                            ._editingBy
                            .ToList() //create copy list
                            .Where(editTab => NowEditing[fileId.ToString()]._editingBy[editTab.Key].UserId == userId);
                        foreach (var editTab in listForRemove)
                        {
                            NowEditing[fileId.ToString()]._editingBy.Remove(editTab.Key);
                        }
                        return;
                    }

                    NowEditing.Remove(fileId.ToString());
                }
            }
        }

        public static void RemoveAllOther(object fileId)
        {
            lock (NowEditing)
            {
                if (NowEditing.ContainsKey(fileId.ToString()))
                {
                    var listForRemove = NowEditing[fileId.ToString()]._editingBy.Where(editTab => editTab.Value.UserId != SecurityContext.CurrentAccount.ID).ToList();
                    if (listForRemove.Count() != NowEditing[fileId.ToString()]._editingBy.Count)
                    {
                        foreach (var forRemove in listForRemove)
                        {
                            NowEditing[fileId.ToString()]._editingBy.Remove(forRemove.Key);
                        }
                    }
                    else
                    {
                        NowEditing.Remove(fileId.ToString());
                    }
                }
            }
        }

        public static bool IsEditing(object fileId)
        {
            lock (NowEditing)
            {
                if (fileId != null && NowEditing.ContainsKey(fileId.ToString()))
                {
                    var listForRemove = NowEditing[fileId.ToString()]
                        ._editingBy
                        .ToList() //create copy list
                        .Where(editTab => !editTab.Value.NewScheme && (DateTime.UtcNow - editTab.Value.TrackTime).Duration() > TrackTimeout);
                    foreach (var editTab in listForRemove)
                    {
                        NowEditing[fileId.ToString()]._editingBy.Remove(editTab.Key);
                    }

                    if (NowEditing[fileId.ToString()]._editingBy.Count == 0)
                    {
                        NowEditing.Remove(fileId.ToString());
                        return false;
                    }

                    return true;
                }
            }
            return false;
        }

        public static bool IsEditingAlone(object fileId)
        {
            lock (NowEditing)
            {
                return
                    fileId != null
                    && NowEditing.ContainsKey(fileId.ToString())
                    && NowEditing[fileId.ToString()]._editingBy.Count == 1
                    && NowEditing[fileId.ToString()]._editingBy.FirstOrDefault().Value.EditingAlone;
            }
        }

        public static bool FixedVersion(object fileId)
        {
            lock (NowEditing)
            {
                if (fileId != null && NowEditing.ContainsKey(fileId.ToString()))
                {
                    return NowEditing[fileId.ToString()]._fixedVersion;
                }
            }
            return false;
        }

        public static void ChangeRight(object fileId, Guid userId, bool check)
        {
            lock (NowEditing)
            {
                if (fileId == null || !NowEditing.ContainsKey(fileId.ToString())) return;

                NowEditing[fileId.ToString()]._editingBy.Values.ToList()
                                             .ForEach(trackInfo =>
                                                          {
                                                              if (trackInfo.UserId == userId || userId == Guid.Empty)
                                                              {
                                                                  trackInfo.CheckRightTime =
                                                                      check ? DateTime.MinValue : DateTime.UtcNow;
                                                              }
                                                          });
            }
        }

        public static List<Guid> GetEditingBy(object fileId)
        {
            lock (NowEditing)
            {
                return IsEditing(fileId)
                           ? NowEditing[fileId.ToString()]._editingBy.Values
                                                          .Select(trackInfo => trackInfo.UserId).Distinct().ToList()
                           : new List<Guid>();
            }
        }
    }
}