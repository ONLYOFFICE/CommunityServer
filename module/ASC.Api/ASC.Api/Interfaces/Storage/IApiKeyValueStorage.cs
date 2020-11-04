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


namespace ASC.Api.Interfaces.Storage
{
    public interface IApiKeyValueStorage
    {
        object Get(IApiEntryPoint entrypoint, string key);
        void Set(IApiEntryPoint entrypoint, string key, object @object);
        bool Exists(IApiEntryPoint entrypoint, string key);
        void Remove(IApiEntryPoint entrypoint, string key);
        void Clear(IApiEntryPoint entrypoint);
    }
}