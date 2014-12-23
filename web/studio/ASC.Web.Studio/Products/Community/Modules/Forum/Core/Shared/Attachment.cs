/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
