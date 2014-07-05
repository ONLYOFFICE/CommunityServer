/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using ASC.Common.Threading.Workers;
using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Engine;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using BasecampRestAPI;
using log4net;
using Comment = ASC.Projects.Core.Domain.Comment;
using Project = ASC.Projects.Core.Domain.Project;
using System.Security.Principal;
using ASC.Web.Studio.Core;
using ASC.Common.Web;
using ASC.Web.Core.Users;

namespace ASC.Web.Projects.Configuration
{
    public class ImportQueue
    {
        private static readonly WorkerQueue<ImportFromBasecamp> Imports = new WorkerQueue<ImportFromBasecamp>(4, TimeSpan.FromMinutes(30), 1, true);

        private static readonly List<ImportFromBasecamp> Completed = new List<ImportFromBasecamp>();

        static ImportQueue()
        {
            Imports.Start(DoImport);
        }

        public static int Add(string url, string userName, string password, bool processClosed, bool disableNotifications, bool importUsersAsCollaborators, IEnumerable<int> projects)
        {
            if (Imports.GetItems().Count(x => x.Id == TenantProvider.CurrentTenantID) > 0)
            {
                throw new DuplicateNameException("Import already running");
            }

            lock (Completed)
            {
                Completed.RemoveAll(x => x.Id == TenantProvider.CurrentTenantID);
            }

            SecurityContext.DemandPermissions(Constants.Action_AddRemoveUser);

            var importTask = new ImportFromBasecamp(url, userName, password, SecurityContext.CurrentAccount.ID, processClosed, disableNotifications, importUsersAsCollaborators, Global.EngineFactory, projects);

            Imports.Add(importTask);

            return importTask.Id;
        }

        public static IEnumerable<Project> GetProjects(string url, string userName, string password)
        {
            var basecampManager = BaseCamp.GetInstance(ImportFromBasecamp.PrepUrl(url).ToString().TrimEnd('/') + "/api/v1", userName, password);

            return basecampManager.Projects.Select(r => new Project {ID = r.ID, Title = r.Name, Status = r.IsClosed ? ProjectStatus.Closed : ProjectStatus.Open});
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
            var importd = Imports.GetItems().SingleOrDefault(x => x.Id == TenantProvider.CurrentTenantID);
            if (importd == null)
            {
                lock (Completed)
                {
                    //Maybe it's completed already
                    importd = Completed.FirstOrDefault(x => x.Id == TenantProvider.CurrentTenantID);
                    Completed.RemoveAll(x => x.Id == TenantProvider.CurrentTenantID);
                }
            }
            if (importd != null)
            {
                return importd.Status;
            }
            throw new KeyNotFoundException("Import not found"); //todo: return ImportStatus
        }

        private static void DoImport(ImportFromBasecamp obj)
        {
            try
            {
                obj.StartImport();
                obj.Status.Completed = true;
                NotifyClient.Instance.SendAboutImportComplite(obj.InitiatorId);
            }
            catch(Exception e)
            {
                obj.Status.LogError(ImportResource.ImportFailed, e);
                obj.Status.Error = e;
                obj.LogError("generic error", e);
            }
            finally
            {
                obj.Status.CompletedAt = DateTime.Now;
                lock (Completed)
                {
                    Completed.Add(obj);
                }
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
        public Exception Ex { get; set; }

        public ImportStatusLogEntry(Exception e)
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
                FileProgress = 100;
                UserProgress = 100;
                ProjectProgress = 100;
                Error = null;
            }
        }

        public DateTime CompletedAt { get; set; }

        [DataMember]
        public List<ImportStatusLogEntry> Log { get; set; }

        public ImportStatus(string url)
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
            Log.Add(new ImportStatusLogEntry(e) {Message = HttpUtility.HtmlEncode(message), Type = "error"});
        }

        public void LogWarn(string message)
        {
            Log.Add(new ImportStatusLogEntry(null) {Message = HttpUtility.HtmlEncode(message), Type = "warn"});
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
        public ImportStatus Status { get; set; }
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
            Status = new ImportStatus(_url);
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

                Status.LogInfo(ImportResource.ImportStarted);
                var basecampManager = BaseCamp.GetInstance(_url, _userName, _password);
                LogStatus("import users");
                SaveUsers(basecampManager);
                LogStatus("import projects");
                SaveProjects(basecampManager);
                Status.LogInfo(ImportResource.ImportCompleted);
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

                    Status.UserProgress += step;
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
                    Status.LogError(string.Format(ImportResource.FailedToSaveUser, person.EmailAddress), e);
                    LogError(string.Format("user '{0}' failed", person.EmailAddress), e);
                    _newUsersID.RemoveAll(x => x.InBasecamp == person.ID);
                }
            }
        }

        private void SaveProjects(IBaseCamp basecampManager)
        {
            var projects = basecampManager.Projects;
            var step = 50.0 / projects.Count();
            var projectEngine = _engineFactory.GetProjectEngine();
            var participantEngine = _engineFactory.GetParticipantEngine();

            if (_projects.Any())
            {
                projects = projects.Where(r => _projects.Any(pr => pr == r.ID)).ToArray();
            }

            foreach (var project in projects)
            {
                try
                {
                    Status.LogInfo(string.Format(ImportResource.ImportProjectStarted, project.Name));
                    Status.ProjectProgress += step;
                    var newProject = new Project
                        {
                            Status = !project.IsClosed ? ProjectStatus.Open : ProjectStatus.Closed,
                            Title = ReplaceLineSeparator(project.Name),
                            Description = project.Description,
                            Responsible = _initiatorId,
                            Private = true
                        };

                    projectEngine.SaveOrUpdate(newProject, true);
                    Participant prt = participantEngine.GetByID(newProject.Responsible);
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
                    Status.LogError(string.Format(ImportResource.FailedToSaveProject, project.Name), e);
                    LogError(string.Format("project '{0}' failed", project.Name), e);
                    _newProjectsID.RemoveAll(x => x.InBasecamp == project.ID);
                }
            }

            //Select only suceeded projects
            var projectsToProcess = projects.Where(x => _newProjectsID.Count(y => y.InBasecamp == x.ID) > 0).ToList();
            step = 50.0 / projectsToProcess.Count;
            foreach (var project in projectsToProcess)
            {
                Status.LogInfo(string.Format(ImportResource.ImportProjectDataStarted, project.Name));

                Status.ProjectProgress += step;

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
            var projectEngine = _engineFactory.GetProjectEngine();
            var messageEngine = _engineFactory.GetMessageEngine();
            try
            {
                var newMessage = new Message
                    {
                        Title = ReplaceLineSeparator(message.Title),
                        Content = message.Body,
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
                Status.LogError(string.Format(ImportResource.FailedToSaveMessage, message.Title), e);
                LogError(string.Format("message '{0}' failed", message.Title), e);
                _newMessagesID.RemoveAll(x => x.InBasecamp == message.ID);
            }
        }

        private void SaveTasks(IToDoList todoList, int projectID)
        {
            var projectEngine = _engineFactory.GetProjectEngine();
            var taskEngine = _engineFactory.GetTaskEngine();
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
                    Status.LogError(string.Format(ImportResource.FailedToSaveTask, task.Content), e);
                    LogError(string.Format("task '{0}' failed", task.Content), e);
                    _newTasksID.RemoveAll(x => x.InBasecamp == task.ID);
                }
            }
        }

        private void SaveMessageComments(IEnumerable<IComment> comments, int messageid)
        {
            var commentEngine = _engineFactory.GetCommentEngine();
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
                    Status.LogError(string.Format(ImportResource.FailedToSaveComment, comment.ID), e);
                    LogError(string.Format("comment '{0}' failed", comment.ID), e);
                }
            }
        }

        private void SaveTaskComments(IEnumerable<IComment> comments, int taskid)
        {
            var commentEngine = _engineFactory.GetCommentEngine();
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
                    Status.LogError(string.Format(ImportResource.FailedToSaveComment, comment.ID), e);
                    LogError(string.Format("comment '{0}' failed", comment.ID), e);
                }
            }
        }

        private void SaveFiles(IBaseCamp basecampManeger, IEnumerable<IAttachment> attachments, int projectID)
        {
            var step = 100.0 / attachments.Count();

            Status.LogInfo(string.Format(ImportResource.ImportFileStarted, attachments.Count()));

            //select last version
            foreach (var attachment in attachments)
            {
                Status.FileProgress += step;
                try
                {
                    var httpWReq = basecampManeger.Service.GetRequest(attachment.DownloadUrl);
                    using (var httpWResp = (HttpWebResponse)httpWReq.GetResponse())
                    {
                        if (attachment.ByteSize > SetupInfo.MaxUploadSize)
                        {
                            Status.LogError(string.Format(ImportResource.FailedSaveFileMaxSizeExided, attachment.Name), new Exception());
                            continue;
                        }

                        var file = new ASC.Files.Core.File
                            {
                                FolderID = FileEngine2.GetRoot(FindProject(projectID)),
                                Title = attachment.Name,
                                ContentLength = attachment.ByteSize,
                                CreateBy = FindUser(attachment.AuthorID),
                                CreateOn = attachment.CreatedOn.ToUniversalTime()
                            };
                        if (file.Title.LastIndexOf('\\') != -1) file.Title = file.Title.Substring(file.Title.LastIndexOf('\\') + 1);

                        file = FileEngine2.SaveFile(file, httpWResp.GetResponseStream());

                        if ("Message".Equals(attachment.OwnerType, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var messageId = FindMessage(attachment.OwnerID);
                                FileEngine2.AttachFileToMessage(new Message {ID = messageId}, file.ID); //It's not critical 
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
                                FileEngine2.AttachFileToTask(new Task {ID = taskId}, file.ID); //It's not critical 
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
                        Status.LogError(string.Format(ImportResource.FailedToSaveFile, attachment.Name), e);
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
}