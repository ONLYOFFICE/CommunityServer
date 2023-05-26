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
using System.Linq;
using System.Runtime.Serialization;

using ASC.Api.Impl;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Specific;
using ASC.Web.Core;
using ASC.Web.Core.Users;

namespace ASC.Api.Employee
{
    ///<inherited>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</inherited>
    [DataContract(Name = "person", Namespace = "")]
    public class EmployeeWraperFull : EmployeeWraper
    {
        ///<example>Mike</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public string FirstName { get; set; }

        ///<example>Zanyatski</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public string LastName { get; set; }

        ///<example>Mike.Zanyatski</example>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public string UserName { get; set; }

        ///<example>my@domain.com</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public string Email { get; set; }

        ///<type>ASC.Api.Employee.Contact, ASC.Api.Employee</type>
        ///<order>12</order>
        [DataMember(Order = 12, EmitDefaultValue = false)]
        protected List<Contact> Contacts { get; set; }

        ///<example>2008-04-10T06-30-00.000Z</example>
        ///<order>10</order>
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public ApiDateTime Birthday { get; set; }

        ///<example>male</example>
        ///<order>10</order>
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Sex { get; set; }

        ///<example type="int">1</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public EmployeeStatus Status { get; set; }

        ///<example type="int">0</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public EmployeeActivationStatus ActivationStatus { get; set; }

        ///<example>2008-04-10T06-30-00.000Z</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public ApiDateTime Terminated { get; set; }

        ///<example>Marketing</example>
        ///<order>10</order>
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Department { get; set; }

        ///<example>2008-04-10T06-30-00.000Z</example>
        ///<order>10</order>
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public ApiDateTime WorkFrom { get; set; }

        ///<type>ASC.Api.Employee.GroupWrapperSummary, ASC.Api.Employee</type>
        ///<order>20</order>
        [DataMember(Order = 20, EmitDefaultValue = false)]
        public List<GroupWrapperSummary> Groups { get; set; }

        ///<example>Palo Alto</example>
        ///<order>10</order>
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Location { get; set; }

        ///<example>Notes to worker</example>
        ///<order>10</order>
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Notes { get; set; }

        ///<example>055312F1-1D71-4786-BB5B-D5910316E53C</example>
        ///<order>20</order>
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public Guid Lead { get; set; }

        ///<example>url to medium avatar</example>
        ///<order>20</order>
        [DataMember(Order = 20)]
        public string AvatarMedium { get; set; }

        ///<example>url to big avatar</example>
        ///<order>20</order>
        [DataMember(Order = 20)]
        public string Avatar { get; set; }

        ///<example>false</example>
        ///<order>20</order>
        [DataMember(Order = 20)]
        public bool IsAdmin { get; set; }

        ///<example>false</example>
        ///<order>20</order>
        [DataMember(Order = 20)]
        public bool IsLDAP { get; set; }

        ///<example>projects,crm</example>
        ///<order>20</order>
        ///<collection split=",">list</collection>
        [DataMember(Order = 20, EmitDefaultValue = false)]
        public List<string> ListAdminModules { get; set; }

        ///<example>false</example>
        ///<order>20</order>
        [DataMember(Order = 20)]
        public bool IsOwner { get; set; }

        ///<example>false</example>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public bool IsVisitor { get; set; }

        ///<example>en-EN</example>
        ///<order>20</order>
        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string CultureName { get; set; }

        ///<example>MobilePhone</example>
        ///<order>11</order>
        [DataMember(Order = 11, EmitDefaultValue = false)]
        protected String MobilePhone { get; set; }

        ///<example>1</example>
        ///<order>11</order>
        [DataMember(Order = 11, EmitDefaultValue = false)]
        protected MobilePhoneActivationStatus MobilePhoneActivationStatus { get; set; }

        ///<example>false</example>
        ///<order>20</order>
        [DataMember(Order = 20)]
        public bool IsSSO { get; set; }

        [DataMember(Order = 21)]
        public long QuotaLimit { get; set; }

        [DataMember(Order = 22)]
        public long UsedSpace { get; set; }

        [DataMember(Order = 22)]
        public long DocsSpace { get; set; }

        [DataMember(Order = 22)]
        public long MailSpace { get; set; }

        [DataMember(Order = 22)]
        public long TalkSpace { get; set; }
        
        public EmployeeWraperFull()
        {
        }

        public EmployeeWraperFull(UserInfo userInfo)
            : this(userInfo, null)
        {
        }

        public EmployeeWraperFull(UserInfo userInfo, ApiContext context)
            : base(userInfo, context)
        {
            UserName = userInfo.UserName;
            IsVisitor = userInfo.IsVisitor();
            FirstName = userInfo.FirstName;
            LastName = userInfo.LastName;
            Birthday = (ApiDateTime)userInfo.BirthDate;

            if (userInfo.Sex.HasValue)
                Sex = userInfo.Sex.Value ? "male" : "female";

            Status = userInfo.Status;
            ActivationStatus = userInfo.ActivationStatus & ~EmployeeActivationStatus.AutoGenerated;
            Terminated = (ApiDateTime)userInfo.TerminatedDate;

            WorkFrom = (ApiDateTime)userInfo.WorkFromDate;

            if (!string.IsNullOrEmpty(userInfo.Location))
            {
                Location = userInfo.Location;
            }

            if (!string.IsNullOrEmpty(userInfo.Notes))
            {
                Notes = userInfo.Notes;
            }

            if (userInfo.Lead.HasValue)
            {
                Lead = userInfo.Lead.Value;
            }

            MobilePhoneActivationStatus = userInfo.MobilePhoneActivationStatus;

            if (!string.IsNullOrEmpty(userInfo.CultureName))
            {
                CultureName = userInfo.CultureName;
            }

            if (userInfo.CanViewPrivateData())
            {
                Email = userInfo.Email;

                if (!string.IsNullOrEmpty(userInfo.MobilePhone))
                {
                    MobilePhone = userInfo.MobilePhone;
                }

                FillConacts(userInfo);
            }

            if (CheckContext(context, "groups") || CheckContext(context, "department"))
            {
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
            }

            if (CheckContext(context, "avatarMedium"))
            {
                var mediumPhotUrl = UserPhotoManager.GetMediumPhotoURL(userInfo.ID);

                AvatarMedium = GetParametrizedPhotoUrl(mediumPhotUrl, userInfo);
            }

            if (CheckContext(context, "avatar"))
            {
                var bigPhotUrl = UserPhotoManager.GetBigPhotoURL(userInfo.ID);

                Avatar = GetParametrizedPhotoUrl(bigPhotUrl, userInfo);
            }

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

            IEnumerable<TenantQuotaRow> userQuotaRows;
            var useCache = !CheckContext(context, "doNotUseCache");
            if (context != null && CheckContext(context, "usedSpace"))
            {
                userQuotaRows = CoreContext.TenantManager.FindUserQuotaRows(CoreContext.TenantManager.GetCurrentTenant().TenantId, userInfo.ID, useCache).Where(r => !string.IsNullOrEmpty(r.Tag)).Where(r => r.Tag != Guid.Empty.ToString());
                UsedSpace = Math.Max(0, userQuotaRows.Sum(r => r.Counter));

                if (userQuotaRows != null)
                {
                    var MAIL_QUOTA_TAG = "666ceac1-4532-4f8c-9cba-8f510eca2fd1";
                    foreach (var userQuotaRow in userQuotaRows)
                    {
                        if (userQuotaRow.Tag == WebItemManager.DocumentsProductID.ToString())
                        {
                            DocsSpace = userQuotaRow.Counter;
                        }

                        if (userQuotaRow.Tag == MAIL_QUOTA_TAG)
                        {
                            MailSpace = userQuotaRow.Counter;
                        }

                        if (userQuotaRow.Tag == WebItemManager.TalkProductID.ToString())
                        {
                            TalkSpace= userQuotaRow.Counter;
                        }
                    }
                } 
            }
            
            if (context != null && CheckContext(context, "quotaLimit"))
            {
                var quotaSettings = TenantUserQuotaSettings.Load();
                if (quotaSettings.EnableUserQuota)
                {
                    var userQuotaSettings = UserQuotaSettings.LoadForUser(userInfo.ID);
                    QuotaLimit = userQuotaSettings != null ? userQuotaSettings.UserQuota : quotaSettings.DefaultUserQuota;
                }
                else
                {
                    QuotaLimit = -1;
                }
            }

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

        public static bool CheckContext(ApiContext context, string field)
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
                Email = "my@domain.com",
                FirstName = "Mike",
                Id = Guid.Empty,
                IsAdmin = false,
                ListAdminModules = new List<string> { "projects", "crm" },
                UserName = "Mike.Zanyatski",
                LastName = "Zanyatski",
                Title = "Manager",
                Groups = new List<GroupWrapperSummary> { GroupWrapperSummary.GetSample() },
                AvatarMedium = "url to medium avatar",
                Birthday = ApiDateTime.GetSample(),
                Department = "Marketing",
                Location = "Palo Alto",
                Notes = "Notes to worker",
                Lead = Guid.Empty,
                Sex = "male",
                Status = EmployeeStatus.Active,
                WorkFrom = ApiDateTime.GetSample(),
                //Terminated = ApiDateTime.GetSample(),
                CultureName = "en-EN",
                IsLDAP = false,
                IsSSO = false
            };
        }
    }
}