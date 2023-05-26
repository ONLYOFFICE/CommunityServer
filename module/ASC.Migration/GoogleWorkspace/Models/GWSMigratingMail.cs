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
