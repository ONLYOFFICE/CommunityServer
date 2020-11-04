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
using System.Runtime.Serialization;
using ASC.Files.Core.Security;
using ASC.Web.Files.Services.WCFService;

namespace ASC.Api.Documents
{
    /// <summary>
    /// </summary>
    [DataContract(Name = "share", Namespace = "")]
    public class FileShareParams
    {
        /// <summary>
        /// </summary>
        [DataMember(Name = "shareTo", Order = 0)]
        public Guid ShareTo { get; set; }
        
        /// <summary>
        /// </summary>
        [DataMember(Name = "access", Order = 1)]
        public FileShare Access { get; set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public AceWrapper ToAceObject()
        {
            return new AceWrapper
                          {
                              Share = Access,
                              SubjectId = ShareTo,
                              SubjectGroup = !Core.CoreContext.UserManager.UserExists(ShareTo)
                          };
        }
    }
}