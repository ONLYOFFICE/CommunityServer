
using System;

using ASC.Migration.Core.Models;
using ASC.Migration.Resources;

namespace ASC.Migration.NextcloudWorkspace.Models
{
    public class NCMigratingMail : MigratingMail
    {
        private int messagesCount;

        public override int MessagesCount => messagesCount;
        public override string ModuleName => MigrationResource.ModuleNameMail;
        public override void Migrate()
        {
            throw new System.NotImplementedException();
        }

        public override void Parse()
        {
            messagesCount++;
            throw new System.NotImplementedException();
        }

        public NCMigratingMail(Action<string, Exception> log) : base(log) { }
    }
}
