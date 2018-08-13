/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Impl;
using ASC.Core;
using ASC.Core.Users;
using ASC.Specific;
using ASC.Web.Core.Users;

namespace ASC.Api.Employee
{
    [DataContract(Name = "person", Namespace = "")]
    public class EmployeeWraperFull : EmployeeWraper
    {
        [DataMember(Order = 10)]
        public string FirstName { get; set; }

        [DataMember(Order = 10)]
        public string LastName { get; set; }

        [DataMember(Order = 2)]
        public string UserName { get; set; }

        [DataMember(Order = 10)]
        public string Email { get; set; }

        [DataMember(Order = 12, EmitDefaultValue = false)]
        protected List<Contact> Contacts { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public ApiDateTime Birthday { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Sex { get; set; }

        [DataMember(Order = 10)]
        public EmployeeStatus Status { get; set; }

        [DataMember(Order = 10)]
        public EmployeeActivationStatus ActivationStatus { get; set; }

        [DataMember(Order = 10)]
        public ApiDateTime Terminated { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Department { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public ApiDateTime WorkFrom { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public List<GroupWrapperSummary> Groups { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Location { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Notes { get; set; }

        [DataMember(Order = 20)]
        public string AvatarMedium { get; set; }

        [DataMember(Order = 20)]
        public string Avatar { get; set; }

        [DataMember(Order = 20)]
        public bool IsOnline { get; set; }

        [DataMember(Order = 20)]
        public bool IsAdmin { get; set; }

        [DataMember(Order = 20)]
        public bool IsLDAP { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public List<string> ListAdminModules { get; set; }

        [DataMember(Order = 20)]
        public bool IsOwner { get; set; }

        [DataMember(Order = 2)]
        public bool IsVisitor { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string CultureName { get; set; }


        [DataMember(Order = 11, EmitDefaultValue = false)]
        protected String MobilePhone { get; set; }

        [DataMember(Order = 11, EmitDefaultValue = false)]
        protected MobilePhoneActivationStatus MobilePhoneActivationStatus { get; set; }

        [DataMember(Order = 20)]
        public bool IsSSO { get; set; }

        public EmployeeWraperFull()
        {
        }

        public EmployeeWraperFull(UserInfo userInfo)
            : this(userInfo, null)
        {
        }

        public EmployeeWraperFull(UserInfo userInfo, ApiContext context)
            : base(userInfo)
        {
            UserName = userInfo.UserName;
            IsVisitor = userInfo.IsVisitor();
            FirstName = userInfo.FirstName;
            LastName = userInfo.LastName;
            Birthday = (ApiDateTime)userInfo.BirthDate;

            if (userInfo.Sex.HasValue)
                Sex = userInfo.Sex.Value ? "male" : "female";

            Status = userInfo.Status;
            ActivationStatus = userInfo.ActivationStatus;
            Terminated = (ApiDateTime)userInfo.TerminatedDate;

            WorkFrom = (ApiDateTime)userInfo.WorkFromDate;
            Email = userInfo.Email;

            if (!string.IsNullOrEmpty(userInfo.Location))
            {
                Location = userInfo.Location;
            }

            if (!string.IsNullOrEmpty(userInfo.Notes))
            {
                Notes = userInfo.Notes;
            }

            if (!string.IsNullOrEmpty(userInfo.MobilePhone))
            {
                MobilePhone = userInfo.MobilePhone;
            }

            MobilePhoneActivationStatus = userInfo.MobilePhoneActivationStatus;

            if (!string.IsNullOrEmpty(userInfo.CultureName))
            {
                CultureName = userInfo.CultureName;
            }

            FillConacts(userInfo);

            var groups = CoreContext.UserManager.GetUserGroups(userInfo.ID).Select(x => new GroupWrapperSummary(x)).ToList();

            if (groups.Any())
            {
                Groups = groups;
                Department = string.Join(", ", Groups.Select(d => d.Name.HtmlEncode()));
            }
            else
            {
                Department = "";
            }

            if (CheckContext(context, "avatarSmall"))
            {
                AvatarSmall = UserPhotoManager.GetSmallPhotoURL(userInfo.ID);
            }

            if (CheckContext(context, "avatarMedium"))
                AvatarMedium = UserPhotoManager.GetMediumPhotoURL(userInfo.ID);

            if (CheckContext(context, "avatar"))
            {
                Avatar = UserPhotoManager.GetBigPhotoURL(userInfo.ID);
            }

            IsOnline = false;
            IsAdmin = userInfo.IsAdmin();

            if (CheckContext(context, "listAdminModules"))
            {
                var listAdminModules = userInfo.GetListAdminModules();

                if (listAdminModules.Any())
                    ListAdminModules = listAdminModules;
            }

            IsOwner = userInfo.IsOwner();

            IsLDAP = userInfo.IsLDAP();
            IsSSO = userInfo.IsSSO();
        }

        private void FillConacts(UserInfo userInfo)
        {
            var contacts = new List<Contact>();
            for (var i = 0; i < userInfo.Contacts.Count; i += 2)
            {
                if (i + 1 < userInfo.Contacts.Count)
                {
                    contacts.Add(new Contact(userInfo.Contacts[i], userInfo.Contacts[i + 1]));
                }
            }

            if (contacts.Any())
            {
                Contacts = contacts;
            }
        }

        private static bool CheckContext(ApiContext context, string field)
        {
            return context == null || context.Fields == null ||
                   (context.Fields != null && context.Fields.Contains(field));
        }

        public static EmployeeWraperFull GetFull(Guid userId)
        {
            try
            {
                return GetFull(CoreContext.UserManager.GetUsers(userId));

            }
            catch (Exception)
            {
                return GetFull(Core.Users.Constants.LostUser);
            }
        }

        public static EmployeeWraperFull GetFull(UserInfo userInfo)
        {
            return new EmployeeWraperFull(userInfo);
        }

        public new static EmployeeWraperFull GetSample()
        {
            return new EmployeeWraperFull
                {
                    Avatar = "url to big avatar",
                    AvatarSmall = "url to small avatar",
                    Contacts = new List<Contact> { Contact.GetSample() },
                    Email = "my@gmail.com",
                    FirstName = "Mike",
                    Id = Guid.Empty,
                    IsAdmin = false,
                    ListAdminModules = new List<string> {"projects", "crm"},
                    UserName = "Mike.Zanyatski",
                    LastName = "Zanyatski",
                    Title = "Manager",
                    Groups = new List<GroupWrapperSummary> { GroupWrapperSummary.GetSample() },
                    AvatarMedium = "url to medium avatar",
                    Birthday = ApiDateTime.GetSample(),
                    Department = "Marketing",
                    Location = "Palo Alto",
                    Notes = "Notes to worker",
                    Sex = "male",
                    Status = EmployeeStatus.Active,
                    WorkFrom = ApiDateTime.GetSample(),
                    Terminated = ApiDateTime.GetSample(),
                    CultureName = "en-EN",
                    IsLDAP = false,
                    IsSSO = false
                };
        }
    }
}