/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using System.Text.RegularExpressions;

using ASC.Data.Storage;

namespace ASC.Data.Backup.Utils
{
    static class FCKEditorPathUtility
    {
        private static readonly Regex regex = new Regex("(?<start>\\/data\\/(?>htmleditorfiles|fckcomments))(?<tenant>\\/0\\/|\\/[\\d]+\\/\\d\\d\\/\\d\\d\\/)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);


        public static string CorrectStoragePath(string content, int tenant)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return content;
            }
            var tenantPath = "/" + TenantPath.CreatePath(tenant.ToString()) + "/";
            return regex.Replace(content, (m) => m.Success ? m.Groups["start"] + tenantPath : string.Empty);
        }
    }
}
