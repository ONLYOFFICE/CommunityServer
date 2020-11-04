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


namespace ASC.Data.Storage.Configuration
{
    static class Schema
    {
        public const string SECTION_NAME = "storage";
        public const string FILE_PATH = "file";

        public const string APPENDERS = "appender";
        public const string APPEND = "append";
        public const string APPENDSECURE = "appendssl";
        public const string EXTs = "exts";

        public const string HANDLERS = "handler";
        public const string PROPERTIES = "properties";
        public const string PROPERTY = "property";

        public const string MODULES = "module";
        public const string TYPE = "type";
        public const string NAME = "name";
        public const string VALUE = "value";
        public const string PATH = "path";
        public const string DATA = "data";
        public const string VIRTUALPATH = "virtualpath";
        public const string VISIBLE = "visible";
        public const string COUNT_QUOTA = "count";
        public const string APPEND_TENANT_ID = "appendTenantId";
        public const string ACL = "acl";
        public const string EXPIRES = "expires";
        public const string DOMAINS = "domain";
        public const string PUBLIC = "public";
        public const string DISABLEDMIGRATE = "disableMigrate";
        public const string DISABLEDENCRYPTION = "disableEncryption";
    }
}