using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using ASC.Core;
using ASC.Core.Users;
using ASC.Migration.Core.Models;
using ASC.Migration.Core.Models.Api;
using ASC.Migration.Resources;
using ASC.Web.Files.Resources;

namespace ASC.Migration.OwnCloud.Models
{
    public class OCMigratingUser : MigratingUser<OCMigratingContacts, OCMigratingCalendar, OCMigratingFiles, OCMigratingMail>
    {
        public override string Email => userInfo.Email;

        public override string DisplayName => userInfo.ToString();

        public List<MigrationModules> ModulesList = new List<MigrationModules>();

        public Guid Guid => userInfo.ID;

        public override string ModuleName => MigrationResource.ModuleNameUsers;

        public string ConnectionString { get; set; }
        private string rootFolder;
        private bool hasPhoto;
        private string pathToPhoto;
        private UserInfo userInfo;
        private OCUser User;
        private Regex emailRegex = new Regex(@"(\S*@\S*\.\S*)");

        public OCMigratingUser(string key, OCUser userData, string rootFolder, Action<string, Exception> log) : base(log)
        {
            Key = key;
            User = userData;
            this.rootFolder = rootFolder;
        }

        public override void Parse()
        {
            ModulesList.Add(new MigrationModules(ModuleName, MigrationResource.OnlyofficeModuleNamePeople));
            userInfo = new UserInfo()
            {
                ID = Guid.NewGuid()
            };
            var drivePath = Directory.Exists(Path.Combine(rootFolder, "data", Key, "cache")) ?
                Path.Combine(rootFolder, "data", Key, "cache") : null;
            if (drivePath == null)
            {
                hasPhoto = false;
            }
            else
            {
                pathToPhoto = File.Exists(Path.Combine(drivePath, "avatar_upload")) ? Directory.GetFiles(drivePath, "avatar_upload")[0] : null;
                hasPhoto = pathToPhoto != null ? true : false;
            }

            string[] userName = User.Data.DisplayName.Split(' ');
            userInfo.FirstName = userName[0];
            if (userName.Length > 1)
            {
                userInfo.LastName = userName[1];
            }
            if (User.Data.Email != null && User.Data.Email != "" && User.Data.Email != "NULL")
            {
                var email = emailRegex.Match(User.Data.Email);
                if (email.Success)
                {
                    userInfo.Email = email.Groups[1].Value;
                }
                userInfo.UserName = userInfo.Email.Split('@').First();
            }
            userInfo.ActivationStatus = EmployeeActivationStatus.Pending;
            Action<string, Exception> log = (m, e) => { Log($"{DisplayName} ({Email}): {m}", e); };

            MigratingContacts = new OCMigratingContacts(this, User.Addressbooks, log);
            MigratingContacts.Parse();
            if (MigratingContacts.ContactsCount != 0)
            {
                ModulesList.Add(new MigrationModules(MigratingContacts.ModuleName, MigrationResource.OnlyofficeModuleNameMail));
            }

            MigratingCalendar = new OCMigratingCalendar(User.Calendars, log);
            //MigratingCalendar.Parse();
            if (MigratingCalendar.CalendarsCount != 0)
            {
                ModulesList.Add(new MigrationModules(MigratingCalendar.ModuleName, MigrationResource.OnlyofficeModuleNameCalendar));
            }

            MigratingFiles = new OCMigratingFiles(this, User.Storages, rootFolder, log);
            MigratingFiles.Parse();
            if (MigratingFiles.FoldersCount != 0 || MigratingFiles.FilesCount != 0)
            {
                ModulesList.Add(new MigrationModules(MigratingFiles.ModuleName, MigrationResource.OnlyofficeModuleNameDocuments));
            }

            MigratingMail = new OCMigratingMail(log);
        }

        public void dataСhange(MigratingApiUser frontUser)
        {
            if (userInfo.Email == null)
            {
                userInfo.Email = frontUser.Email;
                if (userInfo.UserName == null)
                {
                    userInfo.UserName = userInfo.Email.Split('@').First();
                }
            }
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
                using (var ms = new MemoryStream())
                {
                    using (var fs = File.OpenRead(pathToPhoto))
                    {
                        fs.CopyTo(ms);
                    }
                    CoreContext.UserManager.SaveUserPhoto(saved.ID, ms.ToArray());
                }
            }
        }
    }
}
