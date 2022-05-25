using System;
using System.Collections.Generic;
using System.IO;

using ASC.Migration.Core.Models;
using ASC.Migration.GoogleWorkspace.Models.Parse;
using ASC.Migration.Resources;

using MimeKit;

namespace ASC.Migration.GoogleWorkspace.Models
{
    public class GwsMigratingMail : MigratingMail
    {
        private int messagesCount;
        private string rootFolder;
        private GwsMigratingUser user;
        private List<GwsMail> mails = new List<GwsMail>();
        
        public override int MessagesCount => messagesCount;
        public override string ModuleName => MigrationResource.ModuleNameMail;

        public GwsMigratingMail(string rootFolder, GwsMigratingUser user, Action<string, Exception> log) : base(log)
        {
            this.rootFolder = rootFolder;
            this.user = user;
        }

        public override void Migrate()
        {
            throw new System.NotImplementedException();
        }

        public override void Parse()
        {
            var path = Path.Combine(rootFolder, "Mail");
            var foldersName = Directory.GetFiles(path);
            foreach(var item in foldersName)
            {
                var mail = new GwsMail();
                List<MimeMessage> messagesList = new List<MimeMessage>();
                using (FileStream sr = File.OpenRead(item))
                {
                    var parser = new MimeParser(sr, MimeFormat.Mbox);
                    while (!parser.IsEndOfStream)
                    {
                        messagesList.Add(parser.ParseMessage());
                        messagesCount++;
                    }
                }
                var folder = item.Split(Path.DirectorySeparatorChar);
                mail.ParentFolder = folder[folder.Length - 1].Split('.')[0].ToLower() == "All mail Including Spam and Trash".ToLower()? "inbox" : folder[folder.Length - 1].Split('.')[0];
                mail.Message = messagesList;
                mails.Add(mail);
            }
            throw new System.NotImplementedException();
        }
    }
}
