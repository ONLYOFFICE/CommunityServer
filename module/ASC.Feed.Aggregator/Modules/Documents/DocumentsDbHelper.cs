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

using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Files.Core;

namespace ASC.Feed.Aggregator.Modules.Documents
{
    public class DocumentsDbHelper
    {
        private const char seporator = '|';

        public static SqlQuery GetRootFolderType(string parentFolderColumnName)
        {
            return new SqlQuery("files_folder d")
                .From("files_folder_tree t")
                .Select(string.Format("concat(cast(d.folder_type as char), '{0}', d.create_by, '{0}', cast(d.id as char))", seporator))
                .Where(Exp.EqColumns("d.id", "t.parent_id") &
                       Exp.EqColumns("t.folder_id", "f." + parentFolderColumnName))
                .OrderBy("level", false)
                .SetMaxResults(1);
        }

        private delegate bool TryParseHandler<T>(string value, out T result);

        private static T TryParse<T>(object v, int index, TryParseHandler<T> handler)
        {
            var result = default(T);

            if (v == null) return result;

            var data = v.ToString().Split(seporator);

            if (data.Length != 3) return result;

            if (index < 0 || index > 2) return result;

            if (string.IsNullOrEmpty(data[index])) return result;

            handler(data[index], out result);

            return result;
        }

        public static FolderType ParseRootFolderType(object v)
        {
            return TryParse<FolderType>(v, 0, Enum.TryParse);
        }

        public static Guid ParseRootFolderCreator(object v)
        {
            return TryParse<Guid>(v, 1, Guid.TryParse);
        }

        public static int ParseRootFolderId(object v)
        {
            return TryParse<int>(v, 2, int.TryParse);
        }
    }
}