using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ASC.Core;
using ASC.Migration.Core;
using ASC.Migration.Core.Models;
using ASC.Migration.Core.Models.Api;
using ASC.Migration.GoogleWorkspace.Models;
using ASC.Migration.Resources;

namespace ASC.Migration.GoogleWorkspace
{
    [ApiMigrator("GoogleWorkspace")]
    public class GoogleWorkspaceMigration : AbstractMigration<GwsMigrationInfo, GwsMigratingUser, GwsMigratingContacts, GwsMigratingCalendar, GwsMigratingFiles, GwsMigratingMail>
    {
        private string[] takeouts;

        public override void Init(string path, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            List<string> tempTakeouts = new List<string>();
            var files = Directory.GetFiles(path);
            if (!files.Any() || !files.Any(f => f.EndsWith(".zip")))
            {
                throw new Exception("Folder must not be empty and should contain .zip files.");
            }
            foreach(var item in files)
            {
                if(item.EndsWith(".zip"))
                {
                    tempTakeouts.Add(item);
                }
            }
            takeouts = tempTakeouts.ToArray();

            migrationInfo = new GwsMigrationInfo();
            migrationInfo.MigratorName = this.GetType().CustomAttributes.First().ConstructorArguments.First().Value.ToString();
        }

        public override Task<MigrationApiInfo> Parse()
        {
            ReportProgress(0, MigrationResource.StartOfDataProcessing);

            var progressStep = 100 / takeouts.Length;
            var i = 1;
            foreach (var takeout in takeouts)
            {
                if (cancellationToken.IsCancellationRequested) { ReportProgress(100, MigrationResource.MigrationCanceled); return null; }
                ReportProgress(GetProgress() + progressStep, MigrationResource.DataProcessing + $" {takeout} ({i++}/{takeouts.Length})");
                var tmpFolder = Path.Combine(TempPath.GetTempPath(), Path.GetFileNameWithoutExtension(takeout));
                try
                {
                    ZipFile.ExtractToDirectory(takeout, tmpFolder);

                    var rootFolder = Path.Combine(tmpFolder, "Takeout");

                    if (!Directory.Exists(rootFolder))
                    {
                        throw new Exception("Takeout zip does not contain root 'Takeout' folder.");
                    }
                    var directories = Directory.GetDirectories(rootFolder);
                    if (directories.Length == 1 && directories[0].Split(Path.DirectorySeparatorChar).Last() == "Groups")
                    {
                        var group = new GWSMigratingGroups(rootFolder, Log);
                        group.Parse();
                        if(group.Module.MigrationModule != null)
                        {
                            migrationInfo.Groups.Add(group);
                            if (!migrationInfo.Modules.Exists(x => x.MigrationModule == group.Module.MigrationModule))
                            {
                                migrationInfo.Modules.Add(new MigrationModules(group.Module.MigrationModule, group.Module.Module));
                            }
                        }
                    }
                    else
                    {
                        var user = new GwsMigratingUser(takeout, rootFolder, Log);
                        user.Parse();
                        foreach (var element in user.ModulesList)
                        {
                            if (!migrationInfo.Modules.Exists(x => x.MigrationModule == element.MigrationModule))
                            {
                                migrationInfo.Modules.Add(new MigrationModules(element.MigrationModule, element.Module));
                            }
                        }
                        migrationInfo.Users.Add(takeout, user);
                    }
                }
                catch (Exception ex)
                {
                    migrationInfo.failedArchives.Add(Path.GetFileName(takeout));
                    Log($"Couldn't parse user from {Path.GetFileNameWithoutExtension(takeout)} archive", ex);
                }
                finally
                {
                    if (Directory.Exists(tmpFolder))
                    {
                        Directory.Delete(tmpFolder, true);
                    }
                }
            }
            ReportProgress(100, MigrationResource.DataProcessingCompleted);
            return Task.FromResult(migrationInfo.ToApiInfo());
        }

        public override Task Migrate(MigrationApiInfo migrationApiInfo)
        {
            ReportProgress(0, MigrationResource.PreparingForMigration);
            migrationInfo.Merge(migrationApiInfo);

            var usersForImport = migrationInfo.Users
                .Where(u => u.Value.ShouldImport)
                .Select(u => u.Value);

            importedUsers = new List<Guid>();
            var failedUsers = new List<GwsMigratingUser>();
            var usersCount = usersForImport.Count();
            var progressStep = 25 / usersCount;
            // Add all users first
            var i = 1;
            foreach (var user in usersForImport)
            {
                if (cancellationToken.IsCancellationRequested) { ReportProgress(100, MigrationResource.MigrationCanceled); return null; }
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
                //Create all groups
                i = 1;
                foreach (var group in groupsForImport)
                {
                    if (cancellationToken.IsCancellationRequested) { ReportProgress(100, MigrationResource.MigrationCanceled); return null; }
                    ReportProgress(GetProgress() + progressStep, String.Format(MigrationResource.GroupMigration, group.GroupName, i++, groupsCount));
                    try
                    {
                        group.Migrate();
                    }
                    catch (Exception ex)
                    {
                        Log($"Couldn't migrate group {group.GroupName} ", ex);
                    }
                }
            }

            // Add files, contacts and other stuff
            i = 1;
            foreach (var user in usersForImport)
            {
                if (cancellationToken.IsCancellationRequested) { ReportProgress(100, MigrationResource.MigrationCanceled); return null; }
                if (failedUsers.Contains(user))
                {
                    ReportProgress(GetProgress() + progressStep, String.Format(MigrationResource.UserSkipped, user.DisplayName, i, usersCount));
                    continue;
                }

                var smallStep = progressStep / 4;

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

                /*try
                {
                    user.MigratingCalendar.Migrate();
                }
                catch (Exception ex)
                {
                    Log($"Couldn't migrate user {user.DisplayName} ({user.Email}) calendar", ex);
                }
                finally
                {
                    ReportProgress(GetProgress() + smallStep, String.Format(MigrationResource.UserCalendarMigration, user.DisplayName, i, usersCount));
                }*/

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

            foreach (var item in takeouts)
            {
                File.Delete(item);
            }

            ReportProgress(100, MigrationResource.MigrationCompleted);
            return Task.CompletedTask;
        }
    }
}
