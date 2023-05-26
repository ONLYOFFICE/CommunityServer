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


using System.Collections.Generic;

using ASC.Api.Attributes;
using ASC.Api.Interfaces;
using ASC.Web.Sample.Classes;

namespace ASC.Api.Sample
{
    /// <summary>
    /// Sample CRUD API.
    /// </summary>
    public class SampleApi : IApiEntryPoint
    {
        /// <summary>
        /// ASC.Api.Interfaces.IApiEntryPoint.Name
        /// </summary>
        public string Name
        {
            get { return "sample"; }
        }

        /// <summary>
        /// Creates a new module for the current portal.
        /// </summary>
        /// <short>
        /// Create a module
        /// </short>
        /// <param type="System.String, System" name="value">Module name</param>
        /// <returns>Newly created module</returns>
        /// <path>api/2.0/sample/create</path>
        /// <httpMethod>POST</httpMethod>
        /// <requiresAuthorization>false</requiresAuthorization>
        [Create("create", false)]
        public SampleClass Create(string value)
        {
            return SampleDao.Create(value);
        }

        /// <summary>
        /// Returns a module with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get a module
        /// </short>
        /// <param type="System.Int32, System" name="id">Module ID</param>
        /// <returns>Module</returns>
        /// <path>api/2.0/sample/read/{id}</path>
        /// <requiresAuthorization>false</requiresAuthorization>
        /// <httpMethod>GET</httpMethod>
        [Read(@"read/{id:[0-9]+}", false)]
        public SampleClass Read(int id)
        {
            return SampleDao.Read(id);
        }

        /// <summary>
        /// Returns all the portal modules.
        /// </summary>
        /// <short>
        /// Get modules
        /// </short>
        /// <returns>List of portal modules</returns>
        /// <collection>list</collection>
        /// <path>api/2.0/sample/read</path>
        /// <httpMethod>GET</httpMethod>
        /// <requiresAuthorization>false</requiresAuthorization>
        /// <collection>list</collection>
        [Read("read", false)]
        public List<SampleClass> Read()
        {
            return SampleDao.Read();
        }

        /// <summary>
        /// Updates the selected module with a name specified in the request.
        /// </summary>
        /// <short>
        /// Update a module
        /// </short>
        /// <param type="System.Int32, System" name="id">Module ID</param>
        /// <param type="System.String, System" name="value">New module name</param>
        /// <path>api/2.0/sample/update</path>
        /// <requiresAuthorization>false</requiresAuthorization>
        /// <httpMethod>PUT</httpMethod>
        [Update("update", false)]
        public void Update(int id, string value)
        {
            SampleDao.Update(id, value);
        }

        /// <summary>
        /// Deletes a module with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Delete a module
        /// </short>
        /// <param type="System.Int32, System" name="id">Module ID</param>
        /// <path>api/2.0/sample/delete/{id}</path>
        /// <requiresAuthorization>false</requiresAuthorization>
        /// <httpMethod>DELETE</httpMethod>
        [Delete("delete/{id:[0-9]+}", false)]
        public void Delete(int id)
        {
            SampleDao.Delete(id);
        }
    }
}
