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


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security;
using System.Web;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Specific;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using log4net;
using ASC.Web.Core;
using SecurityContext = ASC.Core.SecurityContext;
using System.Net;
using Resources;
using ASC.FederatedLogin;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.FederatedLogin.Profile;

namespace ASC.Api.Employee
{
    ///<summary>
    ///User profiles access
    ///</summary>
    public class EmployeeApi : Interfaces.IApiEntryPoint
    {
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
        ///Returns the detailed information about the current user profile
        ///</summary>
        ///<short>
        ///My profile
        ///</short>
        ///<returns>Profile</returns>
        [Read("@self")]
        public EmployeeWraperFull GetMe()
        {
            return new EmployeeWraperFull(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID));
        }

        ///<summary>
        ///Returns the list of profiles for all portal users
        ///</summary>
        ///<short>
        ///All profiles
        ///</short>
        ///<returns>List of profiles</returns>
        /// <remarks>This method returns a partial profile. Use more specific method to get full profile</remarks>
        [Read("")]
        public IEnumerable<EmployeeWraperFull> GetAll()
        {
            return GetByStatus(EmployeeStatus.Active);
        }

        [Read("status/{status}")]
        public IEnumerable<EmployeeWraperFull> GetByStatus(EmployeeStatus status)
        {
            if (CoreContext.Configuration.Personal) throw new MethodAccessException("Method not available on personal.onlyoffice.com");
            var query = CoreContext.UserManager.GetUsers(status).AsEnumerable();
            if ("group".Equals(_context.FilterBy, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_context.FilterValue))
            {
                var groupId = new Guid(_context.FilterValue);
                //Filter by group
                query = query.Where(x => CoreContext.UserManager.IsUserInGroup(x.ID, groupId));
                _context.SetDataFiltered();
            }
            return query.Select(x => new EmployeeWraperFull(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the detailed information about the profile of the user with the name specified in the request
        ///</summary>
        ///<short>
        ///Specific profile
        ///</short>
        ///<param name="username">User name</param>
        ///<returns>User profile</returns>
        [Read("{username}")]
        public EmployeeWraperFull GetById(string username)
        {
            if (CoreContext.Configuration.Personal) throw new MethodAccessException("Method not available on personal.onlyoffice.com");
            var user = CoreContext.UserManager.GetUserByUserName(username);
            if (user.ID == Core.Users.Constants.LostUser.ID)
            {
                user = CoreContext.UserManager.GetUsers(new Guid(username));
            }

            if (user.ID == Core.Users.Constants.LostUser.ID)
            {
                throw new ItemNotFoundException("User not found");
            }

            return new EmployeeWraperFull(user);
        }

        ///<summary>
        ///Returns the detailed information about the profile of the user with the name specified in the request
        ///</summary>
        ///<short>
        ///Specific profile
        ///</short>
        ///<param name="email">User email</param>
        ///<returns>User profile</returns>
        [Read("email")]
        public EmployeeWraperFull GetByEmail(string email)
        {
            if (CoreContext.Configuration.Personal && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsOwner())
                throw new MethodAccessException("Method not available on personal.onlyoffice.com");
            var user = CoreContext.UserManager.GetUserByEmail(email);
            if (user.ID == Core.Users.Constants.LostUser.ID)
            {
                throw new ItemNotFoundException("User not found");
            }

            return new EmployeeWraperFull(user);
        }
        ///<summary>
        ///Returns the list of profiles for all portal users matching the search query
        ///</summary>
        ///<short>
        ///Search users
        ///</short>
        ///<param name="query">Query</param>
        ///<returns>List of users</returns>
        [Read("@search/{query}")]
        public IEnumerable<EmployeeWraperFull> GetSearch(string query)
        {
            if (CoreContext.Configuration.Personal) throw new MethodAccessException("Method not available on personal.onlyoffice.com");
            try
            {
                var groupId = Guid.Empty;
                if ("group".Equals(_context.FilterBy, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_context.FilterValue))
                {
                    groupId = new Guid(_context.FilterValue);
                }

                return CoreContext.UserManager.Search(query, EmployeeStatus.Active, groupId)
                                  .Select(x => new EmployeeWraperFull(x))
                                  .AsEnumerable()
                                  .ToSmartList();
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Web").Error(error);
            }
            return null;
        }

        ///<summary>
        ///Returns the list of users matching the search criteria
        ///</summary>
        ///<short>
        ///User search
        ///</short>
        ///<param name="query">Search text</param>
        ///<returns>User list</returns>
        [Read("search")]
        public IEnumerable<EmployeeWraperFull> GetPeopleSearch(string query)
        {
            return GetSearch(query);
        }

        ///<summary>
        ///Returns the list of users matching the status filter and text search
        ///</summary>
        ///<short>
        ///User search by filter
        ///</short>
        ///<param name="status">User status</param>
        ///<param name="query">Search text</param>
        ///<returns>User list</returns>
        [Read("status/{status}/search")]
        public IEnumerable<EmployeeWraperFull> GetAdvanced(EmployeeStatus status, string query)
        {
            if (CoreContext.Configuration.Personal) throw new MethodAccessException("Method not available on personal.onlyoffice.com");
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

                var usr = list.Select(x => new EmployeeWraperFull(x)).AsEnumerable();

                return usr.ToSmartList();
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Web").Error(error);
            }
            return null;
        }

        /// <summary>
        ///    Returns the list of users matching the filter with the parameters specified in the request
        /// </summary>
        /// <short>
        ///    User search by extended filter
        /// </short>
        /// <param optional="true" name="employeeStatus">User status</param>
        /// <param optional="true" name="groupId">Group ID</param>
        /// <param optional="true" name="activationStatus">Activation status</param>
        /// <param optional="true" name="employeeType">User type</param>
        ///  <param optional="true" name="isAdministrator">Administrator(bool type)</param>
        /// <returns>
        ///    User list
        /// </returns>
        [Read("filter")]
        public IEnumerable<EmployeeWraperFull> GetByFilter(EmployeeStatus? employeeStatus, Guid? groupId, EmployeeActivationStatus? activationStatus, EmployeeType? employeeType, bool? isAdministrator)
        {
            if (CoreContext.Configuration.Personal) throw new MethodAccessException("Method not available on personal.onlyoffice.com");
            var isAdmin = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();
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
                            users.Where(x => x.ActivationStatus == EmployeeActivationStatus.Activated) :
                            users.Where(x => x.ActivationStatus == EmployeeActivationStatus.NotActivated ||
                                             x.ActivationStatus == EmployeeActivationStatus.Pending);
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

            users = _context.SortDescending ? users.OrderByDescending(r => r.DisplayUserName()) : users.OrderBy(r => r.DisplayUserName());
            users = users.Skip((int)_context.StartIndex).Take((int)_context.Count - 1);

            _context.SetDataSorted();
            _context.SetDataPaginated();

            return users.Select(x => new EmployeeWraperFull(x, _context)).ToSmartList();
        }

        /// <summary>
        /// Adds a new portal user with the first and last name, email address and several optional parameters specified in the request
        /// </summary>
        /// <short>
        /// Add new user
        /// </short>
        /// <param name="isVisitor">User or Visitor (bool type: false|true)</param>
        /// <param name="email">Email</param>
        /// <param name="firstname">First name</param>
        /// <param name="lastname">Last name</param>
        /// <param name="department" optional="true">Department</param>
        /// <param name="title" optional="true">Title</param>
        /// <param name="location" optional="true">Location</param>
        /// <param name="sex" optional="true">Sex (male|female)</param>
        /// <param name="birthday" optional="true">Birthday</param>
        /// <param name="worksfrom" optional="true">Works from date. If not specified - current will be set</param>
        /// <param name="comment" optional="true">Comment for user</param>
        /// <param name="contacts">List of contacts</param>
        /// <param name="files">Avatar photo url</param>
        /// <returns>Newly created user</returns>
        [Create("")]
        public EmployeeWraperFull AddMember(bool isVisitor, string email, string firstname, string lastname, Guid[] department, string title, string location, string sex, ApiDateTime birthday, ApiDateTime worksfrom, string comment, IEnumerable<Contact> contacts, string files)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_AddRemoveUser);
            var password = UserManagerWrapper.GeneratePassword();
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

            user = UserManagerWrapper.AddUser(user, password, false, true, isVisitor);

            var messageAction = isVisitor ? MessageAction.GuestCreated : MessageAction.UserCreated;
            MessageService.Send(Request, messageAction, user.DisplayUserName(false));

            UpdateDepartments(department, user);

            if (files != UserPhotoManager.GetDefaultPhotoAbsoluteWebPath())
            {
                UpdatePhotoUrl(files, user);
            }

            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Adds a new portal user with the first and last name, email address and several optional parameters specified in the request
        /// </summary>
        /// <short>
        /// Add new user
        /// </short>
        /// <param name="isVisitor">User or Visitor (bool type: false|true)</param>
        /// <param name="email">Email</param>
        /// <param name="firstname">First name</param>
        /// <param name="lastname">Last name</param>
        /// <param name="department" optional="true">Department</param>
        /// <param name="title" optional="true">Title</param>
        /// <param name="location" optional="true">Location</param>
        /// <param name="sex" optional="true">Sex (male|female)</param>
        /// <param name="birthday" optional="true">Birthday</param>
        /// <param name="worksfrom" optional="true">Works from date. If not specified - current will be set</param>
        /// <param name="comment" optional="true">Comment for user</param>
        /// <param name="contacts">List of contacts</param>
        /// <param name="files">Avatar photo url</param>
        /// <param name="password">User Password</param>
        /// <returns>Newly created user</returns>
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

            if (String.IsNullOrEmpty(password))
                password = UserManagerWrapper.GeneratePassword();

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

            user = UserManagerWrapper.AddUser(user, password, false, false, isVisitor);

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
        /// Updates the data for the selected portal user with the first and last name, email address and/or optional parameters specified in the request
        /// </summary>
        /// <short>
        /// Update user
        /// </short>
        /// <param name="isVisitor">User or Visitor (bool type: false|true)</param>
        /// <param name="userid">User ID to update</param>
        /// <param name="email">Email</param>
        /// <param name="firstname">First name</param>
        /// <param name="lastname">Last name</param>
        /// <param name="comment" optional="true">Comment for user</param>
        /// <param name="department" optional="true">Department</param>
        /// <param name="title" optional="true">Title</param>
        /// <param name="location" optional="true">Location</param>
        /// <param name="sex" optional="true">Sex (male|female)</param>
        /// <param name="birthday" optional="true">Birthday</param>
        /// <param name="worksfrom" optional="true">Works from date. If not specified - current will be set</param>
        /// <param name="contacts">List fo contacts</param>
        /// <param name="files">Avatar photo url</param>
        /// <param name="disable"></param>
        /// <returns>Newly created user</returns>
        [Update("{userid}")]
        public EmployeeWraperFull UpdateMember(bool isVisitor, string userid, string email, string firstname, string lastname, string comment, Guid[] department, string title, string location, string sex, ApiDateTime birthday, ApiDateTime worksfrom, IEnumerable<Contact> contacts, string files, bool? disable)
        {
            SecurityContext.DemandPermissions(new UserSecurityProvider(new Guid(userid)), Core.Users.Constants.Action_EditUser);

            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            var self = SecurityContext.CurrentAccount.ID.Equals(user.ID);
            var resetDate = new DateTime(1900, 01, 01);

            //Update it

            //Validate email
            if (!string.IsNullOrEmpty(email))
            {
                var address = new MailAddress(email);
                user.Email = address.Address;
            }

            //Set common fields
            user.FirstName = firstname ?? user.FirstName;
            user.LastName = lastname ?? user.LastName;
            user.Title = title ?? user.Title;
            user.Location = location ?? user.Location;
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

            if (self && !CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Core.Users.Constants.GroupAdmin.ID))
            {
                StudioNotifyService.Instance.SendMsgToAdminAboutProfileUpdated();
            }

            // change user type
            var canBeGuestFlag = !user.IsOwner() && !user.IsAdmin() && !user.IsMe();

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

            CoreContext.UserManager.SaveUserInfo(user, isVisitor);
            MessageService.Send(Request, MessageAction.UserUpdated, user.DisplayUserName(false));

            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Deletes the user with the ID specified in the request from the portal
        /// </summary>
        /// <short>
        /// Delete user
        /// </short>
        /// <param name="userid">ID of user to delete</param>
        /// <returns></returns>
        [Delete("{userid}")]
        public EmployeeWraperFull DeleteMember(string userid)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_AddRemoveUser);

            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            if (user.Status != EmployeeStatus.Terminated)
                throw new Exception("The user is not suspended");

            var userName = user.DisplayUserName(false);

            UserPhotoManager.RemovePhoto(Guid.Empty, user.ID);
            CoreContext.UserManager.DeleteUser(user.ID);

            MessageService.Send(Request, MessageAction.UserDeleted, userName);

            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Updates the specified user contact information merging the sent data with the present on the portal
        /// </summary>
        /// <short>
        /// Update user contacts
        /// </short>
        /// <param name="userid">User ID</param>
        /// <param name="contacts">Contacts list</param>
        /// <returns>Updated user profile</returns>
        [Update("{userid}/contacts")]
        public EmployeeWraperFull UpdateMemberContacts(string userid, IEnumerable<Contact> contacts)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            UpdateContacts(contacts, user);
            CoreContext.UserManager.SaveUserInfo(user);
            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Updates the specified user contact information changing the data present on the portal for the sent data
        /// </summary>
        /// <short>
        /// Set user contacts
        /// </short>
        /// <param name="userid">User ID</param>
        /// <param name="contacts">Contacts list</param>
        /// <returns>Updated user profile</returns>
        [Create("{userid}/contacts")]
        public EmployeeWraperFull SetMemberContacts(string userid, IEnumerable<Contact> contacts)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            user.Contacts.Clear();
            UpdateContacts(contacts, user);
            CoreContext.UserManager.SaveUserInfo(user);
            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Updates the specified user contact information deleting the data specified in the request from the portal
        /// </summary>
        /// <short>
        /// Delete user contacts
        /// </short>
        /// <param name="userid">User ID</param>
        /// <param name="contacts">Contacts list</param>
        /// <returns>Updated user profile</returns>
        [Delete("{userid}/contacts")]
        public EmployeeWraperFull DeleteMemberContacts(string userid, IEnumerable<Contact> contacts)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            DeleteContacts(contacts, user);
            CoreContext.UserManager.SaveUserInfo(user);
            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Updates the specified user photo with the pathname
        /// </summary>
        /// <short>
        /// Update user photo
        /// </short>
        /// <param name="userid">User ID</param>
        /// <param name="files">Avatar photo url</param>
        /// <returns></returns>
        [Update("{userid}/photo")]
        public EmployeeWraperFull UpdateMemberPhoto(string userid, string files)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            if (files != UserPhotoManager.GetPhotoAbsoluteWebPath(user.ID))
            {
                UpdatePhotoUrl(files, user);
            }

            CoreContext.UserManager.SaveUserInfo(user);
            MessageService.Send(Request, MessageAction.UserAddedAvatar, user.DisplayUserName(false));

            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Deletes the photo of the user with the ID specified in the request
        /// </summary>
        /// <short>
        /// Delete user photo
        /// </short>
        /// <param name="userid">User ID</param>
        /// <returns></returns>
        [Delete("{userid}/photo")]
        public EmployeeWraperFull DeleteMemberPhoto(string userid)
        {
            var user = GetUserInfo(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            SecurityContext.DemandPermissions(new UserSecurityProvider(user.ID), Core.Users.Constants.Action_EditUser);

            UserPhotoManager.RemovePhoto(Guid.Empty, user.ID);

            CoreContext.UserManager.SaveUserInfo(user);
            MessageService.Send(Request, MessageAction.UserDeletedAvatar, user.DisplayUserName(false));

            return new EmployeeWraperFull(user);
        }

        /// <summary>
        /// Remind password for the user with email specified in the request
        /// </summary>
        /// <short>
        /// Remind user password
        /// </short>
        /// <param name="email">User email</param>     
        /// <returns></returns>
        [Read("{email}/password")]
        public void SendUserPassword(string email)
        {
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException("email");

            var userInfo = CoreContext.UserManager.GetUserByEmail(email);
            if (!CoreContext.UserManager.UserExists(userInfo.ID) || string.IsNullOrEmpty(userInfo.Email))
            {
                throw new Exception("The user email could not be found");
            }
            if (userInfo.Status == EmployeeStatus.Terminated)
            {
                throw new Exception("Your profile is suspended");
            }
            StudioNotifyService.Instance.UserPasswordChange(userInfo);
        }

        /// <summary>
        ///   Sets the password and email for the user with the ID specified in the request
        /// </summary>
        /// <param name="userid">User ID</param>
        /// <param name="password">Password</param>
        /// <param name="email">New email</param>
        /// <returns>Detailed user information</returns>
        /// <visible>false</visible>
        [Update("{userid}/password")]
        public EmployeeWraperFull ChangeUserPassword(Guid userid, String password, String email)
        {
            SecurityContext.DemandPermissions(new UserSecurityProvider(userid), Core.Users.Constants.Action_EditUser);

            if (!CoreContext.UserManager.UserExists(userid)) return null;

            var user = CoreContext.UserManager.GetUsers(userid);

            if (CoreContext.UserManager.IsSystemUser(user.ID))
                throw new SecurityException();

            if (!string.IsNullOrEmpty(email))
            {
                var address = new MailAddress(email);
                if (!string.Equals(address.Address, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = address.Address.ToLowerInvariant();
                    user.ActivationStatus = EmployeeActivationStatus.Activated;
                    CoreContext.UserManager.SaveUserInfo(user);
                }
            }

            if (!string.IsNullOrEmpty(password))
            {
                SecurityContext.SetUserPassword(userid, password);
            }

            return new EmployeeWraperFull(GetUserInfo(userid.ToString()));
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
        ///   Sets the required activation status to the list of users with the ID specified in the request
        /// </summary>
        /// <summary>
        ///   Set activation status
        /// </summary>
        /// <param name="userIds">User list ID</param>
        /// <param name="activationstatus">Required status</param>
        /// <returns>List of users</returns>
        /// <visible>false</visible>
        [Update("activationstatus/{activationstatus}")]
        public IEnumerable<EmployeeWraperFull> UpdateEmployeeActivationStatus(EmployeeActivationStatus activationstatus, IEnumerable<Guid> userIds)
        {
            var retuls = new List<EmployeeWraperFull>();
            foreach (var id in userIds.Where(userId => !CoreContext.UserManager.IsSystemUser(userId)))
            {
                SecurityContext.DemandPermissions(new UserSecurityProvider(id), Core.Users.Constants.Action_EditUser);
                var u = CoreContext.UserManager.GetUsers(id);
                if (u.ID == Core.Users.Constants.LostUser.ID) continue;

                u.ActivationStatus = activationstatus;
                CoreContext.UserManager.SaveUserInfo(u);
                retuls.Add(new EmployeeWraperFull(u));
            }

            return retuls;
        }

        /// <summary>
        /// Changes the type between user and guest for the user with the ID specified in the request
        /// </summary>
        /// <short>
        /// User type change
        /// </short>
        /// <param name="type">New user type</param>
        /// <param name="userIds">User ID list</param>
        /// <returns>User list</returns>
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
                        CoreContext.UserManager.AddUserIntoGroup(user.ID, Core.Users.Constants.GroupVisitor.ID);
                        WebItemSecurity.ClearCache();
                        break;
                }
            }

            MessageService.Send(Request, MessageAction.UsersUpdatedType, users.Select(x => x.DisplayUserName(false)));

            return users.Select(user => new EmployeeWraperFull(user)).ToSmartList();
        }

        /// <summary>
        /// Changes the status between active and disabled for the user with the ID specified in the request
        /// </summary>
        /// <short>
        /// User status change
        /// </short>
        /// <param name="status">New user status</param>
        /// <param name="userIds">User ID list</param>
        /// <returns>User list</returns>
        [Update("status/{status}")]
        public IEnumerable<EmployeeWraperFull> UpdateUserStatus(EmployeeStatus status, IEnumerable<Guid> userIds)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditUser);

            var users = userIds
                .Where(userId => !CoreContext.UserManager.IsSystemUser(userId))
                .Select(userId => CoreContext.UserManager.GetUsers(userId))
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
                                CoreContext.UserManager.SaveUserInfo(user);
                            }
                        }
                        break;
                    case EmployeeStatus.Terminated:
                        user.Status = EmployeeStatus.Terminated;
                        CoreContext.UserManager.SaveUserInfo(user);
                        break;
                }
            }

            MessageService.Send(Request, MessageAction.UsersUpdatedStatus, users.Select(x => x.DisplayUserName(false)));

            return users.Select(user => new EmployeeWraperFull(user)).ToSmartList();
        }

        /// <summary>
        /// Sends emails once again for the users who have not activated their emails
        /// </summary>
        /// <short>
        /// Send activation email
        /// </short>
        /// <param name="userIds">User ID list</param>
        /// <returns>User list</returns>
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

            MessageService.Send(Request, MessageAction.UsersSentActivationInstructions, users.Select(x => x.DisplayUserName(false)));

            return users.Select(user => new EmployeeWraperFull(user)).ToSmartList();
        }

        /// <summary>
        /// Delete the list of selected users
        /// </summary>
        /// <short>
        /// Delete users
        /// </short>
        /// <param name="userIds">User ID list</param>
        /// <returns>User list</returns>
        [Update("delete")]
        public IEnumerable<EmployeeWraperFull> RemoveUsers(IEnumerable<Guid> userIds)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_AddRemoveUser);

            var users = userIds
                .Where(userId => !CoreContext.UserManager.IsSystemUser(userId))
                .Select(userId => CoreContext.UserManager.GetUsers(userId))
                .ToList();

            var userNames = users.Select(x => x.DisplayUserName(false)).ToList();

            foreach (var user in users)
            {
                if (user.Status != EmployeeStatus.Terminated) continue;

                UserPhotoManager.RemovePhoto(Guid.Empty, user.ID);
                CoreContext.UserManager.DeleteUser(user.ID);
            }

            MessageService.Send(Request, MessageAction.UsersDeleted, userNames);

            return users.Select(user => new EmployeeWraperFull(user)).ToSmartList();
        }


        /// <summary>
        /// Send instructions for delete user own profile
        /// </summary>
        /// <short>
        /// Send delete instructions
        /// </short>
        /// <returns>Info message</returns>
        [Update("self/delete")]
        public string SendInstructionsToDelete()
        {
            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            StudioNotifyService.Instance.SendMsgProfileDeletion(user.Email);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentDeleteInstructions);

            return String.Format(Resource.SuccessfullySentNotificationDeleteUserInfoMessage, "<b>" + user.Email + "</b>");
        }

        /// <summary>
        /// Join to affiliate programm
        /// </summary>
        /// <short>
        /// Join to affiliate programm
        /// </short>
        /// <returns>Link to affiliate programm</returns>
        [Update("self/joinaffiliate")]
        public string JoinToAffiliateProgram()
        {
            return AffiliateHelper.Join();
        }

        #region Auth page hidden methods
 
        ///<visible>false</visible>
        [Update("remindpwd", RequiresAuthorization = false)]
        public string RemindPwd(string email)
        {
            if (!email.TestEmailRegex())
            {
                throw new Exception("<div>" + Resource.ErrorNotCorrectEmail + "</div>");
            }

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            if (tenant != null)
            {
                var settings = SettingsManager.Instance.LoadSettings<IPRestrictionsSettings>(tenant.TenantId);
                if (settings.Enable && !IPSecurity.IPSecurity.Verify(tenant))
                {
                    throw new Exception("<div>" + Resource.ErrorAccessRestricted + "</div>");
                }
            }


            UserManagerWrapper.SendUserPassword(email);


            var userInfo = CoreContext.UserManager.GetUserByEmail(email);
            if (userInfo.Sid != null)
            {
                throw new Exception("<div>" + Resource.CouldNotRecoverPasswordForLdapUser + "</div>");
            }

            string displayUserName = userInfo.DisplayUserName(false);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentPasswordChangeInstructions, displayUserName);


            return String.Format(Resource.MessageYourPasswordSuccessfullySendedToEmail, "<b>" + email + "</b>");
        }


        ///<visible>false</visible>
        [Update("thirdparty/linkaccount")]
        public void LinkAccount(string serializedProfile)
        {
            var profile = new LoginProfile(serializedProfile);

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

    }
}