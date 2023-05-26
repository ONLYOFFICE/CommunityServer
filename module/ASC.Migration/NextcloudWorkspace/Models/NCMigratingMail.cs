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
