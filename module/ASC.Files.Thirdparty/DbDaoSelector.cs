/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Text.RegularExpressions;
using ASC.Files.Core;
using ASC.Files.Core.Security;

namespace ASC.Files.Thirdparty
{
    internal class DbDaoSelector : RegexDaoSelectorBase<int>
    {
        public DbDaoSelector(Func<object, IFileDao> fileDaoActivator,
                             Func<object, IFolderDao> folderDaoActivator,
                             Func<object, ISecurityDao> securityDaoActivator,
                             Func<object, ITagDao> tagDaoActivator)
            : base(new Regex(@"^\d+$", RegexOptions.Singleline | RegexOptions.Compiled),
                   fileDaoActivator,
                   folderDaoActivator,
                   securityDaoActivator,
                   tagDaoActivator,
                   IdConverter)
        {
        }

        public override object GetIdCode(object id)
        {
            return 0;
        }

        private static int IdConverter(object id)
        {
            int result;
            if (Int32.TryParse(Convert.ToString(id), out result))
                return result;

            throw new ArgumentException(string.Format("Int32.TryParse({0})", id));
        }
    }
}