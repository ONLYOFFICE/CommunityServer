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
using ASC.Core.Tenants;

namespace ASC.Web.UserControls.Wiki.Data
{
    public class File : IVersioned
    {
        public int Tenant { get; set; }
        public Guid UserID { get; set; }
        public int Version { get; set; }
        public DateTime Date { get; set; }

        public Guid OwnerID { get; set; }
        public object GetObjectId()
        {
            return FileName;
        }

        private string _FileName = string.Empty;
        public string FileName
        {
            get { return _FileName; }
            set { _FileName = value.Replace("[", "(").Replace("]", ")"); }
        }               
        public string UploadFileName { get; set; }
        public string FileLocation { get; set; }
        public int FileSize { get; set; }


        public File()
        {
            UploadFileName = FileLocation = string.Empty;
            Date = TenantUtil.DateTimeNow();
        }      
    }
}