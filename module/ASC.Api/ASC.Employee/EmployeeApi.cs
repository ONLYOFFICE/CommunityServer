/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Common.Radicale;
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
using ASC.Web.Studio.Core.Notify;
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

        ///<summary>
        ///Returns the detailed information about the current user profile.
        ///</summary>
        ///<short>
        ///Get my profile
        ///</short>
        ///<category>Profiles</category>
        ///<returns>Detailed information about my profile</returns>
        [Read("@self")]
        public EmployeeWraperFull GetMe()
        {
            return new EmployeeWraperFull(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID));
        }

        ///<summary>
        ///Returns a list of profiles for all the portal users.
        ///</summary>
        ///<short>
        ///Get all profiles
        ///</short>
        ///<category>Profiles</category>
        ///<returns>List of profiles</returns>
        /// <remarks>This method returns the partial profiles. Use more specific method to get full profiles.</remarks>
        [Read("")]
        public IEnumerable<EmployeeWraperFull> GetAll()
        {
            return GetByStatus(EmployeeStatus.Active);
        }

        ///<summary>
        ///Returns a list of profiles filtered by user status.
        ///</summary>
        ///<short>
        ///Get profiles by status
        ///</short>
        ///<category>User status</category>
        ///<param name="status">User status</param>
        ///<returns>List of profiles</returns>
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

        ///<summary>
        ///Returns the detailed information about a profile of the user with the name specified in the request.
        ///</summary>
        ///<short>
        ///Get a profile by user name
        ///</short>
        ///<category>Profiles</category>
        ///<param name="username">User name</param>
        ///<returns>User profile</returns>
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
        ///<param name="email">User email</param>
        ///<returns>User profile</returns>
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
        ///<param name="query">Search query</param>
        ///<returns>List of user profiles</returns>
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
        ///<param name="query">Search query</param>
        ///<returns>List of users</returns>
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
        ///<param name="status">User status</param>
        ///<param name="query">Search query</param>
        ///<returns>List of users</returns>
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
        /// Imports the new portal users with the first name, last name and email address.
        /// </summary>
        /// <short>
        /// Import users
        /// </short>
        /// <category>Profiles</category>
        /// <param name="userList">List of users</param>
        /// <param name="importUsersAsCollaborators" optional="true">Imports users as guests (true) or not (false)</param>
        /// <returns>Newly added users</returns>
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
        /// <category>User status</category>
        /// <returns>Current user information</returns>
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
        /// <param optional="true" name="employeeStatus">User status</param>
        /// <param optional="true" name="groupId">Group ID</param>
        /// <param optional="true" name="activationStatus">Activation status</param>
        /// <param optional="true" name="employeeType">User type</param>
        ///  <param optional="true" name="isAdministrator">Specifies if the user is administrator or not</param>
        /// <returns>
        /// List of users with their information
        /// </returns>
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
        /// <param optional="true" name="employeeStatus">User status</param>
        /// <param optional="true" name="groupId">Group ID</param>
        /// <param optional="true" name="activationStatus">Activation status</param>
        /// <param optional="true" name="employeeType">User type</param>
        ///  <param optional="true" name="isAdministrator">Specifies if the user is administrator or not</param>
        /// <returns>
        /// List of users
        /// </returns>
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
        /// Adds a new portal user with the first name, last name, email address and several optional parameters specified in the request.
        /// </summary>
        /// <short>
        /// Add a user
        /// </short>
        /// <category>Profiles</category>
        /// <param name="isVisitor">Specifies if this is a guest (true) or user (false)</param>
        /// <param name="email">User email</param>
        /// <param name="firstname">User first name</param>
        /// <param name="lastname">User last name</param>
        /// <param name="department" optional="true">User department</param>
        /// <param name="title" optional="true">User title</param>
        /// <param name="location" optional="true">User location</param>
        /// <param name="sex" optional="true">User sex (male or female)</param>
        /// <param name="birthday" optional="true">User birthday</param>
        /// <param name="worksfrom" optional="true">User registration date. If it is not specified, then the current date will be set</param>
        /// <param name="comment" optional="true">User comments</param>
        /// <param name="contacts">Contact list</param>
        /// <param name="files">Avatar photo URL</param>
        /// <param name="password" optional="true">User password</param>
        /// <param name="passwordHash" visible="false">Password hash</param>
        /// <returns>Newly added user</returns>
        [Create("")]
        public EmployeeWraperFull AddMember(bool isVisitor, string email, string firstname, string lastname, Guid[] department, string title, string location, string sex, ApiDateTime birthday, ApiDateTime worksfrom, string comment, IEnumerable<Contact> contacts, string files, string password, string passwordHash)
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

            //this eject

            return new EmployeeWraperFull(user);
        }



        /// <summary>
        /// Adds a new activated portal user with the first name, last name, email address and several optional parameters specified in the request.
        /// </summary>
        /// <short>
        /// Add an activated user
        /// </short>
        /// <category>Profiles</category>
        /// <param name="isVisitor">Specifies if this is a guest (true) or user (false)</param>
        /// <param name="email">User email</param>
        /// <param name="firstname">User first name</param>
        /// <param name="lastname">User last name</param>
        /// <param name="department" optional="true">User department</param>
        /// <param name="title" optional="true">User title</param>
        /// <param name="location" optional="true">User location</param>
        /// <param name="sex" optional="true">User sex (male or female)</param>
        /// <param name="birthday" optional="true">User birthday</param>
        /// <param name="worksfrom" optional="true">User registration date. If it is not specified, then the current date will be set</param>
        /// <param name="comment" optional="true">User comments</param>
        /// <param name="contacts">Contact list</param>
        /// <param name="files">Avatar photo URL</param>
        /// <param name="password">User password</param>
        /// <returns>Newly added user</returns>
        /// <visible>false</visible>
        [Create("active")]
        public EmployeeWraperFull AddMemberAsActivated(
            bool isVisitor,
            String email,
            String firstname,
            String lastname,
            Guid[] department,
            String title,
            String location,
            String sex,
            ApiDateTime birthday,
            ApiDateTime worksfrom,
            String comment,
            IEnumerable<Contact> contacts,
            String files,
            String password)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_AddRemoveUser);

            var user = new UserInfo();

            password = (password ?? "").Trim();

            if (String.IsNullOrEmpty(password))
            {
                password = UserManagerWrapper.GeneratePassword();
            }
            else
            {
                UserManagerWrapper.CheckPasswordPolicy(password);
            }

            var passwordHash = PasswordHasher.GetClientPassword(password);

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

            UpdateContacts(contacts, user);
            Cache.Insert("REWRITE_URL" + CoreContext.TenantManager.GetCurrentTenant().TenantId, HttpContext.Current.Request.GetUrlRewriter().ToString(), TimeSpan.FromMinutes(5));
            user = UserManagerWrapper.AddUser(user, passwordHash, false, false, isVisitor, false, true, true);

            user.ActivationStatus = EmployeeActivationStatus.Activated;

            UpdateDepartments(department, user);

            if (files != UserPhotoManager.GetDefaultPhotoAbsoluteWebPath())
            {
                UpdatePhotoUrl(files, user);
            }

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
        /// Updates the data for the selected portal user with the first name, last name, email address and/or optional parameters specified in the request.
        /// </summary>
        /// <short>
        /// Update a user
        /// </short>
        /// <category>Profiles</category>
        /// <param name="isVisitor">Specifies if this is a guest (true) or user (false)</param>
        /// <param name="userid">User ID</param>
        /// <param name="firstname">New user first name</param>
        /// <param name="lastname">New user last name</param>
        /// <param name="comment" optional="true">New user comments</param>
        /// <param name="department" optional="true">New user department</param>
        /// <param name="title" optional="true">New user title</param>
        /// <param name="location" optional="true">New user location</param>
        /// <param name="sex" optional="true">New user sex (male or female)</param>
        /// <param name="birthday" optional="true">New user birthday</param>
        /// <param name="worksfrom" optional="true">New user registration date. If it is not specified, then the current date will be set</param>
        /// <param name="contacts">New contact list</param>
        /// <param name="files">New avatar photo URL</param>
        /// <param name="disable">Disable user</param>
        /// <returns>Updated user</returns>
        [Update("{userid}")]
        public EmployeeWraperFull UpdateMember(bool isVisitor, string userid, string firstname, string lastname, string comment, Guid[] department, string title, string location, string sex, ApiDateTime birthday, ApiDateTime worksfrom, IEnumerable<Contact> contacts, string files, bool? disable)
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
        /// <param name="userid">User ID</param>
        /// <returns>Deleted user</returns>
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
        /// Updates the contact information of the user with the ID specified in the request merging new data with the present portal data.
        /// </summary>
        /// <short>
        /// Update user contacts
        /// </short>
        /// <category>Contacts</category>
        /// <param name="userid">User ID</param>
        /// <param name="contacts">List of new contacts</param>
        /// <returns>Updated user profile</returns>
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
        /// Sets the contacts of the user with the ID specified in the request replacing the present portal data with the new data.
        /// </summary>
        /// <short>
        /// Set user contacts
        /// </short>
        /// <category>Contacts</category>
        /// <param name="userid">User ID</param>
        /// <param name="contacts">List of new contacts</param>
        /// <returns>Updated user profile</returns>
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
        /// <param name="userid">User ID</param>
        /// <param name="contacts">List of contacts</param>
        /// <returns>Updated user profile</returns>
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
        /// <param name="userid">User ID</param>
        /// <returns>User photo</returns>
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
        /// <param name="userid">User ID</param>
        /// <param name="files">New avatar photo URL</param>
        /// <returns>Updated user photo</returns>
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
        /// <param name="userid">User ID</param>
        /// <returns>Deleted user photo</returns>
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
        /// <param name="userid">User ID</param>
        /// <param name="tmpFile">Path to the temporary file</param>
        /// <param name="x">Horizontal coordinate</param>
        /// <param name="y">Vertical coordinate</param>
        /// <param name="width">Thumbnail width</param>
        /// <param name="height">Thumbnail height</param>
        /// <returns>Thumbnail</returns>
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
        /// <param name="userid">User ID</param>
        /// <param name="email">New email</param>
        /// <returns>Detailed user information</returns>
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

                    StudioNotifyService.Instance.SendEmailActivationInstructions(user, email);
                    MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentActivationInstructions, messageTarget, messageDescription);
                }
            }

            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Reminds a password to the user using the email specified in the request.
        /// </summary>
        /// <short>
        /// Remind a user password
        /// </short>
        /// <category>Password</category>
        /// <param name="email">User email</param>     
        /// <returns>Email with the password</returns>
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
        /// <param name="userid">User ID</param>
        /// <param name="password">New password</param>
        /// <returns>Detailed user information</returns>
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
        /// <short>Set an activation status</short>
        /// <category>Activation status</category>
        /// <param name="userid">User ID</param>
        /// <param name="activationstatus">Required activation status</param>
        /// <returns>Detailed user information</returns>
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
        /// Set an activation status
        /// </short>
        /// <category>Activation status</category>
        /// <param name="userIds">List of user IDs</param>
        /// <param name="activationstatus">Required activation status</param>
        /// <returns>List of users</returns>
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
        /// <param name="type">New user type</param>
        /// <param name="userIds">List of user IDs</param>
        /// <returns>List of users</returns>
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
        /// <param name="status">New user status</param>
        /// <param name="userIds">List of user IDs</param>
        /// <returns>List of users</returns>
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
        /// <param name="userIds">List of user IDs</param>
        /// <returns>List of users</returns>
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
        /// <param name="userIds">List of user IDs</param>
        /// <returns>List of users</returns>
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
        /// Send the delete instructions
        /// </short>
        /// <category>Profiles</category>
        /// <returns>Information message</returns>
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
        /// Subscribes to or unsubscribes from the birthday of a user with the ID specified.
        /// </summary>
        /// <short>Birthday subscription</short>
        /// <param name="userid">User ID</param>
        /// <param name="onRemind">Defines if the user will be notified about other user's birthday or not</param>
        /// <returns>Bool value: true means that the user will get the notification</returns>
        /// <category>Birthday</category>
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
        /// <param name="serializedProfile">Third-party profile in the serialized format</param>
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
        /// <param name="provider">Provider name</param>
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
        /// Returns the progress of the started reassigning process for the user with the ID specified in the request.
        /// </summary>
        /// <short>Get the reassigning progress</short>
        /// <param name="userId">User ID whose data is reassigned</param>
        /// <category>Reassign user data</category>
        /// <returns>Reassigning progress</returns>
        [Read(@"reassign/progress")]
        public ReassignProgressItem GetReassignProgress(Guid userId)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            return QueueWorker.GetProgressItemStatus(TenantProvider.CurrentTenantID, userId, typeof(ReassignProgressItem)) as ReassignProgressItem;
        }

        /// <summary>
        /// Terminates the reassigning process for the user with the ID specified in the request.
        /// </summary>
        /// <short>Terminate the reassigning process</short>
        /// <param name="userId">User ID whose data is reassigned</param>
        /// <category>Reassign user data</category>
        [Update(@"reassign/terminate")]
        public void TerminateReassign(Guid userId)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            QueueWorker.Terminate(TenantProvider.CurrentTenantID, userId, typeof(ReassignProgressItem));
        }

        /// <summary>
        /// Starts the reassigning process for the user with the ID specified in the request.
        /// </summary>
        /// <short>Start the reassigning process</short>
        /// <param name="fromUserId">User ID whose data will be reassigned to another user</param>
        /// <param name="toUserId">User ID to whom all the data will be reassigned</param>
        /// <param name="deleteProfile">Deletes a profile when reassignment will be finished or not</param>
        /// <category>Reassign user data</category>
        /// <returns>Reassigning progress</returns>
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
        /// Returns the progress of the started removing process for the user with the ID specified in the request.
        /// </summary>
        /// <short>Get the removing progress</short>
        /// <param name="userId">User ID</param>
        /// <category>Remove user data</category>
        /// <returns>Removing progress</returns>
        [Read(@"remove/progress")]
        public RemoveProgressItem GetRemoveProgress(Guid userId)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            return QueueWorker.GetProgressItemStatus(TenantProvider.CurrentTenantID, userId, typeof(RemoveProgressItem)) as RemoveProgressItem;
        }

        /// <summary>
        /// Terminates the removing process for the user with the ID specified in the request.
        /// </summary>
        /// <short>Terminate the removing process</short>
        /// <param name="userId">User ID</param>
        /// <category>Remove user data</category>
        [Update(@"remove/terminate")]
        public void TerminateRemove(Guid userId)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            QueueWorker.Terminate(TenantProvider.CurrentTenantID, userId, typeof(RemoveProgressItem));
        }

        /// <summary>
        /// Starts the removing process for the user with the ID specified in the request.
        /// </summary>
        /// <short>Start the removing process</short>
        /// <param name="userId">User ID</param>
        /// <category>Remove user data</category>
        /// <returns>Removing progress</returns>
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