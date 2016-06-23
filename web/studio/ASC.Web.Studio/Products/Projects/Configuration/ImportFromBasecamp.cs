/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Common.Caching;
using ASC.Common.Threading.Workers;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Engine;
using ASC.Web.Core.Users;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using BasecampRestAPI;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Comment = ASC.Projects.Core.Domain.Comment;
using Project = ASC.Projects.Core.Domain.Project;


namespace ASC.Web.Projects.Configuration
{
    public class ImportQueue
    {
        private static readonly WorkerQueue<ImportFromBasecamp> imports = new WorkerQueue<ImportFromBasecamp>(4, TimeSpan.FromMinutes(30), 1, true);

        static ImportQueue()
        {
            imports.Start(DoImport);
        }

        public static int Add(string url, string userName, string password, bool processClosed, bool disableNotifications, bool importUsersAsCollaborators, IEnumerable<int> projects)
        {
            var status = GetStatus();
            if (imports.GetItems().Count(x => x.Id == TenantProvider.CurrentTenantID) > 0 || (status.Started && !status.Completed))
            {
                throw new DuplicateNameException("Import already running");
            }

            SecurityContext.DemandPermissions(Constants.Action_AddRemoveUser);

            var importTask = new ImportFromBasecamp(url, userName, password, SecurityContext.CurrentAccount.ID, processClosed, disableNotifications, importUsersAsCollaborators, Global.EngineFactory, projects);

            imports.Add(importTask);

            return importTask.Id;
        }

        public static IEnumerable<Project> GetProjects(string url, string userName, string password)
        {
            var basecampManager = BaseCamp.GetInstance(ImportFromBasecamp.PrepUrl(url).ToString().TrimEnd('/') + "/api/v1", userName, password);
            return basecampManager.Projects.Select(r => new Project {ID = r.ID, Title = r.Name, Status = r.IsClosed ? ProjectStatus.Closed : ProjectStatus.Open}).ToList();
        }

        public static int CheckUsersQuota(string url, string userName, string password)
        {
            var basecampManager = BaseCamp.GetInstance(ImportFromBasecamp.PrepUrl(url).ToString().TrimEnd('/') + "/api/v1", userName, password);
            var countImportedUsers = basecampManager.People.Count();
            var remainingAmount = TenantExtra.GetRemainingCountUsers();

            if (remainingAmount == 0) return 0;

            var difference = remainingAmount - countImportedUsers;
            return difference >= 0 ? remainingAmount : -remainingAmount;
        }

        public static ImportStatus GetStatus()
        {
            return StatusState.GetStatus();
        }

        private static void DoImport(ImportFromBasecamp obj)
        {
            try
            {
                obj.StartImport();

                NotifyClient.Instance.SendAboutImportComplite(obj.InitiatorId);
            }
            catch(Exception e)
            {
                obj.ImportError(e);
            }
            finally
            {
                obj.ImportComplete();
            }
        }
    }

    [DataContract(Name = "import_status_log_entry", Namespace = "")]
    public class ImportStatusLogEntry
    {
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime Time { get; set; }

        [DataMember]
        public string Ex { get; set; }

        public ImportStatusLogEntry(string e)
        {
            Time = DateTime.UtcNow;
            Ex = e;
        }
    }

    [DataContract(Name = "import_status", Namespace = "")]
    public class ImportStatus
    {
        private double _projectProgress;
        private double _userProgress;
        private double _fileProgress;

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public double UserProgress
        {
            get { return Math.Min(Math.Max(_userProgress, 0), 100); }
            set { _userProgress = value; }
        }

        [DataMember]
        public double ProjectProgress
        {
            get { return Math.Min(Math.Max(_projectProgress, 0), 100); }
            set { _projectProgress = value; }
        }

        [DataMember]
        public double FileProgress
        {
            get { return Math.Min(Math.Max(_fileProgress, 0), 100); }
            set { _fileProgress = value; }
        }

        [DataMember]
        public Exception Error { get; set; }

        [DataMember]
        public bool Completed
        {
            get { return FileProgress == 100 && ProjectProgress == 100 && UserProgress == 100 && Error == null; }
            set
            {
                if (!value) return;
                FileProgress = 100;
                UserProgress = 100;
                ProjectProgress = 100;
                Error = null;
            }
        }

        public DateTime CompletedAt { get; set; }

        [DataMember]
        public List<ImportStatusLogEntry> Log { get; set; }

        [DataMember]
        public bool Started { get; set; }

        public ImportStatus(string url = "")
        {
            Url = url;
            Log = new List<ImportStatusLogEntry>();
        }

        public void LogInfo(string message)
        {
            Log.Add(new ImportStatusLogEntry(null) {Message = HttpUtility.HtmlEncode(message), Type = "info"});
        }

        public void LogError(string message, Exception e)
        {
            Log.Add(new ImportStatusLogEntry(e.ToString()) {Message = HttpUtility.HtmlEncode(message), Type = "error"});
        }
    }

    public class ImportFromBasecamp
    {
        #region Members

        private readonly List<UserIDWrapper> _newUsersID;
        private readonly List<ProjectIDWrapper> _newProjectsID;
        private readonly List<MilestoneIDWrapper> _newMilestonesID;
        private readonly List<MessageIDWrapper> _newMessagesID;
        private readonly List<TaskIDWrapper> _newTasksID;
        private readonly List<FileIDWrapper> _newFilesID;
        private readonly string _url;
        private readonly string _userName;
        private readonly string _password;
        private readonly Guid _initiatorId;
        private readonly bool _withClosed;
        private readonly bool _disableNotifications;
        private readonly bool _importUsersAsCollaborators;
        private bool _importUsersOverLimitAsCollaborators;
        private readonly EngineFactory _engineFactory;
        public readonly int Id;

        private readonly ILog _log;
        private readonly IPrincipal _principal;
        private readonly IEnumerable<int> _projects;

        public Guid InitiatorId
        {
            get { return _initiatorId; }
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ImportFromBasecamp)) return false;
            return Equals((ImportFromBasecamp)obj);
        }

        public ImportFromBasecamp(string url, string userName, string password, Guid initiatorId, bool withClosed, bool disableNotifications, bool importUsersAsCollaborators, EngineFactory engineFactory, IEnumerable<int> projects)
        {
            _newUsersID = new List<UserIDWrapper>();
            _newProjectsID = new List<ProjectIDWrapper>();
            _newMilestonesID = new List<MilestoneIDWrapper>();
            _newMessagesID = new List<MessageIDWrapper>();
            _newTasksID = new List<TaskIDWrapper>();
            _newFilesID = new List<FileIDWrapper>();

            _url = (PrepUrl(url).ToString().TrimEnd('/') + "/api/v1");
            _userName = userName;
            _password = password;
            _initiatorId = initiatorId;
            _withClosed = withClosed;
            _disableNotifications = disableNotifications;
            _importUsersAsCollaborators = importUsersAsCollaborators;
            _engineFactory = engineFactory;
            _engineFactory.DisableNotifications = disableNotifications;
            StatusState.SetStatus(new ImportStatus(_url));
            Id = TenantProvider.CurrentTenantID;
            _log = LogManager.GetLogger("ASC.Project.BasecampImport");
            _principal = Thread.CurrentPrincipal;
            _projects = projects;
        }

        public static Uri PrepUrl(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                if (!(url.StartsWith(Uri.UriSchemeHttp) || url.StartsWith(Uri.UriSchemeHttps)))
                {
                    //Add and try again
                    url = Uri.UriSchemeHttps + "://" + url;
                    return PrepUrl(url);
                }
                return new Uri(url, UriKind.Absolute);
            }
            return uri;
        }

        private void LogStatus(string message)
        {
            _log.DebugFormat("in {0}; {1} {2}", _initiatorId, _url, message);
        }

        internal void LogError(string message, Exception e)
        {
            _log.Error(string.Format("in {0}; {1} {2}", _initiatorId, _url, message), e);
        }

        public void StartImport()
        {
            HttpContext.Current = null;
            try
            {
                LogStatus("started");

                CoreContext.TenantManager.SetCurrentTenant(Id);
                Thread.CurrentPrincipal = _principal;

                HttpContext.Current = new HttpContext(
                    new HttpRequest("fake", CommonLinkUtility.GetFullAbsolutePath("/"), string.Empty),
                    new HttpResponse(new StringWriter()));

                StatusState.SetStatusStarted();
                StatusState.StatusLogInfo(ImportResource.ImportStarted);
                var basecampManager = BaseCamp.GetInstance(_url, _userName, _password);

                LogStatus("import users");
                SaveUsers(basecampManager);

                LogStatus("import projects");

                SaveProjects(basecampManager);

                StatusState.SetStatusCompleted();

                StatusState.StatusLogInfo(ImportResource.ImportCompleted);
            }
            finally
            {
                if (HttpContext.Current != null)
                {
                    new DisposableHttpContext(HttpContext.Current).Dispose();
                    HttpContext.Current = null;
                }
            }
        }

        public void ImportError(Exception e)
        {
            StatusState.StatusLogError(ImportResource.ImportFailed, e);
            StatusState.StatusError(e);
            LogError("generic error", e);
        }

        public void ImportComplete()
        {
            StatusState.StatusCompletedAt(DateTime.Now);
        }

        #region Save Functions

        private void SaveUsers(IBaseCamp basecampManager)
        {
            var employees = basecampManager.People;
            var step = 100.0 / employees.Count();
            foreach (var person in employees)
            {
                try
                {
                    if (TenantExtra.GetRemainingCountUsers() <= 0)
                    {
                        _importUsersOverLimitAsCollaborators = true;
                    }

                    StatusState.StatusUserProgress(step);
                    var userID = FindUserByEmail(person.EmailAddress);

                    if (userID.Equals(Guid.Empty))
                    {
                        var userName = Regex.Replace(person.UserName, @"[!|@|#|$|%|'|+]", "");
                        var name = userName.Split(' ');
                        var userInfo = new UserInfo
                            {
                                Email = person.EmailAddress,
                                FirstName = name.First(),
                                LastName = name.Count() > 1 ? name.Last() : "",
                                UserName = userName,
                                Status = EmployeeStatus.Active,
                            };
                        var collaboratorFlag = _importUsersOverLimitAsCollaborators || _importUsersAsCollaborators;

                        if (!UserManagerWrapper.ValidateEmail(userInfo.Email)) throw new Exception("Invalid email");

                        var newUserInfo = UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword(), false, !_disableNotifications, collaboratorFlag);
                        _newUsersID.Add(new UserIDWrapper {InBasecamp = person.ID, InProjects = newUserInfo.ID});

                        //save user avatar
                        const string emptyAvatar = "http://asset0.37img.com/global/default_avatar_v1_4/avatar.gif?r=3";
                        if (person.AvatarUrl != emptyAvatar)
                            UserPhotoManager.SaveOrUpdatePhoto(newUserInfo.ID, StreamFile(person.AvatarUrl));
                    }
                    else
                    {
                        _newUsersID.Add(new UserIDWrapper {InBasecamp = person.ID, InProjects = userID});
                    }
                }
                catch(Exception e)
                {
                    StatusState.StatusLogError(string.Format(ImportResource.FailedToSaveUser, person.EmailAddress), e);
                    LogError(string.Format("user '{0}' failed", person.EmailAddress), e);
                    _newUsersID.RemoveAll(x => x.InBasecamp == person.ID);
                }
            }
        }

        private void SaveProjects(IBaseCamp basecampManager)
        {
            var projects = basecampManager.Projects;
            var step = 50.0 / projects.Count();
            var projectEngine = _engineFactory.ProjectEngine;
            var participantEngine = _engineFactory.ParticipantEngine;

            if (_projects.Any())
            {
                projects = projects.Where(r => _projects.Any(pr => pr == r.ID)).ToArray();
            }

            foreach (var project in projects)
            {
                try
                {
                    StatusState.StatusLogInfo(string.Format(ImportResource.ImportProjectStarted, project.Name));
                    StatusState.StatusProjectProgress(step);
                    var newProject = new Project
                        {
                            Status = !project.IsClosed ? ProjectStatus.Open : ProjectStatus.Closed,
                            Title = ReplaceLineSeparator(project.Name),
                            Description = project.Description,
                            Responsible = _initiatorId,
                            Private = true
                        };

                    projectEngine.SaveOrUpdate(newProject, true);
                    var prt = participantEngine.GetByID(newProject.Responsible);
                    projectEngine.AddToTeam(newProject, prt, true);

                    foreach (var wrapper in
                        project.People.SelectMany(user => _newUsersID.Where(wrapper => user.ID == wrapper.InBasecamp)))
                    {
                        prt = participantEngine.GetByID(wrapper.InProjects);
                        projectEngine.AddToTeam(newProject, prt, true);

                        //check permission
                        var user = project.People.ToList().Find(p => p.ID == wrapper.InBasecamp);
                        if (user != null)
                        {
                        }
                    }
                    _newProjectsID.Add(new ProjectIDWrapper {InBasecamp = project.ID, InProjects = newProject.ID});
                }
                catch(Exception e)
                {
                    StatusState.StatusLogError(string.Format(ImportResource.FailedToSaveProject, project.Name), e);
                    LogError(string.Format("project '{0}' failed", project.Name), e);
                    _newProjectsID.RemoveAll(x => x.InBasecamp == project.ID);
                }
            }

            //Select only suceeded projects
            var projectsToProcess = projects.Where(x => _newProjectsID.Count(y => y.InBasecamp == x.ID) > 0).ToList();
            step = 50.0 / projectsToProcess.Count;
            foreach (var project in projectsToProcess)
            {
                StatusState.StatusLogInfo(string.Format(ImportResource.ImportProjectDataStarted, project.Name));

                StatusState.StatusProjectProgress(step);

                var messages = project.RecentMessages;
                foreach (var message in messages)
                {
                    SaveMessages(message, project.ID);
                }

                var todoLists = project.ToDoLists;
                foreach (var todoList in todoLists)
                {
                    SaveTasks(todoList, project.ID);
                }
                LogStatus("import files");
                SaveFiles(basecampManager, project.Attachments, project.ID);
            }
        }

        private void SaveMessages(IPost message, int projectID)
        {
            var projectEngine = _engineFactory.ProjectEngine;
            var messageEngine = _engineFactory.MessageEngine;
            try
            {
                var newMessage = new Message
                    {
                        Title = ReplaceLineSeparator(message.Title),
                        Description = message.Body,
                        Project = projectEngine.GetByID(FindProject(projectID)),
                        CreateOn = message.PostedOn.ToUniversalTime(),
                        CreateBy = FindUser(message.AuthorID)
                    };

                newMessage = messageEngine.SaveOrUpdate(newMessage, true, new[] {newMessage.CreateBy}, null);
                _newMessagesID.Add(new MessageIDWrapper {InBasecamp = message.ID, InProjects = newMessage.ID});
                SaveMessageComments(message.RecentComments, message.ID);
            }
            catch(Exception e)
            {
                StatusState.StatusLogError(string.Format(ImportResource.FailedToSaveMessage, message.Title), e);
                LogError(string.Format("message '{0}' failed", message.Title), e);
                _newMessagesID.RemoveAll(x => x.InBasecamp == message.ID);
            }
        }

        private void SaveTasks(IToDoList todoList, int projectID)
        {
            var projectEngine = _engineFactory.ProjectEngine;
            var taskEngine = _engineFactory.TaskEngine;
            foreach (var task in todoList.Items.Where(x => _withClosed || !x.Completed))
            {
                try
                {
                    var newTask = new Task
                        {
                            Title = ReplaceLineSeparator(task.Content),
                            Status = task.Completed ? TaskStatus.Closed : TaskStatus.Open,
                            Project = projectEngine.GetByID(FindProject(projectID)),
                            CreateOn = task.CreatedOn.ToUniversalTime(),
                            CreateBy = FindUser(task.CreatorID),
                            Description = string.Empty,
                            Deadline = task.Deadline
                        };

                    newTask.Deadline = DateTime.SpecifyKind(newTask.Deadline, DateTimeKind.Local);

                    if (task.ResponsibleID != -1)
                    {
                        var user = FindUser(task.ResponsibleID);
                        newTask.Responsibles.Add(user);
                    }

                    if (todoList.MilestoneID != -1)
                    {
                        var foundMilestone = FindMilestone(todoList.MilestoneID);
                        if (foundMilestone != -1)
                        {
                            newTask.Milestone = foundMilestone;
                        }
                    }

                    newTask = taskEngine.SaveOrUpdate(newTask, null, true, true);
                    _newTasksID.Add(new TaskIDWrapper {InBasecamp = task.ID, InProjects = newTask.ID});
                    SaveTaskComments(task.RecentComments, task.ID);
                }
                catch(Exception e)
                {
                    StatusState.StatusLogError(string.Format(ImportResource.FailedToSaveTask, task.Content), e);
                    LogError(string.Format("task '{0}' failed", task.Content), e);
                    _newTasksID.RemoveAll(x => x.InBasecamp == task.ID);
                }
            }
        }

        private void SaveMessageComments(IEnumerable<IComment> comments, int messageid)
        {
            var commentEngine = _engineFactory.CommentEngine;
            foreach (var comment in comments)
            {
                try
                {
                    var newComment = new Comment
                        {
                            CreateBy = FindUser(comment.AuthorID),
                            Content = comment.Body,
                            CreateOn = comment.CreatedAt.ToUniversalTime(),
                            TargetUniqID = ProjectEntity.BuildUniqId<Message>(FindMessage(messageid))
                        };
                    commentEngine.SaveOrUpdate(newComment);
                }
                catch(Exception e)
                {
                    StatusState.StatusLogError(string.Format(ImportResource.FailedToSaveComment, comment.ID), e);
                    LogError(string.Format("comment '{0}' failed", comment.ID), e);
                }
            }
        }

        private void SaveTaskComments(IEnumerable<IComment> comments, int taskid)
        {
            var commentEngine = _engineFactory.CommentEngine;
            foreach (var comment in comments)
            {
                try
                {
                    var newComment = new Comment
                        {
                            CreateBy = FindUser(comment.AuthorID),
                            Content = comment.Body,
                            CreateOn = comment.CreatedAt.ToUniversalTime(),
                            TargetUniqID = ProjectEntity.BuildUniqId<Task>(FindTask(taskid))
                        };
                    commentEngine.SaveOrUpdate(newComment);
                }
                catch(Exception e)
                {
                    StatusState.StatusLogError(string.Format(ImportResource.FailedToSaveComment, comment.ID), e);
                    LogError(string.Format("comment '{0}' failed", comment.ID), e);
                }
            }
        }

        private void SaveFiles(IBaseCamp basecampManeger, IEnumerable<IAttachment> attachments, int projectID)
        {
            var step = 100.0 / attachments.Count();

            StatusState.StatusLogInfo(string.Format(ImportResource.ImportFileStarted, attachments.Count()));

            //select last version
            foreach (var attachment in attachments)
            {
                StatusState.StatusFileProgress(step);

                try
                {
                    var httpWReq = basecampManeger.Service.GetRequest(attachment.DownloadUrl);
                    using (var httpWResp = (HttpWebResponse)httpWReq.GetResponse())
                    {
                        if (attachment.ByteSize > SetupInfo.MaxUploadSize)
                        {
                            StatusState.StatusLogError(string.Format(ImportResource.FailedSaveFileMaxSizeExided, attachment.Name), new Exception());
                            continue;
                        }

                        var file = new ASC.Files.Core.File
                            {
                                FolderID =_engineFactory.FileEngine.GetRoot(FindProject(projectID)),
                                Title = attachment.Name,
                                ContentLength = attachment.ByteSize,
                                CreateBy = FindUser(attachment.AuthorID),
                                CreateOn = attachment.CreatedOn.ToUniversalTime(),
                                Comment = ImportResource.CommentImport,
                            };
                        if (file.Title.LastIndexOf('\\') != -1) file.Title = file.Title.Substring(file.Title.LastIndexOf('\\') + 1);

                        file = _engineFactory.FileEngine.SaveFile(file, httpWResp.GetResponseStream());

                        if ("Message".Equals(attachment.OwnerType, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var messageId = FindMessage(attachment.OwnerID);
                                _engineFactory.MessageEngine.AttachFile(new Message {ID = messageId}, file.ID, false); //It's not critical 
                            }
                            catch(Exception e)
                            {
                                LogError(string.Format("not critical. attaching file '{0}' to message  failed", attachment.Name), e);
                            }
                        }

                        if ("Todo".Equals(attachment.OwnerType, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var taskId = FindTask(attachment.OwnerID);
                                _engineFactory.TaskEngine.AttachFile(new Task {ID = taskId}, file.ID, false); //It's not critical 
                            }
                            catch(Exception e)
                            {
                                LogError(string.Format("not critical. attaching file '{0}' to message  failed", attachment.Name), e);
                            }
                        }

                        _newFilesID.Add(new FileIDWrapper
                            {
                                InBasecamp = attachment.ID,
                                InProjects = file.ID
                            });
                    }
                }
                catch(Exception e)
                {
                    try
                    {
                        StatusState.StatusLogError(string.Format(ImportResource.FailedToSaveFile, attachment.Name), e);
                        LogError(string.Format("file '{0}' failed", attachment.Name), e);
                        _newFilesID.RemoveAll(x => x.InBasecamp == attachment.ID);
                    }
                    catch(Exception ex)
                    {
                        LogError(string.Format("file remove after error failed"), ex);
                    }
                }
            }
        }

        private static byte[] StreamFile(string filepath)
        {
            var urlGrabber = new WebClient();
            var data = urlGrabber.DownloadData(filepath);
            return data;
        }

        private static string ReplaceLineSeparator(string value)
        {
            var listChar = value.ToCharArray().ToList();
            listChar.RemoveAll(r => char.GetUnicodeCategory(r) == UnicodeCategory.LineSeparator);
            return new string(listChar.ToArray());
        }

        #endregion

        #region Search matches

        private static Guid FindUserByEmail(string email)
        {
            foreach (var user in CoreContext.UserManager.GetUsers(EmployeeStatus.All))
            {
                if (String.Equals(user.Email, email, StringComparison.InvariantCultureIgnoreCase)
                    && !String.IsNullOrEmpty(email))
                    return user.ID;
            }
            return Guid.Empty;
        }

        private Guid FindUser(int userID)
        {
            foreach (var record in _newUsersID.Where(record => record.InBasecamp == userID))
            {
                return record.InProjects;
            }
            return Guid.Empty;
        }

        private int FindProject(int projectID)
        {
            foreach (var record in _newProjectsID.Where(record => record.InBasecamp == projectID))
            {
                return record.InProjects;
            }
            throw new ArgumentException(string.Format("basecamp project not found {0}", projectID), "projectID");
        }

        private int FindMilestone(int milestoneID)
        {
            foreach (MilestoneIDWrapper record in _newMilestonesID)
            {
                if (record.InBasecamp == milestoneID)
                    return record.InProjects;
            }
            return -1;
        }

        private int FindMessage(int messageID)
        {
            foreach (MessageIDWrapper record in _newMessagesID)
            {
                if (record.InBasecamp == messageID)
                    return record.InProjects;
            }
            throw new ArgumentException(string.Format("basecamp message not found {0}", messageID), "messageID");
        }

        private int FindTask(int taskID)
        {
            foreach (TaskIDWrapper record in _newTasksID)
            {
                if (record.InBasecamp == taskID)
                    return record.InProjects;
            }
            throw new ArgumentException(string.Format("basecamp task not found {0}", taskID), "taskID");
        }

        #endregion

        #region Wrappers

        private class UserIDWrapper
        {
            public int InBasecamp;
            public Guid InProjects;
        }

        private class ProjectIDWrapper
        {
            public int InBasecamp;
            public int InProjects;
        }

        private class MilestoneIDWrapper
        {
            public int InBasecamp = 0;
            public int InProjects = 0;
        }

        private class MessageIDWrapper
        {
            public int InBasecamp;
            public int InProjects;
        }

        private class TaskIDWrapper
        {
            public int InBasecamp;
            public int InProjects;
        }

        private class FileIDWrapper
        {
            public string InBasecamp;
            public object InProjects;
        }

        #endregion

        public bool Equals(ImportFromBasecamp other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }

    static class StatusState
    {
        private static readonly ICache cache = AscCache.Default;

        internal static ImportStatus GetStatus()
        {
            return cache.Get<ImportStatus>(GetStateCacheKey()) ?? new ImportStatus();
        }

        internal static void SetStatus(ImportStatus status)
        {
            cache.Insert(GetStateCacheKey(), status, TimeSpan.FromMinutes(10));
        }

        private static string GetStateCacheKey()
        {
            return string.Format("{0}:project:queue:importbasecamp", TenantProvider.CurrentTenantID);
        }


        internal static void SetStatusStarted()
        {
            var status = GetStatus();
            status.Started = true;
            status.Completed = false;
            SetStatus(status);
        }

        internal static void SetStatusCompleted()
        {
            var status = GetStatus();
            status.Completed = true;
            SetStatus(status);
        }

        internal static void StatusLogInfo(string infoText)
        {
            var status = GetStatus();
            status.LogInfo(infoText);
            SetStatus(status);
        }

        internal static void StatusLogError(string errorText, Exception e)
        {
            var status = GetStatus();
            status.LogError(errorText, e);
            SetStatus(status);
        }

        internal static void StatusError(Exception e)
        {
            var status = GetStatus();
            status.Error = e;
            SetStatus(status);
        }

        internal static void StatusCompletedAt(DateTime dateTime)
        {
            var status = GetStatus();
            status.CompletedAt = dateTime;
            SetStatus(status);
        }

        internal static void StatusUserProgress(double step)
        {
            var status = GetStatus();
            status.UserProgress += step;
            SetStatus(status);
        }

        internal static void StatusProjectProgress(double step)
        {
            var status = GetStatus();
            status.ProjectProgress += step;
            SetStatus(status);
        }

        internal static void StatusFileProgress(double step)
        {
            var status = GetStatus();
            status.FileProgress += step;
            SetStatus(status);
        }
    }
}