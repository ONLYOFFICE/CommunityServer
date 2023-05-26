/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using ASC.Core;
using ASC.Migration.Core;
using ASC.Migration.Core.Models;
using ASC.Migration.Core.Models.Api;
using ASC.Migration.NextcloudWorkspace.Models;
using ASC.Migration.NextcloudWorkspace.Models.Parse;
using ASC.Migration.Resources;

namespace ASC.Migration.NextcloudWorkspace
{
    [ApiMigrator("Nextcloud", 6, false)]
    public class NextcloudWorkspaceMigration : AbstractMigration<NCMigrationInfo, NCMigratingUser, NCMigratingContacts, NCMigratingCalendar, NCMigratingFiles, NCMigratingMail>
    {
        private string takeout;
        public string[] tempParse;
        private string tmpFolder;

        public override void Init(string path, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            takeout = Directory.GetFiles(path).LastOrDefault(f => f.EndsWith(".zip"));
            if (string.IsNullOrEmpty(takeout))
            {
                throw new Exception("Folder must not be empty and should contain only .zip files.");
            }

            migrationInfo = new NCMigrationInfo();
            migrationInfo.MigratorName = this.GetType().CustomAttributes.First().ConstructorArguments.First().Value.ToString();
            tmpFolder = path;
        }
        public override MigrationApiInfo Parse()
        {
            ReportProgress(0, MigrationResource.Unzipping);
            try
            {
                try
                {
                    ZipFile.ExtractToDirectory(takeout, tmpFolder);
                }
                catch (Exception ex)
                {
                    Log($"Couldn't to unzip {takeout}", ex);
                }
                ReportProgress(30, MigrationResource.UnzippingFinished);
                var bdFile = "";
                try
                {
                    bdFile = Directory.GetFiles(Directory.GetDirectories(tmpFolder)[0], "*.bak")[0];
                    if (bdFile == null)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception ex)
                {
                    migrationInfo.failedArchives.Add(Path.GetFileName(takeout));
                    Log("Archive must not be empty and should contain .bak files.", ex);
                }
                ReportProgress(40, MigrationResource.DumpParse);
                var users = DBExtractUser(bdFile);
                var progress = 40;
                foreach (var item in users)
                {
                    if (cancellationToken.IsCancellationRequested) { ReportProgress(100, MigrationResource.MigrationCanceled); return null; }
                    ReportProgress(progress, MigrationResource.DataProcessing);
                    progress += 30 / users.Count;
                    if (item.Data.DisplayName != null)
                    {
                        try
                        {
                            string[] userName = item.Data.DisplayName.Split(' ');
                            item.Data.DisplayName = userName.Length > 1 ? String.Format("{0} {1}", userName[0], userName[1]).Trim() : userName[0].Trim();
                            var user = new NCMigratingUser(item.Uid, item, Directory.GetDirectories(tmpFolder)[0], Log);
                            user.Parse();
                            foreach (var element in user.ModulesList)
                            {
                                if (!migrationInfo.Modules.Exists(x => x.MigrationModule == element.MigrationModule))
                                {
                                    migrationInfo.Modules.Add(new MigrationModules(element.MigrationModule, element.Module));
                                }
                            }
                            migrationInfo.Users.Add(item.Uid, user);
                        }
                        catch (Exception ex)
                        {
                            Log($"Couldn't parse user {item.Data.DisplayName}", ex);
                        }
                    }
                }

                var groups = DBExtractGroup(bdFile);
                progress = 80;
                foreach (var item in groups)
                {
                    if (cancellationToken.IsCancellationRequested) { ReportProgress(100, MigrationResource.MigrationCanceled); return null; }
                    ReportProgress(progress, MigrationResource.DataProcessing);
                    progress += 10 / groups.Count;
                    var group = new NCMigratingGroups(item, Log);
                    group.Parse();
                    if (group.Module.MigrationModule != null)
                    {
                        migrationInfo.Groups.Add(group);
                        if (!migrationInfo.Modules.Exists(x => x.MigrationModule == group.Module.MigrationModule))
                        {
                            migrationInfo.Modules.Add(new MigrationModules(group.Module.MigrationModule, group.Module.Module));
                        }
                    }
                }
                ReportProgress(90, MigrationResource.ClearingTemporaryData);
            }
            catch (Exception ex)
            {
                migrationInfo.failedArchives.Add(Path.GetFileName(takeout));
                Log($"Couldn't parse users from {Path.GetFileNameWithoutExtension(takeout)} archive", ex);
            }
            ReportProgress(100, MigrationResource.DataProcessingCompleted);
            return migrationInfo.ToApiInfo();
        }

        public List<NCGroup> DBExtractGroup(string dbFile)
        {
            var groups = new List<NCGroup>();

            var sqlFile = File.ReadAllText(dbFile);

            var groupList = GetDumpChunk("oc_groups", sqlFile);
            if (groupList == null) return groups;

            foreach (var group in groupList)
            {
                groups.Add(new NCGroup
                {
                    GroupGid = group.Split(',').First().Trim('\''),
                    UsersUid = new List<string>()
                });
            }

            var usersInGroups = GetDumpChunk("oc_group_user", sqlFile);
            foreach (var user in usersInGroups)
            {
                var userGroupGid = user.Split(',').First().Trim('\'');
                var userUid = user.Split(',').Last().Trim('\'');
                groups.Find(ggid => userGroupGid == ggid.GroupGid).UsersUid.Add(userUid);
            }

            return groups;
        }

        public List<NCUser> DBExtractUser(string dbFile)
        {
            var userDataList = new Dictionary<string, NCUser>();

            var sqlFile = File.ReadAllText(dbFile);

            var accounts = GetDumpChunk("oc_accounts", sqlFile);
            if (accounts == null) return userDataList.Values.ToList();

            foreach (var account in accounts)
            {
                var userId = account.Split(',').First().Trim('\'');

                userDataList.Add(userId, new NCUser
                {
                    Uid = userId,
                    Data = new NCUserData(),
                    Addressbooks = null,
                    Calendars = new List<NCCalendars>(),
                    Storages = new NCStorages()
                });
            }

            var accountsData = GetDumpChunk("oc_accounts_data", sqlFile);
            if (accountsData != null)
            {
                foreach (var accountData in accountsData)
                {
                    var values = accountData.Split(',')
                        .Select(s => s.Trim('\'')).ToArray();
                    userDataList.TryGetValue(values[1], out var user);
                    if (user == null) continue;

                    switch (values[2])
                    {
                        case "displayname":
                            user.Data.DisplayName = values[3];
                            break;
                        case "address":
                            user.Data.Address = values[3];
                            break;
                        case "email":
                            user.Data.Email = values[3];
                            break;
                        case "phone":
                            user.Data.Phone = values[3];
                            break;
                        case "twitter":
                            user.Data.Twitter = values[3];
                            break;
                    }
                }
            }
            else
            {
                throw new Exception();
            }

            var calendarsData = GetDumpChunk("oc_calendars", sqlFile);
            if (calendarsData != null)
            {
                foreach (var calendarData in calendarsData)
                {
                    var values = calendarData.Split(',')
                        .Select(s => s.Trim('\'')).ToArray();
                    var uid = values[1].Split('/').Last();
                    userDataList.TryGetValue(uid, out var user);
                    if (user == null) continue;

                    user.Calendars.Add(new NCCalendars()
                    {
                        Id = int.Parse(values[0]),
                        CalendarObject = new List<NCCalendarObjects>(),
                        DisplayName = values[2]
                    });
                }
            }

            var calendars = userDataList.Values
                .SelectMany(u => u.Calendars)
                .ToDictionary(c => c.Id, c => c);
            var calendarObjects = GetDumpChunk("oc_calendarobjects", sqlFile);
            if (calendarObjects != null)
            {
                foreach (var calendarObject in calendarObjects)
                {
                    var values = calendarObject.Split(',')
                        .Select(s => s.Trim('\'')).ToArray();
                    var calId = int.Parse(values[3]);
                    calendars.TryGetValue(calId, out var cal);
                    if (cal == null) continue;

                    cal.CalendarObject.Add(new NCCalendarObjects()
                    {
                        Id = int.Parse(values[0]),
                        CalendarData = Encoding.UTF8.GetBytes(values[1]
                                                        .Replace("\\r", "")
                                                        .Replace("\\n", "\n")),
                    });
                }
            }

            var addressBooks = GetDumpChunk("oc_addressbooks", sqlFile);
            if (addressBooks != null)
            {
                foreach (var addressBook in addressBooks)
                {
                    var values = addressBook.Split(',')
                        .Select(s => s.Trim('\'')).ToArray();
                    var uid = values[1].Split('/').Last();
                    userDataList.TryGetValue(uid, out var user);
                    if (user == null) continue;
                    user.Addressbooks = new NCAddressbooks();
                    user.Addressbooks.Id = int.Parse(values[0]);
                    user.Addressbooks.Cards = new List<NCCards>();
                }
            }

            var addressBooksDict = userDataList.Values
                .Select(u => u.Addressbooks)
                .Where(x => x != null)
                .ToDictionary(b => b.Id, b => b);
            var cards = GetDumpChunk("oc_cards", sqlFile);
            if (cards != null)
            {
                foreach (var card in cards)
                {
                    var values = card.Split(',')
                        .Select(s => s.Trim('\'')).ToArray();
                    var bookId = int.Parse(values[1]);
                    addressBooksDict.TryGetValue(bookId, out var book);
                    if (book == null) continue;

                    book.Cards.Add(new NCCards()
                    {
                        Id = int.Parse(values[0]),
                        CardData = Encoding.UTF8.GetBytes(values[2]
                                                        .Replace("\\r", "")
                                                        .Replace("\\n", "\n")),
                    });
                }
            }

            var storages = GetDumpChunk("oc_storages", sqlFile);
            if (storages != null)
            {
                foreach (var storage in storages)
                {
                    var values = storage.Split(',')
                               .Select(s => s.Trim('\'')).ToArray();
                    var uid = values[1].Split(':').Last();
                    userDataList.TryGetValue(uid, out var user);
                    if (user == null) continue;

                    user.Storages.NumericId = int.Parse(values[0]);
                    user.Storages.Id = values[1];
                    user.Storages.FileCache = new List<NCFileCache>();
                }
            }

            var storagesDict = userDataList.Values
                .Select(u => u.Storages)
                .ToDictionary(s => s.NumericId, s => s);
            var fileCaches = GetDumpChunk("oc_filecache", sqlFile);
            if (fileCaches != null)
            {
                foreach (var cache in fileCaches)
                {
                    var values = cache.Split(',')
                               .Select(s => s.Trim('\'')).ToArray();
                    var storageId = int.Parse(values[1]);
                    storagesDict.TryGetValue(storageId, out var storage);
                    if (storage == null) continue;

                    storage.FileCache.Add(new NCFileCache()
                    {
                        FileId = int.Parse(values[0]),
                        Path = values[2],
                        Share = new List<NCShare>()
                    });
                }
            }

            var files = userDataList.Values
                .SelectMany(u => u.Storages.FileCache)
                .ToDictionary(f => f.FileId, f => f);
            var shares = GetDumpChunk("oc_share", sqlFile);
            if (shares != null)
            {
                foreach (var share in shares)
                {
                    var values = share.Split(',')
                               .Select(s => s.Trim('\'')).ToArray();
                    var fileId = int.Parse(values[10]);
                    files.TryGetValue(fileId, out var file);
                    if (file == null) continue;

                    file.Share.Add(new NCShare()
                    {
                        Id = int.Parse(values[0]),
                        ShareWith = values[2],
                        Premissions = int.Parse(values[12])
                    });
                }
            }

            return userDataList.Values.ToList();
        }

        private IEnumerable<string> GetDumpChunk(string tableName, string dump)
        {
            var regex = new Regex($"INSERT INTO `{tableName}` VALUES (.*);");
            var match = regex.Match(dump);
            if (!match.Success) return null;

            var entryRegex = new Regex(@"(\(.*?\))[,;]");
            var accountDataMatches = entryRegex.Matches(match.Groups[1].Value + ";");
            return accountDataMatches.Cast<Match>()
                .Select(m => m.Groups[1].Value.Trim(new[] { '(', ')' }));
        }

        public override void Migrate(MigrationApiInfo migrationApiInfo)
        {
            ReportProgress(0, MigrationResource.PreparingForMigration);
            migrationInfo.Merge(migrationApiInfo);

            var usersForImport = migrationInfo.Users
                .Where(u => u.Value.ShouldImport)
                .Select(u => u.Value);
            importedUsers = new List<Guid>();
            var failedUsers = new List<NCMigratingUser>();
            var usersCount = usersForImport.Count();
            var progressStep = 25 / usersCount;
            var i = 1;
            foreach (var user in usersForImport)
            {
                if (cancellationToken.IsCancellationRequested) { ReportProgress(100, MigrationResource.MigrationCanceled); return; }
                ReportProgress(GetProgress() + progressStep, String.Format(MigrationResource.UserMigration, user.DisplayName, i++, usersCount));
                try
                {
                    user.dataСhange(migrationApiInfo.Users.Find(element => element.Key == user.Key));
                    user.Migrate();
                    importedUsers.Add(user.Guid);
                }
                catch (Exception ex)
                {
                    failedUsers.Add(user);
                    Log($"Couldn't migrate user {user.DisplayName} ({user.Email})", ex);
                }
            }

            var groupsForImport = migrationInfo.Groups
                .Where(g => g.ShouldImport)
                .Select(g => g);
            var groupsCount = groupsForImport.Count();
            if (groupsCount != 0)
            {
                progressStep = 25 / groupsForImport.Count();
                i = 1;
                foreach (var group in groupsForImport)
                {
                    if (cancellationToken.IsCancellationRequested) { ReportProgress(100, MigrationResource.MigrationCanceled); return; }
                    ReportProgress(GetProgress() + progressStep, String.Format(MigrationResource.GroupMigration, group.GroupName, i++, groupsCount));
                    try
                    {
                        group.usersGuidList = migrationInfo.Users
                        .Where(user => group.UserUidList.Exists(u => user.Key == u))
                        .Select(u => u)
                        .ToDictionary(k => k.Key, v => v.Value.Guid);
                        group.Migrate();
                    }
                    catch (Exception ex)
                    {
                        Log($"Couldn't migrate group {group.GroupName} ", ex);
                    }
                }
            }

            i = 1;
            progressStep = 50 / usersForImport.Count();
            foreach (var user in usersForImport)
            {
                if (cancellationToken.IsCancellationRequested) { ReportProgress(100, MigrationResource.MigrationCanceled); return; }
                if (failedUsers.Contains(user))
                {
                    ReportProgress(GetProgress() + progressStep, String.Format(MigrationResource.UserSkipped, user.DisplayName, i, usersCount));
                    continue;
                }

                var smallStep = progressStep / 2;

                try
                {
                    user.MigratingContacts.Migrate();
                }
                catch (Exception ex)
                {
                    Log($"Couldn't migrate user {user.DisplayName} ({user.Email}) contacts", ex);
                }
                finally
                {
                    ReportProgress(GetProgress() + smallStep, String.Format(MigrationResource.MigratingUserContacts, user.DisplayName, i, usersCount));
                }

                try
                {
                    var currentUser = SecurityContext.CurrentAccount;
                    SecurityContext.AuthenticateMe(user.Guid);
                    user.MigratingFiles.SetUsersDict(usersForImport.Except(failedUsers));
                    user.MigratingFiles.SetGroupsDict(groupsForImport);
                    user.MigratingFiles.Migrate();
                    SecurityContext.AuthenticateMe(currentUser.ID);
                }
                catch (Exception ex)
                {
                    Log($"Couldn't migrate user {user.DisplayName} ({user.Email}) files", ex);
                }
                finally
                {
                    ReportProgress(GetProgress() + smallStep, String.Format(MigrationResource.MigratingUserFiles, user.DisplayName, i, usersCount));
                }
                i++;
            }

            if (Directory.Exists(tmpFolder))
            {
                Directory.Delete(tmpFolder, true);
            }
            ReportProgress(100, MigrationResource.MigrationCompleted);
        }
    }
}
