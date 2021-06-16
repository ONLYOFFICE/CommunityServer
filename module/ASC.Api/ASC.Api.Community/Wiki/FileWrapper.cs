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


using System.Runtime.Serialization;

using ASC.Api.Employee;
using ASC.Specific;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;

namespace ASC.Api.Wiki.Wrappers
{
    [DataContract(Name = "file", Namespace = "")]
    public class FileWrapper
    {
        [DataMember(Order = 0)]
        public string Name { get; set; }

        [DataMember(Order = 1)]
        public EmployeeWraper UpdatedBy { get; set; }

        [DataMember(Order = 2)]
        public ApiDateTime Updated { get; set; }

        [DataMember(Order = 3)]
        public string Location { get; set; }

        public FileWrapper(File file)
        {
            Name = file.FileName;
            UpdatedBy = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(file.UserID));
            Updated = (ApiDateTime)file.Date;
            Location = file.FileLocation;
        }

        public FileWrapper()
        {

        }

        public static FileWrapper GetSample()
        {
            return new FileWrapper
            {
                Name = "File name",
                Location = WikiEngine.GetFileLocation("File name"),
                Updated = ApiDateTime.GetSample(),
                UpdatedBy = EmployeeWraper.GetSample()
            };
        }
    }
}
