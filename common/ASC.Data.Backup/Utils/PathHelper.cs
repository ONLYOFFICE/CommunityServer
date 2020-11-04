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


using System.IO;
using System.Reflection;

namespace ASC.Data.Backup.Utils
{
    internal static class PathHelper
    {
        public static string ToRootedPath(string path)
        {
            return ToRootedPath(path, Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
        }

        public static string ToRootedPath(string path, string basePath)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(basePath, path);
            }
            return Path.GetFullPath(path);
        }

        public static string ToRootedConfigPath(string path)
        {
            if (!Path.HasExtension(path))
            {
                path = Path.Combine(path, "Web.config");
            }
            return ToRootedPath(path);
        }

        public static string GetTempFileName(string tempDir)
        {
            string tempPath;
            do
            {
                tempPath = Path.Combine(tempDir, Path.GetRandomFileName());
            } while (File.Exists(tempPath));
            return tempPath;
        }
    }
}
