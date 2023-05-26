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
using System.Text.RegularExpressions;

using ASC.Core;
using ASC.Core.Users;
using ASC.Migration.Core.Models;
using ASC.Migration.Core.Models.Api;
using ASC.Migration.GoogleWorkspace.Models.Parse;
using ASC.Migration.Resources;
using ASC.Web.Files.Resources;

using HtmlAgilityPack;

using Newtonsoft.Json;

namespace ASC.Migration.GoogleWorkspace.Models
{
    public class GwsMigratingUser : MigratingUser<GwsMigratingContacts, GwsMigratingCalendar, GwsMigratingFiles, GwsMigratingMail>
    {
        public override string Email => userInfo.Email;
        public Guid Guid => userInfo.ID;
        public List<MigrationModules> ModulesList = new List<MigrationModules>();
        public override string ModuleName => MigrationResource.ModuleNameUsers;

        public override string DisplayName => $"{userInfo.FirstName} {userInfo.LastName}".Trim();

        public override void Parse()
        {
            ModulesList.Add(new MigrationModules(ModuleName, MigrationResource.OnlyofficeModuleNamePeople));
            userInfo = new UserInfo()
            {
                ID = Guid.NewGuid()
            };

            ParseRootHtml();
            ParseProfile();
            ParseAccount();

            Action<string, Exception> log = (m, e) => { Log($"{DisplayName} ({Email}): {m}", e); };

            MigratingContacts = new GwsMigratingContacts(rootFolder, this, log);
            MigratingContacts.Parse();
            if (MigratingContacts.ContactsCount != 0)
            {
                ModulesList.Add(new MigrationModules(MigratingContacts.ModuleName, MigrationResource.OnlyofficeModuleNameMail));
            }

            MigratingCalendar = new GwsMigratingCalendar(rootFolder, log);
            //MigratingCalendar.Parse();
            if (MigratingCalendar.CalendarsCount != 0)
            {
                ModulesList.Add(new MigrationModules(MigratingCalendar.ModuleName, MigrationResource.OnlyofficeModuleNameCalendar));
            }

            MigratingFiles = new GwsMigratingFiles(rootFolder, this, log);
            MigratingFiles.Parse();
            if (MigratingFiles.FoldersCount != 0 || MigratingFiles.FilesCount != 0)
            {
                ModulesList.Add(new MigrationModules(MigratingFiles.ModuleName, MigrationResource.OnlyofficeModuleNameDocuments));
            }

            MigratingMail = new GwsMigratingMail(rootFolder, this, log);
            //MigratingMail.Parse();

            userInfo.UserName = userInfo.Email.Split('@').First();
            if (userInfo.FirstName == null || userInfo.FirstName == "")
            {
                userInfo.FirstName = userInfo.Email.Split('@').First();
            }
            userInfo.ActivationStatus = EmployeeActivationStatus.Pending;
        }

        public void dataСhange(MigratingApiUser frontUser)
        {
            if (userInfo.LastName == null)
            {
                userInfo.LastName = "NOTPROVIDED";
            }
        }

        public override void Migrate()
        {
            if (string.IsNullOrWhiteSpace(userInfo.FirstName))
            {
                userInfo.FirstName = FilesCommonResource.UnknownFirstName;
            }
            if (string.IsNullOrWhiteSpace(userInfo.LastName))
            {
                userInfo.LastName = FilesCommonResource.UnknownLastName;
            }

            var saved = CoreContext.UserManager.GetUserByEmail(userInfo.Email);
            if (saved != Constants.LostUser)
            {
                saved.Contacts = saved.Contacts.Union(userInfo.Contacts).ToList();
                userInfo.ID = saved.ID;
            }
            else
            {
                saved = CoreContext.UserManager.SaveUserInfo(userInfo);
            }
            if (hasPhoto)
            {
                using (var fs = File.OpenRead(Key))
                {
                    using (var zip = new ZipArchive(fs))
                    {
                        using (var ms = new MemoryStream())
                        {
                            using (var imageStream = zip.GetEntry(string.Join("/", "Takeout", "Profile", "ProfilePhoto.jpg")).Open())
                            {
                                imageStream.CopyTo(ms);
                            }
                            CoreContext.UserManager.SaveUserPhoto(saved.ID, ms.ToArray());
                        }
                    }
                }
            }
        }

        public GwsMigratingUser(string key, string rootFolder, Action<string, Exception> log) : base(log)
        {
            Key = key;
            this.rootFolder = rootFolder;
        }

        private Regex emailRegex = new Regex(@"(\S*@\S*\.\S*)");
        private Regex phoneRegex = new Regex(@"(\+?\d+)");

        private string rootFolder;
        private UserInfo userInfo;
        private bool hasPhoto;

        private void ParseRootHtml()
        {
            var htmlFiles = Directory.GetFiles(rootFolder, "*.html");
            if (htmlFiles.Count() != 1)
            {
                throw new Exception("Incorrect Takeout format.");
            }

            var htmlPath = htmlFiles[0];

            var doc = new HtmlDocument();
            doc.Load(htmlPath);

            var emailNode = doc.DocumentNode.SelectNodes("//body//h1[@class='header_title']")[0];
            var matches = emailRegex.Match(emailNode.InnerText);
            if (!matches.Success)
            {
                throw new Exception("Couldn't parse root html.");
            }

            userInfo.Email = matches.Groups[1].Value;
        }

        private void ParseProfile()
        {
            var profilePath = Path.Combine(rootFolder, "Profile", "Profile.json");
            if (!File.Exists(profilePath)) return;

            var googleProfile = JsonConvert.DeserializeObject<GwsProfile>(File.ReadAllText(profilePath));

            if (googleProfile.Birthday != null)
            {
                userInfo.BirthDate = googleProfile.Birthday.Value.DateTime;
            }

            if (googleProfile.Gender != null)
            {
                switch (googleProfile.Gender.Type)
                {
                    case "male":
                        userInfo.Sex = true;
                        break;

                    case "female":
                        userInfo.Sex = false;
                        break;

                    default:
                        userInfo.Sex = null;
                        break;
                }
            }

            userInfo.FirstName = googleProfile.Name.GivenName;
            userInfo.LastName = googleProfile.Name.FamilyName;

            if (googleProfile.Emails != null)
            {
                foreach (var email in googleProfile.Emails.Distinct())
                {
                    AddEmailToUser(userInfo, email.Value);
                }
            }

            var profilePhotoPath = Path.Combine(rootFolder, "Profile", "ProfilePhoto.jpg");
            hasPhoto = File.Exists(profilePhotoPath);
        }

        private void ParseAccount()
        {
            var accountPath = Path.Combine(rootFolder, "Google Account");
            if (!Directory.Exists(accountPath)) return;
            var htmlFiles = Directory.GetFiles(accountPath, "*.SubscriberInfo.html");
            if (htmlFiles.Count() != 1) return;

            var htmlPath = htmlFiles[0];

            var doc = new HtmlDocument();
            doc.Load(htmlPath);

            var alternateEmails = emailRegex.Matches(doc.DocumentNode.SelectNodes("//div[@class='section'][1]/ul/li[2]")[0].InnerText);
            foreach (Match match in alternateEmails)
            {
                AddEmailToUser(userInfo, match.Value);
            }

            var contactEmail = emailRegex.Match(doc.DocumentNode.SelectNodes("//div[@class='section'][3]/ul/li[1]")[0].InnerText);
            if (contactEmail.Success)
            {
                AddEmailToUser(userInfo, contactEmail.Groups[1].Value);
            }

            var recoveryEmail = emailRegex.Match(doc.DocumentNode.SelectNodes("//div[@class='section'][3]/ul/li[2]")[0].InnerText);
            if (recoveryEmail.Success)
            {
                AddEmailToUser(userInfo, recoveryEmail.Groups[1].Value);
            }

            var recoverySms = phoneRegex.Match(doc.DocumentNode.SelectNodes("//div[@class='section'][3]/ul/li[3]")[0].InnerText);
            if (recoverySms.Success)
            {
                AddPhoneToUser(userInfo, recoverySms.Groups[1].Value);
            }
        }

        private void AddEmailToUser(UserInfo userInfo, string email)
        {
            if (userInfo.Email != email && !userInfo.Contacts.Contains(email))
            {
                userInfo.Contacts.Add(email.EndsWith("@gmail.com") ? "gmail" : "mail"); // SocialContactsManager.ContactType_gmail in ASC.WebStudio
                userInfo.Contacts.Add(email);
            }
        }

        private void AddPhoneToUser(UserInfo userInfo, string phone)
        {
            if (userInfo.MobilePhone != phone && !userInfo.Contacts.Contains(phone))
            {
                userInfo.Contacts.Add("mobphone"); // SocialContactsManager.ContactType_mobphone in ASC.WebStudio
                userInfo.Contacts.Add(phone);
            }
        }
    }
}
