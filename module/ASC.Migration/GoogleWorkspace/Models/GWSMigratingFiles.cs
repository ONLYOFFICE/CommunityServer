using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Migration.Core.Models;
using ASC.Migration.GoogleWorkspace.Models.Parse;
using ASC.Migration.Resources;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.WCFService;

using Newtonsoft.Json;

using ASCFile = ASC.Files.Core.File;
using ASCShare = ASC.Files.Core.Security.FileShare;
using File = System.IO.File;

namespace ASC.Migration.GoogleWorkspace.Models
{
    public class GwsMigratingFiles : MigratingFiles
    {
        public override int FoldersCount => foldersCount;

        public override int FilesCount => filesCount;

        public override long BytesTotal => bytesTotal;
        public override string ModuleName => MigrationResource.GoogleModuleNameDocuments;

        private string newParentFolder;

        public override void Parse()
        {
            var drivePath = Path.Combine(rootFolder, "Drive");
            if (!Directory.Exists(drivePath)) return;

            var entries = Directory.GetFileSystemEntries(drivePath, "*", SearchOption.AllDirectories);

            List<string> filteredEntries = new List<string>();
            files = new List<string>();
            folders = new List<string>();
            folderCreation = folderCreation != null ? folderCreation : DateTime.Now.ToString("dd.MM.yyyy");
            newParentFolder = ModuleName + " " + folderCreation;

            foreach (var entry in entries)
            {
                if (ShouldIgnoreFile(entry, entries)) continue;

                filteredEntries.Add(entry);
            }

            foreach (var entry in filteredEntries)
            {
                var attr = File.GetAttributes(entry);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    foldersCount++;
                    folders.Add(newParentFolder + Path.DirectorySeparatorChar.ToString() + entry.Substring(drivePath.Length + 1));
                }
                else
                {
                    filesCount++;
                    var fi = new FileInfo(entry);
                    bytesTotal += fi.Length;
                    files.Add(newParentFolder + Path.DirectorySeparatorChar.ToString() + entry.Substring(drivePath.Length + 1));
                }
            }
        }

        public void SetUsersDict(IEnumerable<GwsMigratingUser> users)
        {
            this.users = users.ToDictionary(user => user.Email, user => user);
        }
        public void SetGroupsDict(IEnumerable<GWSMigratingGroups> groups)
        {
            this.groups = groups.ToDictionary(group => group.GroupName, group => group);
        }

        public override void Migrate()
        {
            if (!ShouldImport) return;

            var tmpFolder = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(user.Key));
            try
            {
                ZipFile.ExtractToDirectory(user.Key, tmpFolder);
                var drivePath = Path.Combine(tmpFolder, "Takeout", "Drive");

                // Create all folders first
                var foldersDict = new Dictionary<string, Folder>();
                if (folders != null && folders.Count != 0)
                {
                    foreach (var folder in folders)
                    {
                        var split = folder.Split(Path.DirectorySeparatorChar); // recursivly create all the folders
                        for (var i = 0; i < split.Length; i++)
                        {
                            var path = string.Join(Path.DirectorySeparatorChar.ToString(), split.Take(i + 1));
                            if (foldersDict.ContainsKey(path)) continue; // skip folder if it was already created as a part of another path
                            var parentId = i == 0 ? Global.FolderMy : foldersDict[string.Join(Path.DirectorySeparatorChar.ToString(), split.Take(i))].ID;
                            try
                            {
                                var createdFolder = Global.FileStorageService.CreateNewFolder(parentId.ToString(), split[i]);
                                path = path.Contains(newParentFolder + Path.DirectorySeparatorChar.ToString()) ? path.Replace(newParentFolder + Path.DirectorySeparatorChar.ToString(), "") : path;
                                foldersDict.Add(path, createdFolder);
                            }
                            catch (Exception ex)
                            {
                                Log($"Couldn't create folder {path}", ex);
                            }
                        }
                    }
                }
                //create default folder
                if((folders == null || folders.Count == 0) && (files != null && files.Count != 0))
                {
                    var parentId = Global.FolderMy;
                    var createdFolder = Global.FileStorageService.CreateNewFolder(parentId.ToString(), newParentFolder);
                    foldersDict.Add(newParentFolder, createdFolder);
                }

                // Copy all files
                var filesDict = new Dictionary<string, ASCFile>();
                if (files != null && files.Count != 0)
                {
                    foreach (var file in files)
                    {
                        var maskFile = file.Replace(ModuleName + " " + folderCreation + Path.DirectorySeparatorChar.ToString(), "");
                        var maskParentPath = Path.GetDirectoryName(maskFile);

                        // ToDo: maybe we should upload to root, if required folder wasn't created

                        try
                        {
                            var realPath = Path.Combine(drivePath, maskFile);
                            using (var fs = new FileStream(realPath, FileMode.Open))
                            using (var fileDao = Global.DaoFactory.GetFileDao())
                            using (var folderDao = Global.DaoFactory.GetFolderDao())
                            {
                                var parentFolder = string.IsNullOrWhiteSpace(maskParentPath) ? foldersDict[newParentFolder] : foldersDict[maskParentPath];

                                var newFile = new ASCFile
                                {
                                    FolderID = parentFolder.ID,
                                    Comment = FilesCommonResource.CommentCreate,
                                    Title = Path.GetFileName(file),
                                    ContentLength = fs.Length
                                };
                                newFile = fileDao.SaveFile(newFile, fs);
                                realPath = realPath.Contains(Path.DirectorySeparatorChar.ToString() + newParentFolder) ? realPath.Replace(Path.DirectorySeparatorChar.ToString() + newParentFolder, "") : realPath;
                                filesDict.Add(realPath, newFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"Couldn't create file {maskParentPath}/{Path.GetFileName(file)}", ex);
                        }
                    }
                }

                var entries = filesDict
                    .ToDictionary(kv => kv.Key, kv => (FileEntry)kv.Value)
                    .Concat(foldersDict
                        .ToDictionary(kv => Path.Combine(drivePath, kv.Key), kv => (FileEntry)kv.Value))
                    .OrderBy(kv => kv.Value is ASCFile)
                    .ThenBy(kv => kv.Key.Count(c => Path.DirectorySeparatorChar.Equals(c)));

                var favFolders = new ItemList<object>();
                var favFiles = new ItemList<object>();

                var fileSec = new FileSecurity(Global.DaoFactory);

                foreach (var kv in entries)
                {
                    if (TryReadInfoFile(kv.Key, out var info))
                    {
                        if (info.Starred)
                        {
                            if (kv.Value is ASCFile)
                            {
                                favFiles.Add(kv.Value.ID);
                            }
                            else
                            {
                                favFolders.Add(kv.Value.ID);
                            }
                        }

                        var list = new ItemList<AceWrapper>();
                        foreach (var shareInfo in info.Permissions)
                        {
                            if (shareInfo.Type == "user" || shareInfo.Type == "group")
                            {
                                var shareType = GetPortalShare(shareInfo);
                                users.TryGetValue(shareInfo.EmailAddress, out var userToShare);
                                groups.TryGetValue(shareInfo.Name, out var groupToShare);
                                if (shareType == null || (userToShare == null && groupToShare == null)) continue;

                                Func<FileEntry, Guid, bool> checkRights = null;
                                switch (shareType)
                                {
                                    case ASCShare.ReadWrite:
                                        checkRights = fileSec.CanEdit;
                                        break;
                                    case ASCShare.Comment:
                                        checkRights = fileSec.CanComment;
                                        break;
                                    case ASCShare.Read:
                                        checkRights = fileSec.CanRead;
                                        break;
                                    default: // unused
                                        break;
                                }
                                var entryGuid = userToShare == null ? groupToShare.Guid : userToShare.Guid;

                                if (checkRights != null && checkRights(kv.Value, entryGuid)) continue; // already have rights, skip

                                list.Add(new AceWrapper
                                {
                                    Share = shareType.Value,
                                    SubjectId = entryGuid,
                                    SubjectGroup = false
                                });
                            }
                        }

                        if (!list.Any()) continue;

                        var aceCollection = new AceCollection
                        {
                            Entries = new ItemList<string> { (kv.Value is ASCFile ? "file_" : "folder_") + kv.Value.ID },
                            Aces = list,
                            Message = null
                        };
                        try
                        {
                            Global.FileStorageService.SetAceObject(aceCollection, false);
                        }
                        catch (Exception ex)
                        {
                            Log($"Couldn't change file permissions for {aceCollection.Entries.First()}", ex);
                        }
                    }
                }

                if (favFolders.Any() || favFiles.Any())
                {
                    Global.FileStorageService.AddToFavorites(favFolders, favFiles);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (Directory.Exists(tmpFolder))
                {
                    Directory.Delete(tmpFolder, true);
                }
            }
        }

        public GwsMigratingFiles(string rootFolder, GwsMigratingUser user, Action<string, Exception> log) : base(log)
        {
            this.rootFolder = rootFolder;
            this.user = user;
        }

        private static readonly Regex versionRegex = new Regex(@"(\([\d]+\))");
        private string FindInfoFile(string entry)
        {
            var infoFilePath = entry + infoFile;
            if (File.Exists(infoFilePath)) return infoFilePath; // file.docx-info.json

            var ext = Path.GetExtension(entry);
            infoFilePath = entry.Substring(0, entry.Length - ext.Length) + infoFile;
            if (File.Exists(infoFilePath)) return infoFilePath; // file-info.json

            var versionMatch = versionRegex.Match(entry);
            if (!versionMatch.Success) return null;

            var version = versionMatch.Groups[1].Value;
            infoFilePath = entry.Replace(version, "") + infoFile.Replace(".", version + ".");
            if (File.Exists(infoFilePath)) return infoFilePath; // file.docx-info(1).json

            infoFilePath = entry.Substring(0, entry.Length - ext.Length).Replace(version, "") + infoFile.Replace(".", version + ".");
            if (File.Exists(infoFilePath)) return infoFilePath; // file-info(1).json

            return null;
        }

        private bool TryReadInfoFile(string entry, out GwsDriveFileInfo info)
        {
            info = null;
            var infoFilePath = FindInfoFile(entry);

            if (infoFilePath == null) return false;

            try
            {
                info = JsonConvert.DeserializeObject<GwsDriveFileInfo>(File.ReadAllText(infoFilePath));
                return true;
            }
            catch (Exception ex)
            {
                Log($"Couldn't read info file for {entry}", ex);
            }

            return false;
        }

        private static readonly Regex workspacesRegex = new Regex(@"Workspaces(\(\d+\))?.json");
        private static readonly Regex pinnedRegex = new Regex(@".*-at-.*-pinned\..*");
        private const string commentsFile = "-comments.html";
        private const string infoFile = "-info.json";
        private static readonly Regex commentsVersionFile = new Regex(@"-comments(\([\d]+\))\.html");
        private static readonly Regex infoVersionFile = new Regex(@"-info(\([\d]+\))\.json");
        private bool ShouldIgnoreFile(string entry, string[] entries)
        {
            if (workspacesRegex.IsMatch(Path.GetFileName(entry))) return true; // ignore workspaces.json
            if (pinnedRegex.IsMatch(Path.GetFileName(entry))) return true; // ignore pinned files

            if (entry.EndsWith(commentsFile) || entry.EndsWith(infoFile)) // check if this really a meta for existing file
            {
                // folder - folder
                // folder-info.json - valid meta

                // file.docx - file
                // file.docx-info.json - valid meta
                // file-info.json - valid meta

                var baseName = entry.Substring(0, entry.Length - (entry.EndsWith(commentsFile) ? commentsFile.Length : infoFile.Length));
                if (entries.Contains(baseName)) return true;
                if (entries
                    .Where(e => e.StartsWith(baseName + "."))
                    .Select(e => e.Substring(0, e.Length - Path.GetExtension(e).Length))
                    .Contains(baseName)) return true;
            }

            // file(1).docx - file
            // file.docx-info(1).json - valid meta
            // file-info(1).json - valid meta
            var commentsVersionMatch = commentsVersionFile.Match(entry);
            if (commentsVersionMatch.Success)
            {
                var baseName = entry.Substring(0, entry.Length - commentsVersionMatch.Groups[0].Value.Length);
                baseName = baseName.Insert(baseName.LastIndexOf("."), commentsVersionMatch.Groups[1].Value);

                if (entries.Contains(baseName)) return true;
                if (entries
                    .Where(e => e.StartsWith(baseName + "."))
                    .Select(e => e.Substring(0, e.Length - Path.GetExtension(e).Length))
                    .Contains(baseName)) return true;
            }

            var infoVersionMatch = infoVersionFile.Match(entry);
            if (infoVersionMatch.Success)
            {
                var baseName = entry.Substring(0, entry.Length - infoVersionMatch.Groups[0].Length);
                baseName = baseName.Insert(baseName.LastIndexOf("."), infoVersionMatch.Groups[1].Value);

                if (entries.Contains(baseName)) return true;
                if (entries
                    .Where(e => e.StartsWith(baseName + "."))
                    .Select(e => e.Substring(0, e.Length - Path.GetExtension(e).Length))
                    .Contains(baseName)) return true;
            }

            return false;
        }

        private ASCShare? GetPortalShare(GwsDriveFilePermission fileInfo)
        {
            switch (fileInfo.Role)
            {
                case "writer":
                    return ASCShare.ReadWrite;
                case "reader":
                    if (fileInfo.AdditionalRoles == null) return ASCShare.Read;
                    if (fileInfo.AdditionalRoles.Contains("commenter"))
                    {
                        return ASCShare.Comment;
                    }
                    else
                    {
                        return ASCShare.Read;
                    }

                default:
                    return null;
            };
        }

        private List<string> files;
        private List<string> folders;
        private string rootFolder;
        private int foldersCount;
        private int filesCount;
        private long bytesTotal;
        private GwsMigratingUser user;
        private Dictionary<string, GwsMigratingUser> users;
        private Dictionary<string, GWSMigratingGroups> groups;
        private string folderCreation;
    }
}
