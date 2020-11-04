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
using ASC.Api.Employee;
using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Services.WCFService;

namespace ASC.Api.Documents
{
    /// <summary>
    /// </summary>
    public class FileShareWrapper
    {
        private FileShareWrapper()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="aceWrapper"></param>
        public FileShareWrapper(AceWrapper aceWrapper)
        {
            IsOwner = aceWrapper.Owner;
            IsLocked = aceWrapper.LockedRights;
            if (aceWrapper.SubjectGroup)
            {
                if (aceWrapper.SubjectId == FileConstant.ShareLinkId)
                {
                    SharedTo = new FileShareLink
                        {
                            Id = aceWrapper.SubjectId,
                            ShareLink = aceWrapper.Link
                        };
                }
                else
                {
                    //Shared to group
                    SharedTo = new GroupWrapperSummary(CoreContext.UserManager.GetGroupInfo(aceWrapper.SubjectId));
                }
            }
            else
            {
                SharedTo = new EmployeeWraperFull(CoreContext.UserManager.GetUsers(aceWrapper.SubjectId));
            }
            Access = aceWrapper.Share;
        }

        /// <summary>
        /// </summary>
        public FileShare Access { get; set; }

        /// <summary>
        /// </summary>
        public object SharedTo { get; set; }

        /// <summary>
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// </summary>
        public bool IsOwner { get; set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static FileShareWrapper GetSample()
        {
            return new FileShareWrapper
                {
                    Access = FileShare.ReadWrite,
                    IsLocked = false,
                    IsOwner = true,
                    SharedTo = EmployeeWraper.GetSample()
                };
        }
    }


    /// <summary>
    /// </summary>
    public class FileShareLink
    {
        /// <summary>
        /// </summary>
        public Guid Id;

        /// <summary>
        /// </summary>
        public string ShareLink;
    }
}