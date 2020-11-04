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


namespace MultiLanguage
{
    using System;
    using System.Security;

    public enum tagMIMECONTF
    {
        MIMECONTF_BROWSER = 2,
        MIMECONTF_EXPORT = 0x400,
        MIMECONTF_IMPORT = 8,
        MIMECONTF_MAILNEWS = 1,
        MIMECONTF_MIME_IE4 = 0x10000000,
        MIMECONTF_MIME_LATEST = 0x20000000,
        MIMECONTF_MIME_REGISTRY = 0x40000000,
        MIMECONTF_MINIMAL = 4,
        MIMECONTF_PRIVCONVERTER = 0x10000,
        MIMECONTF_SAVABLE_BROWSER = 0x200,
        MIMECONTF_SAVABLE_MAILNEWS = 0x100,
        MIMECONTF_VALID = 0x20000,
        MIMECONTF_VALID_NLS = 0x40000
    }
}
