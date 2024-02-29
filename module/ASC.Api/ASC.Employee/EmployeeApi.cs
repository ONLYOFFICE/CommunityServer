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
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Web;

using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Common.Threading;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Data.Reassigns;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Profile;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Specific;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.People;
using ASC.Web.People.Core.Import;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Quota;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;

using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Api.Employee
{
    ///<summary>
    ///Access to user profiles
    ///</summary>
    ///<name>people</name>
    public class EmployeeApi : Interfaces.IApiEntryPoint
    {
        private static readonly ProgressQueue progressQueue = new ProgressQueue(1, TimeSpan.FromMinutes(5), true);

        private ILog Log = LogManager.GetLogger("ASC.Api");

        public static readonly ICache Cache = AscCache.Default;

        private static Dictionary<string, string> GetHttpHeaders(HttpRequest httpRequest)
        {
            if (httpRequest == null) return null;

            var di = (from object k in httpRequest.Headers.Keys select k.ToString()).ToDictionary(key => key, key => httpRequest.Headers[key]);
            return di;
        }

        private readonly ApiContext _context;

        public string Name
        {
            get { return "people"; }
        }

        public EmployeeApi(ApiContext context)
        {
            _context = context;
        }

        private static HttpRequest Request
        {
            get { return HttpContext.Current.Request; }
        }

        /// <summary>
        /// Returns the detailed information about the current user profile.
        /// </summary>
        /// <short>
        /// Get my profile
        /// </short>
        /// <category>Profiles</category>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">Detailed information about my profile</returns>
        /// <path>api/2.0/people/@self</path>
        /// <httpMethod>GET</httpMethod>
        [Read("@self")]
        public EmployeeWraperFull GetMe()
        {
            return new EmployeeWraperFull(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID));
        }

        /// <summary>
        /// Returns a list of profiles for all the portal users.
        /// </summary>
        /// <short>
        /// Get profiles
        /// </short>
        /// <category>Profiles</category>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">List of profiles</returns>
        /// <path>api/2.0/people</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("")]
        public IEnumerable<EmployeeWraperFull> GetAll()
        {
            return GetByStatus(EmployeeStatus.Active);
        }

        /// <summary>
        /// Returns a list of profiles filtered by user status.
        /// </summary>
        /// <short>
        /// Get profiles by status
        /// </short>
        /// <param type="ASC.Core.Users.EmployeeStatus, ASC.Core.Users" name="status">User status ("Active", "Terminated", "LeaveOfAbsence", "All", or "Default")</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">List of profiles</returns>
        /// <category>User status</category>
        /// <path>api/2.0/people/status/{status}</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("status/{status}")]
        public IEnumerable<EmployeeWraperFull> GetByStatus(EmployeeStatus status)
        {
            if (CoreContext.Configuration.Personal) throw new MethodAccessException("Method not available");
            var query = CoreContext.UserManager.GetUsers(status).AsEnumerable();
            if ("group".Equals(_context.FilterBy, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_context.FilterValue))
            {
                var groupId = new Guid(_context.FilterValue);
                //Filter by group
                query = query.Where(x => CoreContext.UserManager.IsUserInGroup(x.ID, groupId));
                _context.SetDataFiltered();
            }
            return query.Select(x => new EmployeeWraperFull(x));
        }

        /// <summary>
        /// Returns the detailed information about a profile of the user with the name specified in the request.
        /// </summary>
        /// <short>
        /// Get a profile by user name
        /// </short>
        /// <category>Profiles</category>
        /// <param type="System.String, System" method="url" name="username">User name</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">User profile</returns>
        /// <path>api/2.0/people/{username}</path>
        /// <httpMethod>GET</httpMethod>
        [Read("{username}")]
        public EmployeeWraperFull GetById(string username)
        {
            if (CoreContext.Configuration.Personal) throw new MethodAccessException("Method not available");
            var user = CoreContext.UserManager.GetUserByUserName(username);
            if (user.ID == Core.Users.Constants.LostUser.ID)
            {
                Guid userId;
                if (Guid.TryParse(username, out userId))
                {
                    user = CoreContext.UserManager.GetUsers(userId);
                }
                else
                {
                    Log.Error(string.Format("Account {0} сould not get user by name {1}",
                                SecurityContext.CurrentAccount.ID, username));
                }
            }

            if (user.ID == Core.Users.Constants.LostUser.ID)
            {
                throw new ItemNotFoundException("User not found");
            }

            return new EmployeeWraperFull(user);
        }


        ///<summary>
        ///Returns the detailed information about a profile of the user with the email specified in the request.
        ///</summary>
        ///<short>
        ///Get a profile by user email
        ///</short>
        ///<category>Profiles</category>
        ///<param type="System.String, System" method="url" name="email">User email</param>
        ///<returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">User profile</returns>
        ///<path>api/2.0/people/email</path>
        ///<httpMethod>GET</httpMethod>
        [Read("email")]
        public EmployeeWraperFull GetByEmail(string email)
        {
            if (CoreContext.Configuration.Personal && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsOwner())
                throw new MethodAccessException("Method not available");
            var user = CoreContext.UserManager.GetUserByEmail(email);
            if (user.ID == Core.Users.Constants.LostUser.ID)
            {
                throw new ItemNotFoundException("User not found");
            }

            return new EmployeeWraperFull(user);
        }
        ///<summary>
        ///Returns a list of profiles for all the portal users matching the search query.
        ///</summary>
        ///<short>
        ///Search user profiles
        ///</short>
        ///<category>Search</category>
        ///<param type="System.String, System" method="url" name="query">Query</param>
        ///<returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">List of user profiles</returns>
        ///<path>api/2.0/people/@search/{query}</path>
        ///<httpMethod>GET</httpMethod>
        ///<collection>list</collection>
        [Read("@search/{query}")]
        public IEnumerable<EmployeeWraperFull> GetSearch(string query)
        {
            if (CoreContext.Configuration.Personal) throw new MethodAccessException("Method not available");
            try
            {
                var groupId = Guid.Empty;
                if ("group".Equals(_context.FilterBy, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_context.FilterValue))
                {
                    groupId = new Guid(_context.FilterValue);
                }

                return CoreContext.UserManager.Search(query, EmployeeStatus.Active, groupId)
                                  .Select(x => new EmployeeWraperFull(x));
            }
            catch (Exception error)
            {
                Log.Error(error);
            }
            return null;
        }

        ///<summary>
        ///Returns a list of users matching the search query.
        ///</summary>
        ///<short>
        ///Search users
        ///</short>
        ///<category>Search</category>
        ///<param type="System.String, System" method="url" name="query">Search text</param>
        ///<returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">List of users</returns>
        ///<path>api/2.0/people/search</path>
        ///<httpMethod>GET</httpMethod>
        ///<collection>list</collection>
        [Read("search")]
        public IEnumerable<EmployeeWraperFull> GetPeopleSearch(string query)
        {
            return GetSearch(query);
        }

        ///<summary>
        ///Returns a list of users matching the status filter and search query.
        ///</summary>
        ///<short>
        ///Search users by status filter
        ///</short>
        ///<category>Search</category>
        ///<param type="ASC.Core.Users.EmployeeStatus, ASC.Core.Users" method="url" name="status">User status ("Active", "Terminated", "LeaveOfAbsence", "All", or "Default")</param>
        ///<param type="System.String, System" method="url" name="query">Search query</param>
        ///<returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">List of users</returns>
        ///<path>api/2.0/people/status/{status}/search</path>
        ///<httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("status/{status}/search")]
        public IEnumerable<EmployeeWraperFull> GetAdvanced(EmployeeStatus status, string query)
        {
            if (CoreContext.Configuration.Personal) throw new MethodAccessException("Method not available");
            try
            {
                var list = CoreContext.UserManager.GetUsers(status).AsEnumerable();

                if ("group".Equals(_context.FilterBy, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_context.FilterValue))
                {
                    var groupId = new Guid(_context.FilterValue);
                    //Filter by group
                    list = list.Where(x => CoreContext.UserManager.IsUserInGroup(x.ID, groupId));
                    _context.SetDataFiltered();
                }

                list = list.Where(x => x.FirstName != null && x.FirstName.IndexOf(query, StringComparison.OrdinalIgnoreCase) > -1 || (x.LastName != null && x.LastName.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1) ||
                                       (x.UserName != null && x.UserName.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1) || (x.Email != null && x.Email.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1) || (x.Contacts != null && x.Contacts.Any(y => y.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1)));

                return list.Select(x => new EmployeeWraperFull(x));
            }
            catch (Exception error)
            {
                Log.Error(error);
            }
            return null;
        }

        /// <summary>
        /// Imports the new portal users with the first name, last name, and email address.
        /// </summary>
        /// <short>
        /// Import users
        /// </short>
        /// <category>Profiles</category>
        /// <param type="System.String, System" name="userList">List of users</param>
        /// <param type="System.Boolean, System" name="importUsersAsCollaborators" optional="true">Specifies whether to import users as guests (true) or not (false)</param>
        /// <returns>Newly added users</returns>
        /// <path>api/2.0/people/import/save</path>
        /// <httpMethod>POST</httpMethod>
        [Create("import/save")]
        public void SaveUsers(string userList, bool importUsersAsCollaborators)
        {
            lock (progressQueue.SynchRoot)
            {
                var task = progressQueue.GetItems().OfType<ImportUsersTask>().FirstOrDefault(t => (int)t.Id == TenantProvider.CurrentTenantID);
                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                Cache.Insert("REWRITE_URL" + tenant.TenantId, HttpContext.Current.Request.GetUrlRewriter().ToString(), TimeSpan.FromMinutes(5));
                if (task != null && task.IsCompleted)
                {
                    progressQueue.Remove(task);
                    task = null;
                }
                if (task == null)
                {
                    progressQueue.Add(new ImportUsersTask(userList, importUsersAsCollaborators, GetHttpHeaders(HttpContext.Current.Request))
                    {
                        Id = TenantProvider.CurrentTenantID,
                        UserId = SecurityContext.CurrentAccount.ID,
                        Percentage = 0
                    });
                }

            }
        }

        /// <summary>
        /// Returns a status of the current user.
        /// </summary>
        /// <short>
        /// Get a user status
        /// </short>
        /// <returns>Current user information</returns>
        /// <category>User status</category>
        /// <path>api/2.0/people/import/status</path>
        /// <httpMethod>GET</httpMethod>
        [Read("import/status")]
        public object GetStatus()
        {
            lock (progressQueue.SynchRoot)
            {
                var task = progressQueue.GetItems().OfType<ImportUsersTask>().FirstOrDefault(t => (int)t.Id == TenantProvider.CurrentTenantID);
                if (task == null) return null;

                return new
                {
                    Completed = task.IsCompleted,
                    Percents = (int)task.Percentage,
                    UserCounter = task.GetUserCounter,
                    Status = (int)task.Status,
                    Error = (string)task.Error,
                    task.Data
                };
            }
        }


        /// <summary>
        /// Returns a list of users with full information about them matching the parameters specified in the request.
        /// </summary>
        /// <short>
        /// Search users and their information by extended filter
        /// </short>
        /// <category>Search</category>
        /// <param type="System.Nullable{ASC.Core.Users.EmployeeStatus}, System" method="url" optional="true" name="employeeStatus">User status ("Active", "Terminated", "LeaveOfAbsence", "All", or "Default")</param>
        /// <param type="System.Nullable{System.Guid}, System" method="url" optional="true" name="groupId">Group ID</param>
        /// <param type="System.Nullable{ASC.Core.Users.EmployeeActivationStatus}, System" method="url" optional="true" name="activationStatus">Activation status ("NotActivated", "Activated", "Pending", or "AutoGenerated")</param>
        /// <param type="System.Nullable{ASC.Core.Users.EmployeeType}, System" method="url" optional="true" name="employeeType">User type ("All", "User", or "Visitor")</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="isAdministrator">Specifies if the user is an administrator or not</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee">
        ///  List of users with their information
        /// </returns>
        /// <path>api/2.0/people/filter</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("filter")]
        public IEnumerable<EmployeeWraperFull> GetFullByFilter(EmployeeStatus? employeeStatus, Guid? groupId, EmployeeActivationStatus? activationStatus, EmployeeType? employeeType, bool? isAdministrator)
        {
            var users = GetByFilter(employeeStatus, groupId, activationStatus, employeeType, isAdministrator);

            return users.Select(u => new EmployeeWraperFull(u, _context));
        }

        /// <summary>
        /// Returns a list of users matching the parameters specified in the request.
        /// </summary>
        /// <short>
        /// Search users by extended filter
        /// </short>
        /// <category>Search</category>
        /// <param type="System.Nullable{ASC.Core.Users.EmployeeStatus}, System" method="url" optional="true" name="employeeStatus">User status ("Active", "Terminated", "LeaveOfAbsence", "All", or "Default")</param>
        /// <param type="System.Nullable{System.Guid}, System" method="url" optional="true" name="groupId">Group ID</param>
        /// <param type="System.Nullable{ASC.Core.Users.EmployeeActivationStatus}, System" method="url" optional="true" name="activationStatus">Activation status ("NotActivated", "Activated", "Pending", or "AutoGenerated")</param>
        /// <param type="System.Nullable{ASC.Core.Users.EmployeeType}, System" method="url" optional="true" name="employeeType">User type ("All", "User", or "Visitor")</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="isAdministrator">Specifies if the user is an administrator or not</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee">
        ///  List of users
        /// </returns>
        /// <path>api/2.0/people/simple/filter</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("simple/filter")]
        public IEnumerable<EmployeeWraper> GetSimpleByFilter(EmployeeStatus? employeeStatus, Guid? groupId, EmployeeActivationStatus? activationStatus, EmployeeType? employeeType, bool? isAdministrator)
        {
            var users = GetByFilter(employeeStatus, groupId, activationStatus, employeeType, isAdministrator);

            return users.Select(u => new EmployeeWraper(u));
        }

        private IEnumerable<UserInfo> GetByFilter(EmployeeStatus? employeeStatus, Guid? groupId, EmployeeActivationStatus? activationStatus, EmployeeType? employeeType, bool? isAdministrator)
        {
            if (CoreContext.Configuration.Personal) throw new MethodAccessException("Method not available");
            var isAdmin = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin() ||
                          WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID);
            var status = isAdmin ? EmployeeStatus.All : EmployeeStatus.Default;

            if (employeeStatus != null)
            {
                switch (employeeStatus)
                {
                    case EmployeeStatus.Terminated:
                    case EmployeeStatus.All:
                        status = isAdmin ? (EmployeeStatus)employeeStatus : EmployeeStatus.Default;
                        break;
                    default:
                        status = (EmployeeStatus)employeeStatus;
                        break;
                }
            }

            var users = String.IsNullOrEmpty(_context.FilterValue) ?
                            CoreContext.UserManager.GetUsers(status).AsEnumerable() :
                            CoreContext.UserManager.Search(_context.FilterValue, status).AsEnumerable();

            if (groupId != null && !groupId.Equals(Guid.Empty))
            {
                users = users.Where(x => CoreContext.UserManager.IsUserInGroup(x.ID, (Guid)groupId));
            }
            if (activationStatus != null)
            {
                users = activationStatus == EmployeeActivationStatus.Activated ?
                            users.Where(x => x.ActivationStatus.HasFlag(EmployeeActivationStatus.Activated)) :
                            users.Where(x => x.ActivationStatus == EmployeeActivationStatus.NotActivated ||
                                             x.ActivationStatus == EmployeeActivationStatus.Pending ||
                                             x.ActivationStatus == EmployeeActivationStatus.AutoGenerated);
            }
            if (employeeType != null)
            {
                switch (employeeType)
                {
                    case EmployeeType.User:
                        users = users.Where(x => !x.IsVisitor());
                        break;
                    case EmployeeType.Visitor:
                        users = users.Where(x => x.IsVisitor());
                        break;
                }
            }

            if (isAdministrator.HasValue && isAdministrator.Value)
            {
                users = users.Where(x => x.IsAdmin() || x.GetListAdminModules().Any());
            }

            _context.TotalCount = users.Count();

            switch (_context.SortBy)
            {
                case "firstname":
                    users = _context.SortDescending ? users.OrderByDescending(r => r, UserInfoComparer.FirstName) : users.OrderBy(r => r, UserInfoComparer.FirstName);
                    break;
                case "lastname":
                    users = _context.SortDescending ? users.OrderByDescending(r => r, UserInfoComparer.LastName) : users.OrderBy(r => r, UserInfoComparer.LastName);
                    break;
                default:
                    users = _context.SortDescending ? users.OrderByDescending(r => r, UserInfoComparer.Default) : users.OrderBy(r => r, UserInfoComparer.Default);
                    break;
            }

            users = users.Skip((int)_context.StartIndex).Take((int)_context.Count - 1);

            _context.SetDataSorted();
            _context.SetDataPaginated();

            return users;
        }

        /// <summary>
        /// Adds a new portal user with the first name, last name, email address, and several optional parameters specified in the request.
        /// </summary>
        /// <short>
        /// Add a user
        /// </short>
        /// <category>Profiles</category>
        /// <param type="System.Boolean, System" name="isVisitor">Specifies if this is a guest (true) or user (false)</param>
        /// <param type="System.String, System" name="email">User email</param>
        /// <param type="System.String, System" name="firstname">User first name</param>
        /// <param type="System.String, System" name="lastname">User last name</param>
        /// <param type="System.Guid[], System" name="department" optional="true">User department</param>
        /// <param type="System.String, System" name="title" optional="true">User title</param>
        /// <param type="System.String, System" name="location" optional="true">User location</param>
        /// <param type="System.String, System" name="sex" optional="true">User sex (male or female)</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="birthday" optional="true">User birthday</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="worksfrom" optional="true">User registration date. If it is not specified, then the current date will be set</param>
        /// <param type="System.String, System" name="comment" optional="true">User comment</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Employee.Contact}, System.Collections.Generic" name="contacts">Contact list</param>
        /// <param type="System.String, System" name="files">Avatar photo URL</param>
        /// <param type="System.String, System" name="password" optional="true">User password</param>
        /// <param type="System.String, System" name="passwordHash" visible="false">Password hash</param>
        /// <param type="System.Nullable{System.Guid}, System" name="lead">User lead</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">Newly added user</returns>
        /// <path>api/2.0/people</path>
        /// <httpMethod>POST</httpMethod>
        [Create("")]
        public EmployeeWraperFull AddMember(bool isVisitor, string email, string firstname, string lastname, Guid[] department, string title, string location, string sex, ApiDateTime birthday, ApiDateTime worksfrom, string comment, IEnumerable<Contact> contacts, string files, string password, string passwordHash, Guid? lead)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_AddRemoveUser);

            passwordHash = (passwordHash ?? "").Trim();
            if (string.IsNullOrEmpty(passwordHash))
            {
                password = (password ?? "").Trim();

                if (String.IsNullOrEmpty(password))
                {
                    password = UserManagerWrapper.GeneratePassword();
                }
                else
                {
                    UserManagerWrapper.CheckPasswordPolicy(password);
                }

                passwordHash = PasswordHasher.GetClientPassword(password);
            }

            var user = new UserInfo();

            //Validate email
            var address = new MailAddress(email);
            user.Email = address.Address;
            //Set common fields
            user.FirstName = firstname;
            user.LastName = lastname;
            user.Title = title;
            user.Location = location;
            user.Notes = comment;
            user.Sex = "male".Equals(sex, StringComparison.OrdinalIgnoreCase)
                           ? true
                           : ("female".Equals(sex, StringComparison.OrdinalIgnoreCase) ? (bool?)false : null);

            user.BirthDate = birthday != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(birthday)) : (DateTime?)null;
            user.WorkFromDate = worksfrom != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(worksfrom)) : DateTime.UtcNow.Date;
            user.Lead = lead;


            UpdateContacts(contacts, user);
            Cache.Insert("REWRITE_URL" + CoreContext.TenantManager.GetCurrentTenant().TenantId, HttpContext.Current.Request.GetUrlRewriter().ToString(), TimeSpan.FromMinutes(5));
            user = UserManagerWrapper.AddUser(user, passwordHash, false, true, isVisitor, false, true, true);

            var messageAction = isVisitor ? MessageAction.GuestCreated : MessageAction.UserCreated;
            MessageService.Send(Request, messageAction, MessageTarget.Create(user.ID), user.DisplayUserName(false));

            UpdateDepartments(department, user);

            if (files != UserPhotoManager.GetDefaultPhotoAbsoluteWebPath())
            {
                UpdatePhotoUrl(files, user);
            }

            var quotaSettings = TenantUserQuotaSettings.Load();

            if (quotaSettings.EnableUserQuota)
            {
                var newserQuotaSettings = new UserQuotaSettings { UserQuota = quotaSettings.DefaultUserQuota };

                newserQuotaSettings.SaveForUser(user.ID);
            }

            //this eject

            return new EmployeeWraperFull(user);
        }


        private static void UpdateDepartments(IEnumerable<Guid> department, UserInfo user)
        {
            if (!SecurityContext.CheckPermissions(Core.Users.Constants.Action_EditGroups)) return;
            if (department == null) return;

            var groups = CoreContext.UserManager.GetUserGroups(user.ID);
            var managerGroups = new List<Guid>();
            foreach (var groupInfo in groups)
            {
                CoreContext.UserManager.RemoveUserFromGroup(user.ID, groupInfo.ID);
                var managerId = CoreContext.UserManager.GetDepartmentManager(groupInfo.ID);
                if (managerId == user.ID)
                {
                    managerGroups.Add(groupInfo.ID);
                    CoreContext.UserManager.SetDepartmentManager(groupInfo.ID, Guid.Empty);
                }
            }
            foreach (var guid in department)
            {
                var userDepartment = CoreContext.UserManager.GetGroupInfo(guid);
                if (userDepartment != Core.Users.Constants.LostGroupInfo)
                {
                    CoreContext.UserManager.AddUserIntoGroup(user.ID, guid);
                    if (managerGroups.Contains(guid))
                    {
                        CoreContext.UserManager.SetDepartmentManager(guid, user.ID);
                    }
                }
            }
        }

        private static void UpdateContacts(IEnumerable<Contact> contacts, UserInfo user)
        {
            SecurityContext.DemandPermissions(new UserSecurityProvider(user.ID), Core.Users.Constants.Action_EditUser);
            user.Contacts.Clear();
            if (contacts == null) return;

            foreach (var contact in contacts)
            {
                user.Contacts.Add(contact.Type);
                user.Contacts.Add(contact.Value);
            }
        }

        private static void DeleteContacts(IEnumerable<Contact> contacts, UserInfo user)
        {
            SecurityContext.DemandPermissions(new UserSecurityProvider(user.ID), Core.Users.Constants.Action_EditUser);
            if (contacts == null) return;

            foreach (var contact in contacts)
            {
                var index = user.Contacts.IndexOf(contact.Type);
                if (index != -1)
                {
                    //Remove existing
                    user.Contacts.RemoveRange(index, 2);
                }
            }
        }

        private void UpdatePhotoUrl(string files, UserInfo user)
        {
            if (string.IsNullOrEmpty(files))
            {
                return;
            }

            SecurityContext.DemandPermissions(new UserSecurityProvider(user.ID), Core.Users.Constants.Action_EditUser);

            // hack. http://ubuntuforums.org/showthread.php?t=1841740
            if (WorkContext.IsMono)
            {
                ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
            }

            if (!files.StartsWith("http://") && !files.StartsWith("https://"))
            {
                files = _context.RequestContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Scheme | UriPartial.Authority) + "/" + files.TrimStart('/');
            }
            var request = HttpWebRequest.Create(files);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var inputStream = response.GetResponseStream())
                using (var br = new BinaryReader(inputStream))
                {
                    var imageByteArray = br.ReadBytes((int)response.ContentLength);
                    UserPhotoManager.SaveOrUpdatePhoto(user.ID, imageByteArray);
                }
            }
        }

        /// <summary>
        /// Updates the data for the selected portal user with the first name, last name, email address, and/or optional parameters specified in the request.
        /// </summary>
        /// <short>
        /// Update a user
        /// </short>
        /// <category>Profiles</category>
        /// <param type="System.Boolean, System" name="isVisitor">Specifies if this is a guest (true) or user (false)</param>
        /// <param type="System.String, System" name="userid">User ID</param>
        /// <param type="System.String, System" name="firstname">New user first name</param>
        /// <param type="System.String, System" name="lastname">New user last name</param>
        /// <param type="System.String, System" name="comment" optional="true">New user comment</param>
        /// <param type="System.Guid[], System" name="department" optional="true">New user department</param>
        /// <param type="System.String, System" name="title" optional="true">New user title</param>
        /// <param type="System.String, System" name="location" optional="true">New user location</param>
        /// <param type="System.String, System" name="sex" optional="true">New user sex (male or female)</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="birthday" optional="true">New user birthday</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="worksfrom" optional="true">New user registration date. If it is not specified, then the current date will be set</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Employee.Contact}, System.Collections.Generic" name="contacts">New contact list</param>
        /// <param type="System.String, System" name="files">New avatar photo URL</param>
        /// <param type="System.Nullable{System.Boolean}, System" name="disable">Specifies whether to disable a user on the portal or not</param>
        /// <param type="System.Nullable{System.Guid}, System" name="lead">User lead</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">Updated user</returns>
        /// <path>api/2.0/people/{userid}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("{userid}")]
        public EmployeeWraperFull UpdateMember(bool isVisitor, string userid, string firstname, string lastname, string comment, Guid[] department, string title, string location, string sex, ApiDateTime birthday, ApiDateTime worksfrom, IEnumerable<Contact> contacts, string files, bool? disable, Guid? lead)
        {
            SecurityContext.DemandPermissions(new UserSecurityProvider(new Guid(userid)), Core.Users.Constants.Action_EditUser);

            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            var self = SecurityContext.CurrentAccount.ID.Equals(user.ID);
            var resetDate = new DateTime(1900, 01, 01);

            //Update it

            var isLdap = user.IsLDAP();
            var isSso = user.IsSSO();
            var isAdmin = WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID);

            if (!isLdap && !isSso)
            {
                //Set common fields

                user.FirstName = firstname ?? user.FirstName;
                user.LastName = lastname ?? user.LastName;
                user.Location = location ?? user.Location;

                if (isAdmin)
                {
                    user.Title = title ?? user.Title;
                }
            }

            if (!UserFormatter.IsValidUserName(user.FirstName, user.LastName))
                throw new Exception(Resource.ErrorIncorrectUserName);

            user.Notes = comment ?? user.Notes;
            user.Sex = ("male".Equals(sex, StringComparison.OrdinalIgnoreCase)
                            ? true
                            : ("female".Equals(sex, StringComparison.OrdinalIgnoreCase) ? (bool?)false : null)) ?? user.Sex;

            user.BirthDate = birthday != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(birthday)) : user.BirthDate;

            if (user.BirthDate == resetDate)
            {
                user.BirthDate = null;
            }

            user.WorkFromDate = worksfrom != null ? TenantUtil.DateTimeFromUtc(Convert.ToDateTime(worksfrom)) : user.WorkFromDate;

            if (user.WorkFromDate == resetDate)
            {
                user.WorkFromDate = null;
            }

            if (isAdmin)
            {
                user.Lead = lead;
            }
            //Update contacts
            UpdateContacts(contacts, user);
            UpdateDepartments(department, user);

            if (files != UserPhotoManager.GetPhotoAbsoluteWebPath(user.ID))
            {
                UpdatePhotoUrl(files, user);
            }
            if (disable.HasValue)
            {
                user.Status = disable.Value ? EmployeeStatus.Terminated : EmployeeStatus.Active;
                user.TerminatedDate = disable.Value ? DateTime.UtcNow : (DateTime?)null;
            }

            if (self && !isAdmin)
            {
                StudioNotifyService.Instance.SendMsgToAdminAboutProfileUpdated();
            }

            // change user type
            var canBeGuestFlag = !user.IsOwner() && !user.IsAdmin() && !user.GetListAdminModules().Any() && !user.IsMe();

            if (isVisitor && !user.IsVisitor() && canBeGuestFlag)
            {
                CoreContext.UserManager.AddUserIntoGroup(user.ID, Core.Users.Constants.GroupVisitor.ID);
                WebItemSecurity.ClearCache();
            }

            if (!self && !isVisitor && user.IsVisitor())
            {
                var usersQuota = TenantExtra.GetTenantQuota().ActiveUsers;
                if (TenantStatisticsProvider.GetUsersCount() < usersQuota)
                {
                    CoreContext.UserManager.RemoveUserFromGroup(user.ID, Core.Users.Constants.GroupVisitor.ID);
                    WebItemSecurity.ClearCache();
                }
                else
                {
                    throw new TenantQuotaException(string.Format("Exceeds the maximum active users ({0})", usersQuota));
                }
            }

            CoreContext.UserManager.SaveUserInfo(user, isVisitor, true);
            MessageService.Send(Request, MessageAction.UserUpdated, MessageTarget.Create(user.ID), user.DisplayUserName(false));

            if (disable.HasValue && disable.Value)
            {
                CookiesManager.ResetUserCookie(user.ID);
                MessageService.Send(HttpContext.Current.Request, MessageAction.CookieSettingsUpdated);
            }

            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Deletes a user with the ID specified in the request from the portal.
        /// </summary>
        /// <short>
        /// Delete a user
        /// </short>
        /// <category>Profiles</category>
        /// <param type="System.String, System" method="url" name="userid">User ID</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">Deleted user</returns>
        /// <path>api/2.0/people/{userid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete("{userid}")]
        public EmployeeWraperFull DeleteMember(string userid)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_AddRemoveUser);

            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID) || user.IsLDAP())
                throw new SecurityException();

            if (user.Status != EmployeeStatus.Terminated)
                throw new Exception("The user is not suspended");

            CheckReassignProccess(new[] { user.ID });

            var userName = user.DisplayUserName(false);

            UserPhotoManager.RemovePhoto(user.ID);
            CoreContext.UserManager.DeleteUser(user.ID);
            QueueWorker.StartRemove(HttpContext.Current, TenantProvider.CurrentTenantID, user, SecurityContext.CurrentAccount.ID, false);

            MessageService.Send(Request, MessageAction.UserDeleted, MessageTarget.Create(user.ID), userName);

            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Updates the contact information of the user with the ID specified in the request merging the new data into the current portal data.
        /// </summary>
        /// <short>
        /// Update user contacts
        /// </short>
        /// <category>Contacts</category>
        /// <param type="System.String, System" method="url" name="userid">User ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Employee.Contact}, System.Collections.Generic" name="contacts">List of new contacts</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">Updated user profile</returns>
        /// <path>api/2.0/people/{userid}/contacts</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("{userid}/contacts")]
        public EmployeeWraperFull UpdateMemberContacts(string userid, IEnumerable<Contact> contacts)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            UpdateContacts(contacts, user);
            CoreContext.UserManager.SaveUserInfo(user, syncCardDav: true);
            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Sets the contacts of the user with the ID specified in the request replacing the current portal data with the new data.
        /// </summary>
        /// <short>
        /// Set user contacts
        /// </short>
        /// <category>Contacts</category>
        /// <param type="System.String, System" method="url" name="userid">User ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Employee.Contact}, System.Collections.Generic" name="contacts">List of new contacts</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">Updated user profile</returns>
        /// <path>api/2.0/people/{userid}/contacts</path>
        /// <httpMethod>POST</httpMethod>
        [Create("{userid}/contacts")]
        public EmployeeWraperFull SetMemberContacts(string userid, IEnumerable<Contact> contacts)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            user.Contacts.Clear();
            UpdateContacts(contacts, user);
            CoreContext.UserManager.SaveUserInfo(user, syncCardDav: true);
            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Deletes the contacts of the user with the ID specified in the request from the portal.
        /// </summary>
        /// <short>
        /// Delete user contacts
        /// </short>
        /// <category>Contacts</category>
        /// <param type="System.String, System" method="url" name="userid">User ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Employee.Contact}, System.Collections.Generic" name="contacts">List of contacts</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">Updated user profile</returns>
        /// <path>api/2.0/people/{userid}/contacts</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete("{userid}/contacts")]
        public EmployeeWraperFull DeleteMemberContacts(string userid, IEnumerable<Contact> contacts)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            DeleteContacts(contacts, user);
            CoreContext.UserManager.SaveUserInfo(user, syncCardDav: true);
            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Returns a photo of the user with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get a user photo
        /// </short>
        /// <category>Photos</category>
        /// <param type="System.String, System" method="url" name="userid">User ID</param>
        /// <returns type="ASC.Api.Employee.ThumbnailsDataWrapper, ASC.Api.Employee">User photo</returns>
        /// <path>api/2.0/people/{userid}/photo</path>
        /// <httpMethod>GET</httpMethod>
        [Read("{userid}/photo")]
        public ThumbnailsDataWrapper GetMemberPhoto(string userid)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            return new ThumbnailsDataWrapper(user.ID);
        }

        /// <summary>
        /// Updates a photo of the user with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Update a user photo
        /// </short>
        /// <category>Photos</category>
        /// <param type="System.String, System" method="url" name="userid">User ID</param>
        /// <param type="System.String, System" name="files">New avatar photo URL</param>
        /// <returns type="ASC.Api.Employee.ThumbnailsDataWrapper, ASC.Api.Employee">Updated user photo</returns>
        /// <path>api/2.0/people/{userid}/photo</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("{userid}/photo")]
        public ThumbnailsDataWrapper UpdateMemberPhoto(string userid, string files)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            if (files != UserPhotoManager.GetPhotoAbsoluteWebPath(user.ID))
            {
                UpdatePhotoUrl(files, user);
            }

            CoreContext.UserManager.SaveUserInfo(user, syncCardDav: true);
            MessageService.Send(Request, MessageAction.UserAddedAvatar, MessageTarget.Create(user.ID), user.DisplayUserName(false));

            return new ThumbnailsDataWrapper(user.ID);
        }

        /// <summary>
        /// Deletes a photo of the user with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Delete a user photo
        /// </short>
        /// <category>Photos</category>
        /// <param type="System.String, System" method="url" name="userid">User ID</param>
        /// <returns type="ASC.Api.Employee.ThumbnailsDataWrapper,  ASC.Api.Employee">Deleted user photo</returns>
        /// <path>api/2.0/people/{userid}/photo</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete("{userid}/photo")]
        public ThumbnailsDataWrapper DeleteMemberPhoto(string userid)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            SecurityContext.DemandPermissions(new UserSecurityProvider(user.ID), Core.Users.Constants.Action_EditUser);

            UserPhotoManager.RemovePhoto(user.ID);

            CoreContext.UserManager.SaveUserInfo(user, syncCardDav: true);
            MessageService.Send(Request, MessageAction.UserDeletedAvatar, MessageTarget.Create(user.ID), user.DisplayUserName(false));

            return new ThumbnailsDataWrapper(user.ID);
        }

        /// <summary>
        /// Creates a photo thumbnail by coordinates of the original image specified in the request.
        /// </summary>
        /// <short>
        /// Create a photo thumbnail
        /// </short>
        /// <category>Photos</category>
        /// <param type="System.String, System" method="url" name="userid">User ID</param>
        /// <param type="System.String, System" name="tmpFile">Path to the temporary file</param>
        /// <param type="System.Int32, System" name="x">Horizontal coordinate</param>
        /// <param type="System.Int32, System" name="y">Vertical coordinate</param>
        /// <param type="System.Int32, System" name="width">Thumbnail width</param>
        /// <param type="System.Int32, System" name="height">Thumbnail height</param>
        /// <path>api/2.0/people/{userid}/photo/thumbnails</path>
        /// <httpMethod>POST</httpMethod>
        /// <returns type="ASC.Api.Employee.ThumbnailsDataWrapper, ASC.Api.Employee">Thumbnail</returns>
        [Create("{userid}/photo/thumbnails")]
        public ThumbnailsDataWrapper CreateMemberPhotoThumbnails(string userid, string tmpFile, int x, int y, int width, int height)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            SecurityContext.DemandPermissions(new UserSecurityProvider(user.ID), Core.Users.Constants.Action_EditUser);

            if (!string.IsNullOrEmpty(tmpFile))
            {
                var fileName = Path.GetFileName(tmpFile);
                var data = UserPhotoManager.GetTempPhotoData(fileName);

                var settings = new UserPhotoThumbnailSettings(x, y, width, height);
                settings.SaveForUser(user.ID);

                UserPhotoManager.SaveOrUpdatePhoto(user.ID, data);
                UserPhotoManager.RemoveTempPhoto(fileName);
            }
            else
            {
                UserPhotoThumbnailManager.SaveThumbnails(x, y, width, height, user.ID);
            }

            CoreContext.UserManager.SaveUserInfo(user, syncCardDav: true);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserUpdatedAvatarThumbnails, MessageTarget.Create(user.ID), user.DisplayUserName(false));

            return new ThumbnailsDataWrapper(user.ID);
        }

        /// <summary>
        /// Sets a new email to the user with the ID specified in the request.
        /// </summary>
        /// <short>Change a user email</short>
        /// <category>Email</category>
        /// <param type="System.Guid, System" name="userid">User ID</param>
        /// <param type="System.String, System" name="email">New email</param>
        /// <returns>Detailed user information</returns>
        /// <path>api/2.0/people/{userid}/email</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("{userid}/email")]
        public EmployeeWraperFull ChangeUserEmail(Guid userid, string email)
        {
            SecurityContext.DemandPermissions(new UserSecurityProvider(userid), Core.Users.Constants.Action_EditUser);

            var user = CoreContext.UserManager.GetUserByEmail(email);
            if (user.ID != Core.Users.Constants.LostUser.ID)
            {
                throw new Exception(CustomNamingPeople.Substitute<Resource>("ErrorEmailAlreadyExists"));
            }

            user = CoreContext.UserManager.GetUsers(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID) ||
                user.Status == EmployeeStatus.Terminated ||
                user.IsLDAP() ||
                user.IsSSO() ||
                (user.IsOwner() && user.ID != SecurityContext.CurrentAccount.ID))
            {
                throw new SecurityException(Resource.ErrorAccessDenied);
            }

            if (!string.IsNullOrEmpty(email))
            {
                var address = new MailAddress(email);
                if (!string.Equals(address.Address, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var messageDescription = user.DisplayUserName(false);
                    var messageTarget = MessageTarget.Create(user.ID);

                    user.Email = address.Address.ToLowerInvariant();

                    if (user.ActivationStatus.HasFlag(EmployeeActivationStatus.AutoGenerated))
                    {
                        user.ActivationStatus = EmployeeActivationStatus.AutoGenerated;
                    }
                    else
                    {
                        user.ActivationStatus = EmployeeActivationStatus.NotActivated;
                    }

                    CoreContext.UserManager.SaveUserInfo(user, syncCardDav: true);
                    MessageService.Send(Request, MessageAction.UserUpdatedEmail, messageTarget, messageDescription);

                    CookiesManager.ResetUserCookie(user.ID);

                    StudioNotifyService.Instance.SendEmailActivationInstructions(user, email);
                    MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentActivationInstructions, messageTarget, messageDescription);
                }
            }

            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Reminds a password to the user using the email address specified in the request.
        /// </summary>
        /// <short>
        /// Remind a user password
        /// </short>
        /// <category>Password</category>
        /// <param type="System.String, System" name="email">User email</param>
        /// <returns>Email with the password</returns>
        /// <path>api/2.0/people/password</path>
        /// <httpMethod>POST</httpMethod>
        /// <requiresAuthorization>false</requiresAuthorization>
        /// <visible>false</visible>
        [Create("password", false, false)] //NOTE: this method doesn't require auth!!!  //NOTE: this method doesn't check payment!!!
        public string SendUserPassword(string email)
        {
            string error;
            if (!string.IsNullOrEmpty(error = UserManagerWrapper.SendUserPassword(email)))
            {
                Log.ErrorFormat("Password recovery ({0}): {1}", email, error);
            }

            return String.Format(Resource.MessageYourPasswordSendedToEmail, email);
        }

        /// <summary>
        /// Sets a new password to the user with the ID specified in the request.
        /// </summary>
        /// <short>Change a user password</short>
        /// <category>Password</category>
        /// <param type="System.Guid, System" name="userid">User ID</param>
        /// <param type="System.String, System" name="password">New password</param>
        /// <returns>Detailed user information</returns>
        /// <path>api/2.0/people/{userid}/password</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("{userid}/password")]
        public EmployeeWraperFull ChangeUserPassword(Guid userid, string password)
        {
            SecurityContext.DemandPermissions(new UserSecurityProvider(userid), Core.Users.Constants.Action_EditUser);

            var user = CoreContext.UserManager.GetUsers(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID) ||
                user.Status == EmployeeStatus.Terminated ||
                user.IsLDAP() ||
                user.IsSSO() ||
                (user.IsOwner() && user.ID != SecurityContext.CurrentAccount.ID))
            {
                throw new SecurityException(Resource.ErrorAccessDenied);
            }

            if (!string.IsNullOrEmpty(password))
            {
                var messageDescription = user.DisplayUserName(false);
                var messageTarget = MessageTarget.Create(user.ID);

                UserManagerWrapper.CheckPasswordPolicy(password);

                var passwordHash = PasswordHasher.GetClientPassword(password);

                SecurityContext.SetUserPasswordHash(userid, passwordHash);
                MessageService.Send(HttpContext.Current.Request, MessageAction.UserUpdatedPassword, messageTarget, messageDescription);

                CookiesManager.ResetUserCookie(userid);
                MessageService.Send(HttpContext.Current.Request, MessageAction.CookieSettingsUpdated, messageTarget, messageDescription);
            }

            return new EmployeeWraperFull(user);
        }

        private static UserInfo GetUserInfo(string userNameOrId)
        {
            UserInfo user;
            try
            {
                var userId = new Guid(userNameOrId);
                user = CoreContext.UserManager.GetUsers(userId);
            }
            catch (FormatException)
            {
                user = CoreContext.UserManager.GetUserByUserName(userNameOrId);
            }
            if (user == null || user.ID == Core.Users.Constants.LostUser.ID)
                throw new ItemNotFoundException("user not found");
            return user;
        }

        /// <summary>
        /// Sets the required activation status to the user with the ID specified in the request.
        /// </summary>
        /// <short>Set an activation status to the user</short>
        /// <category>Activation status</category>
        /// <param type="System.Guid, System" name="userid">User ID</param>
        /// <param type="ASC.Core.Users.EmployeeActivationStatus, ASC.Core.Users" name="activationstatus">Activation status ("NotActivated", "Activated", "Pending", or "AutoGenerated")</param>
        /// <returns>Detailed user information</returns>
        /// <path>api/2.0/people/{userid}/activationstatus</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update("{userid}/activationstatus")]
        public EmployeeWraperFull UpdateEmployeeActivationStatus(Guid userid, EmployeeActivationStatus activationstatus)
        {
            var user = ChangeEmployeeActivationStatus(userid, activationstatus);

            return new EmployeeWraperFull(user);
        }


        /// <summary>
        /// Sets the required activation status to the list of users with the IDs specified in the request.
        /// </summary>
        /// <short>
        /// Set an activation status to the users
        /// </short>
        /// <category>Activation status</category>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="userIds">List of user IDs</param>
        /// <param type="ASC.Core.Users.EmployeeActivationStatus, ASC.Core.Users" name="activationstatus">Activation status ("NotActivated", "Activated", "Pending", or "AutoGenerated")</param>
        /// <returns>List of users</returns>
        /// <path>api/2.0/people/activationstatus/{activationstatus}</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        /// <visible>false</visible>
        [Update("activationstatus/{activationstatus}")]
        public IEnumerable<EmployeeWraperFull> UpdateEmployeeActivationStatus(EmployeeActivationStatus activationstatus, IEnumerable<Guid> userIds)
        {
            var retuls = new List<EmployeeWraperFull>();

            foreach (var id in userIds.Where(userId => !CoreContext.UserManager.IsSystemUser(userId)))
            {
                var user = ChangeEmployeeActivationStatus(id, activationstatus);

                retuls.Add(new EmployeeWraperFull(user));
            }

            return retuls;
        }

        private UserInfo ChangeEmployeeActivationStatus(Guid userid, EmployeeActivationStatus activationstatus)
        {
            SecurityContext.DemandPermissions(new UserSecurityProvider(userid), Core.Users.Constants.Action_EditUser);

            var user = CoreContext.UserManager.GetUsers(userid);

            if (!Web.Studio.Core.SetupInfo.IsSecretEmail(user.Email))
            {
                throw new SecurityException(Resource.ErrorAccessDenied);
            }

            if (CoreContext.UserManager.IsSystemUser(user.ID) ||
                user.Status == EmployeeStatus.Terminated ||
                user.IsLDAP() ||
                user.IsSSO() ||
                (user.IsOwner() && user.ID != SecurityContext.CurrentAccount.ID))
            {
                throw new SecurityException(Resource.ErrorAccessDenied);
            }

            user.ActivationStatus = activationstatus;

            CoreContext.UserManager.SaveUserInfo(user, syncCardDav: true);
            MessageService.Send(Request, MessageAction.UserUpdated, MessageTarget.Create(user.ID), user.DisplayUserName(false));

            return user;
        }

        /// <summary>
        /// Changes a type (user or visitor) for the users with the IDs specified in the request.
        /// </summary>
        /// <short>
        /// Change a user type
        /// </short>
        /// <category>User type</category>
        /// <param type="ASC.Core.Users.EmployeeType, ASC.Core.Users" method="url" name="type">New user type ("All", "User", or "Visitor")</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="userIds">List of user IDs</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">User list</returns>
        /// <path>api/2.0/people/type/{type}</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update("type/{type}")]
        public IEnumerable<EmployeeWraperFull> UpdateUserType(EmployeeType type, IEnumerable<Guid> userIds)
        {
            var users = userIds
                .Where(userId => !CoreContext.UserManager.IsSystemUser(userId))
                .Select(userId => CoreContext.UserManager.GetUsers(userId))
                .ToList();

            foreach (var user in users)
            {
                if (user.IsOwner() || user.IsAdmin() || user.IsMe() || user.GetListAdminModules().Any())
                    continue;

                switch (type)
                {
                    case EmployeeType.User:
                        if (user.IsVisitor())
                        {
                            if (TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers)
                            {
                                CoreContext.UserManager.RemoveUserFromGroup(user.ID, Core.Users.Constants.GroupVisitor.ID);
                                WebItemSecurity.ClearCache();
                            }
                        }
                        break;
                    case EmployeeType.Visitor:
                        if (CoreContext.Configuration.Standalone || TenantStatisticsProvider.GetVisitorsCount() < TenantExtra.GetTenantQuota().ActiveUsers * Core.Users.Constants.CoefficientOfVisitors)
                        {
                            CoreContext.UserManager.AddUserIntoGroup(user.ID, Core.Users.Constants.GroupVisitor.ID);
                            WebItemSecurity.ClearCache();
                        }
                        break;
                }
            }

            MessageService.Send(Request, MessageAction.UsersUpdatedType, MessageTarget.Create(users.Select(x => x.ID)), users.Select(x => x.DisplayUserName(false)));

            return users.Select(user => new EmployeeWraperFull(user));
        }

        

        /// <summary>
        /// Changes a status for the users with the IDs specified in the request.
        /// </summary>
        /// <short>
        /// Change a user status
        /// </short>
        /// <category>User status</category>
        /// <param type="ASC.Core.Users.EmployeeStatus, ASC.Core.Users" method="url" name="status">New user status ("Active", "Terminated", "LeaveOfAbsence", "All", or "Default"</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="userIds">List of user IDs</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">User list</returns>
        /// <path>api/2.0/people/status/{status}</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update("status/{status}")]
        public IEnumerable<EmployeeWraperFull> UpdateUserStatus(EmployeeStatus status, IEnumerable<Guid> userIds)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            var users = userIds.Select(userId => CoreContext.UserManager.GetUsers(userId))
                .Where(u => !CoreContext.UserManager.IsSystemUser(u.ID) && !u.IsLDAP())
                .ToList();

            foreach (var user in users)
            {
                if (user.IsOwner() || user.IsMe())
                    continue;

                switch (status)
                {
                    case EmployeeStatus.Active:
                        if (user.Status == EmployeeStatus.Terminated)
                        {
                            if (TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers || user.IsVisitor())
                            {
                                user.Status = EmployeeStatus.Active;
                                CoreContext.UserManager.SaveUserInfo(user, syncCardDav: true);
                            }
                        }
                        break;
                    case EmployeeStatus.Terminated:
                        user.Status = EmployeeStatus.Terminated;
                        CoreContext.UserManager.SaveUserInfo(user, syncCardDav: true);

                        CookiesManager.ResetUserCookie(user.ID);
                        MessageService.Send(HttpContext.Current.Request, MessageAction.CookieSettingsUpdated);
                        break;
                }
            }

            MessageService.Send(Request, MessageAction.UsersUpdatedStatus, MessageTarget.Create(users.Select(x => x.ID)), users.Select(x => x.DisplayUserName(false)));

            return users.Select(user => new EmployeeWraperFull(user));
        }

        /// <summary>
        /// Resends emails to the users who have not activated their emails.
        /// </summary>
        /// <short>
        /// Resend an activation email
        /// </short>
        /// <category>Profiles</category>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="userIds">List of user IDs</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">List of users</returns>
        /// <path>api/2.0/people/invite</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update("invite")]
        public IEnumerable<EmployeeWraperFull> ResendUserInvites(IEnumerable<Guid> userIds)
        {
            var users = userIds
                .Where(userId => !CoreContext.UserManager.IsSystemUser(userId))
                .Select(userId => CoreContext.UserManager.GetUsers(userId))
                .ToList();

            foreach (var user in users)
            {
                if (user.IsActive) continue;

                if (user.ActivationStatus == EmployeeActivationStatus.Pending)
                {
                    if (user.IsVisitor())
                    {
                        StudioNotifyService.Instance.GuestInfoActivation(user);
                    }
                    else
                    {
                        StudioNotifyService.Instance.UserInfoActivation(user);
                    }
                }
                else
                {
                    StudioNotifyService.Instance.SendEmailActivationInstructions(user, user.Email);
                }
            }

            MessageService.Send(Request, MessageAction.UsersSentActivationInstructions, MessageTarget.Create(users.Select(x => x.ID)), users.Select(x => x.DisplayUserName(false)));

            return users.Select(user => new EmployeeWraperFull(user));
        }

        /// <summary>
        /// Deletes a list of the users with the IDs specified in the request.
        /// </summary>
        /// <short>
        /// Delete users
        /// </short>
        /// <category>Profiles</category>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="userIds">List of user IDs</param>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee">List of users</returns>
        /// <path>api/2.0/people/delete</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update("delete")]
        public IEnumerable<EmployeeWraperFull> RemoveUsers(IEnumerable<Guid> userIds)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_AddRemoveUser);

            CheckReassignProccess(userIds);

            var users = userIds.Select(userId => CoreContext.UserManager.GetUsers(userId))
                .Where(u => !CoreContext.UserManager.IsSystemUser(u.ID) && !u.IsLDAP())
                .ToList();

            var userNames = users.Select(x => x.DisplayUserName(false)).ToList();

            foreach (var user in users)
            {
                if (user.Status != EmployeeStatus.Terminated) continue;

                UserPhotoManager.RemovePhoto(user.ID);
                CoreContext.UserManager.DeleteUser(user.ID);
                QueueWorker.StartRemove(HttpContext.Current, TenantProvider.CurrentTenantID, user, SecurityContext.CurrentAccount.ID, false);

            }

            MessageService.Send(Request, MessageAction.UsersDeleted, MessageTarget.Create(users.Select(x => x.ID)), userNames);

            return users.Select(user => new EmployeeWraperFull(user));
        }


        /// <summary>
        /// Sends instructions for deleting a user profile.
        /// </summary>
        /// <short>
        /// Send the deletion instructions
        /// </short>
        /// <category>Profiles</category>
        /// <returns>Information message</returns>
        /// <path>api/2.0/people/self/delete</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("self/delete")]
        public string SendInstructionsToDelete()
        {
            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (user.IsLDAP())
                throw new SecurityException();

            StudioNotifyService.Instance.SendMsgProfileDeletion(user);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentDeleteInstructions);

            return String.Format(Resource.SuccessfullySentNotificationDeleteUserInfoMessage, "<b>" + user.Email + "</b>");
        }


        /// <summary>
        /// Subscribes to or unsubscribes from the birthday of the user with the ID specified in the request.
        /// </summary>
        /// <short>Birthday subscription</short>
        /// <param type="System.Guid, System" name="userid">User ID</param>
        /// <param type="System.Boolean, System" name="onRemind">Defines if the user will be notified about another user's birthday or not</param>
        /// <returns>Bool value: true means that the user will get a notification</returns>
        /// <category>Birthday</category>
        /// <path>api/2.0/people/birthdays/reminder</path>
        /// <httpMethod>POST</httpMethod>
        [Create("birthdays/reminder")]
        public bool RemindAboutBirthday(Guid userid, bool onRemind)
        {
            BirthdaysNotifyClient.Instance.SetSubscription(SecurityContext.CurrentAccount.ID, userid, onRemind);
            return onRemind;
        }

        #region Auth page hidden methods

        /// <summary>
        /// Links a third-party account specified in the request to the user profile.
        /// </summary>
        /// <short>
        /// Link a third-pary account
        /// </short>
        /// <category>Third-party accounts</category>
        /// <param type="System.String, System" name="serializedProfile">Third-party profile in the serialized format</param>
        /// <path>api/2.0/people/thirdparty/linkaccount</path>
        /// <httpMethod>PUT</httpMethod>
        ///<visible>false</visible>
        [Update("thirdparty/linkaccount")]
        public void LinkAccount(string serializedProfile)
        {
            var profile = new LoginProfile(serializedProfile);

            if (!(CoreContext.Configuration.Standalone ||
                     CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Oauth))
            {
                throw new Exception("ErrorNotAllowedOption");
            }
            if (string.IsNullOrEmpty(profile.AuthorizationError))
            {
                GetLinker().AddLink(SecurityContext.CurrentAccount.ID.ToString(), profile);
                MessageService.Send(HttpContext.Current.Request, MessageAction.UserLinkedSocialAccount, GetMeaningfulProviderName(profile.Provider));
            }
            else
            {
                // ignore cancellation
                if (profile.AuthorizationError != "Canceled at provider")
                {
                    throw new Exception(profile.AuthorizationError);
                }
            }
        }

        /// <summary>
        /// Unlinks a third-party account specified in the request from the user profile.
        /// </summary>
        /// <short>
        /// Unlink a third-pary account
        /// </short>
        /// <category>Third-party accounts</category>
        /// <param type="System.String, System" name="provider">Provider name</param>
        /// <path>api/2.0/people/thirdparty/unlinkaccount</path>
        /// <httpMethod>DELETE</httpMethod>
        ///<visible>false</visible>
        [Delete("thirdparty/unlinkaccount")]
        public void UnlinkAccount(string provider)
        {
            GetLinker().RemoveProvider(SecurityContext.CurrentAccount.ID.ToString(), provider);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserUnlinkedSocialAccount, GetMeaningfulProviderName(provider));
        }

        private static AccountLinker GetLinker()
        {
            return new AccountLinker("webstudio");
        }

        private static string GetMeaningfulProviderName(string providerName)
        {
            switch (providerName)
            {
                case "google":
                case "openid":
                    return "Google";
                case "facebook":
                    return "Facebook";
                case "twitter":
                    return "Twitter";
                case "linkedin":
                    return "LinkedIn";
                default:
                    return "Unknown Provider";
            }
        }

        #endregion


        #region Reassign user data

        /// <summary>
        /// Returns the progress of the started data reassignment for the user with the ID specified in the request.
        /// </summary>
        /// <short>Get the reassignment progress</short>
        /// <param type="System.Guid, System" method="url" name="userId">User ID whose data is reassigned</param>
        /// <category>User data</category>
        /// <returns type="ASC.Data.Reassigns.ReassignProgressItem, ASC.Data.Reassigns">Reassignment progress</returns>
        /// <path>api/2.0/people/reassign/progress</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"reassign/progress")]
        public ReassignProgressItem GetReassignProgress(Guid userId)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            return QueueWorker.GetProgressItemStatus(TenantProvider.CurrentTenantID, userId, typeof(ReassignProgressItem)) as ReassignProgressItem;
        }

        /// <summary>
        /// Terminates the data reassignment for the user with the ID specified in the request.
        /// </summary>
        /// <short>Terminate the data reassignment</short>
        /// <param type="System.Guid, System" name="userId">User ID whose data is reassigned</param>
        /// <category>User data</category>
        /// <path>api/2.0/people/reassign/terminate</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns></returns>
        [Update(@"reassign/terminate")]
        public void TerminateReassign(Guid userId)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            QueueWorker.Terminate(TenantProvider.CurrentTenantID, userId, typeof(ReassignProgressItem));
        }

        /// <summary>
        /// Starts the data reassignment for the user with the ID specified in the request.
        /// </summary>
        /// <short>Start the data reassignment</short>
        /// <param type="System.Guid, System" name="fromUserId">User ID whose data will be reassigned to another user</param>
        /// <param type="System.Guid, System" name="toUserId">User ID to whom all the data will be reassigned</param>
        /// <param type="System.Boolean, System" name="deleteProfile">Specifies whether to delete a profile when the data reassignment will be finished or not</param>
        /// <category>User data</category>
        /// <returns type="ASC.Data.Reassigns.ReassignProgressItem, ASC.Data.Reassigns">Reassignment progress</returns>
        /// <path>api/2.0/people/reassign/start</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"reassign/start")]
        public ReassignProgressItem StartReassign(Guid fromUserId, Guid toUserId, bool deleteProfile)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            var fromUser = CoreContext.UserManager.GetUsers(fromUserId);

            if (fromUser == null || fromUser.ID == Core.Users.Constants.LostUser.ID)
                throw new ArgumentException("User with id = " + fromUserId + " not found");

            if (fromUser.IsOwner() || fromUser.IsMe() || fromUser.Status != EmployeeStatus.Terminated)
                throw new ArgumentException("Can not delete user with id = " + fromUserId);

            var toUser = CoreContext.UserManager.GetUsers(toUserId);

            if (toUser == null || toUser.ID == Core.Users.Constants.LostUser.ID)
                throw new ArgumentException("User with id = " + toUserId + " not found");

            if (toUser.IsVisitor() || toUser.Status == EmployeeStatus.Terminated)
                throw new ArgumentException("Can not reassign data to user with id = " + toUserId);

            return QueueWorker.StartReassign(HttpContext.Current, TenantProvider.CurrentTenantID, fromUserId, toUserId, SecurityContext.CurrentAccount.ID, deleteProfile);
        }

        private void CheckReassignProccess(IEnumerable<Guid> userIds)
        {
            foreach (var userId in userIds)
            {
                var reassignStatus = QueueWorker.GetProgressItemStatus(TenantProvider.CurrentTenantID, userId, typeof(ReassignProgressItem));
                if (reassignStatus == null || reassignStatus.IsCompleted)
                    continue;

                var userName = CoreContext.UserManager.GetUsers(userId).DisplayUserName();
                throw new Exception(string.Format(Resource.ReassignDataRemoveUserError, userName));
            }
        }

        #endregion


        #region Remove user data

        /// <summary>
        /// Returns the progress of the started data deletion for the user with the ID specified in the request.
        /// </summary>
        /// <short>Get the deletion progress</short>
        /// <param type="System.Guid, System" method="url" name="userId">User ID</param>
        /// <category>User data</category>
        /// <returns type="ASC.Data.Reassigns.RemoveProgressItem, ASC.Data.Reassigns">Deletion progress</returns>
        /// <path>api/2.0/people/remove/progress</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"remove/progress")]
        public RemoveProgressItem GetRemoveProgress(Guid userId)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            return QueueWorker.GetProgressItemStatus(TenantProvider.CurrentTenantID, userId, typeof(RemoveProgressItem)) as RemoveProgressItem;
        }

        /// <summary>
        /// Terminates the data deletion for the user with the ID specified in the request.
        /// </summary>
        /// <short>Terminate the data deletion</short>
        /// <param type="System.Guid, System" name="userId">User ID</param>
        /// <category>User data</category>
        /// <path>api/2.0/people/remove/terminate</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns></returns>
        [Update(@"remove/terminate")]
        public void TerminateRemove(Guid userId)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            QueueWorker.Terminate(TenantProvider.CurrentTenantID, userId, typeof(RemoveProgressItem));
        }

        /// <summary>
        /// Starts the data deletion for the user with the ID specified in the request.
        /// </summary>
        /// <short>Start the data deletion</short>
        /// <param type="System.Guid, System" name="userId">User ID</param>
        /// <category>User data</category>
        /// <returns type="ASC.Data.Reassigns.RemoveProgressItem, ASC.Data.Reassigns">Deletion progress</returns>
        /// <path>api/2.0/people/remove/start</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"remove/start")]
        public RemoveProgressItem StartRemove(Guid userId)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            var user = CoreContext.UserManager.GetUsers(userId);

            if (user == null || user.ID == Core.Users.Constants.LostUser.ID)
                throw new ArgumentException("User with id = " + userId + " not found");

            if (user.IsOwner() || user.IsMe() || user.Status != EmployeeStatus.Terminated)
                throw new ArgumentException("Can not delete user with id = " + userId);

            return QueueWorker.StartRemove(HttpContext.Current, TenantProvider.CurrentTenantID, user, SecurityContext.CurrentAccount.ID, true);
        }

        #endregion
    }
}