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
using System.IO;
using System.Web;
using ASC.Api.Interfaces;

namespace ASC.Api.Impl
{
    internal class ApiStoragePath : IApiStoragePath
    {
        #region IApiStoragePath Members

        public string GetDataDirectory(IApiEntryPoint entryPoint)
        {
            string basePath;
            if (HttpContext.Current != null)
            {
                basePath = HttpContext.Current.Server.MapPath("~/EntryPointData");
            }
            else
            {
                basePath = AppDomain.CurrentDomain.GetData("Data Directory") as string;
                if (string.IsNullOrEmpty(basePath))
                {
                    basePath = Path.Combine(Environment.CurrentDirectory, "EntryPointData");
                }
            }
            if (string.IsNullOrEmpty(basePath))
            {
                throw new InvalidOperationException("failed to resolve data directory");
            }
            string apidata = Path.Combine(basePath, entryPoint.Name);
            if (!Directory.Exists(apidata))
            {
                Directory.CreateDirectory(apidata);
            }
            return apidata;
        }

        #endregion
    }
}