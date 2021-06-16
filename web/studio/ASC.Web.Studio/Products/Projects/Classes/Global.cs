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


using System.Collections.Generic;

using ASC.Web.Core.Files;

namespace ASC.Web.Projects.Classes
{
    public class Global
    {
        public static readonly string DbID = "default";

        public static readonly int EntryCountOnPage = 25;
        public static readonly int VisiblePageCount = 3;

        public static readonly KeyValuePair<FileUtility.CsvDelimiter, string> ReportCsvDelimiter = new KeyValuePair<FileUtility.CsvDelimiter, string>(FileUtility.CsvDelimiter.Comma, ",");
    }
}