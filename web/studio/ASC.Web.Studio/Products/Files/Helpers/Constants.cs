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

namespace ASC.Files.Core
{
    public static class FileConstant
    {
        public static readonly string ModuleId = "files";

        public static readonly string StorageModule = "files";
        public static readonly string StorageDomainTmp = "files_temp";
        public static readonly string StorageTemplate = "files_template";

        public static readonly string DatabaseId = "files";

        public static readonly Guid ShareLinkId = new Guid("{D77BD6AF-828B-41f5-84ED-7FFE2565B13A}");

        public const string StartDocPath = "startdocuments/";
        public const string NewDocPath = "newdocuments/";

        public const string DownloadTitle = "download";
    }
}