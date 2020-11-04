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
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Files.Core;

namespace ASC.Feed.Aggregator.Modules.Documents
{
    public class DocumentsDbHelper
    {
        public static SqlQuery GetRootFolderType(string parentFolderColumnName)
        {
            return new SqlQuery("files_folder d")
                .From("files_folder_tree t")
                .Select("concat(cast(d.folder_type as char), d.create_by, cast(d.id as char))")
                .Where(Exp.EqColumns("d.id", "t.parent_id") &
                       Exp.EqColumns("t.folder_id", "f." + parentFolderColumnName))
                .OrderBy("level", false)
                .SetMaxResults(1);
        }

        public static FolderType ParseRootFolderType(object v)
        {
            return v != null
                       ? (FolderType)Enum.Parse(typeof(FolderType), v.ToString().Substring(0, 1))
                       : default(FolderType);
        }

        public static Guid ParseRootFolderCreator(object v)
        {
            return v != null ? new Guid(v.ToString().Substring(1, 36)) : default(Guid);
        }

        public static int ParseRootFolderId(object v)
        {
            return v != null ? int.Parse(v.ToString().Substring(1 + 36)) : default(int);
        }
    }
}