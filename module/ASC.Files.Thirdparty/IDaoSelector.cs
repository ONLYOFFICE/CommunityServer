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


using ASC.Files.Core;
using ASC.Files.Core.Security;

namespace ASC.Files.Thirdparty
{
    internal interface IDaoSelector
    {
        bool IsMatch(object id);
        IFileDao GetFileDao(object id);
        IFolderDao GetFolderDao(object id);
        ISecurityDao GetSecurityDao(object id);
        ITagDao GetTagDao(object id);
        object ConvertId(object id);
        object GetIdCode(object id);
    }
}