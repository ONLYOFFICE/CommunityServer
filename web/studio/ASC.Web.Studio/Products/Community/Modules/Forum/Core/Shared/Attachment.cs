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
using System.Reflection;

namespace ASC.Forum
{
	public enum AttachmentContentType
    {
        Uknown = 0,

        Audio = 2,

        Video = 1,

        Image = 3,

        Application = 4,

        Office = 5,

        Text = 6,

        Zip = 7,

        Others = 8
    }

	public class Attachment
    {       
        public virtual int ID{get; set;}

        public virtual DateTime CreateDate { get; set; }
       
        public virtual string Name{get; set;}  
        
        public virtual int Size{get; set;}

        public virtual int DownloadCount{get; set;}

        public virtual AttachmentContentType ContentType { get; set; }      

        public virtual string MIMEContentType { get; set; }

        public virtual string OffsetPhysicalPath { get; set; }

        public virtual int PostID { get; set; }

        public virtual int TenantID { get; set; }

        public Attachment()
        {
            this.ID= 0;            
            this.Name = "";
            this.CreateDate = ASC.Core.Tenants.TenantUtil.DateTimeNow();
            this.Size = 0;
            this.MIMEContentType = "";
            this.ContentType = AttachmentContentType.Uknown;
            this.OffsetPhysicalPath = "";
        }
    }
}
