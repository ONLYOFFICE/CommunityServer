/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


#region Import

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security;
using System.Threading;
using ASC.CRM.Core.Entities;
using ASC.Common.Security;
using System.Linq;
using System.Linq.Expressions;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Configuration;
using Action = ASC.Common.Security.Authorizing.Action;
using Constants = ASC.Core.Users.Constants;
using SecurityContext = ASC.Core.SecurityContext;
using ASC.Web.Files.Api;
using ASC.Files.Core.Security;
using ASC.Files.Core;

#endregion

namespace ASC.CRM.Core
{

    public class FileSecurity : IFileSecurity
    {


        #region IFileSecurity Members

        public bool CanCreate(FileEntry file, Guid userId)
        {
            return true;
        }

        public bool CanDelete(FileEntry file, Guid userId)
        {
            return file.CreateBy == userId || file.ModifiedBy == userId || CRMSecurity.IsAdmin;

        }

        public bool CanEdit(FileEntry file, Guid userId)
        {
            return file.CreateBy == userId || file.ModifiedBy == userId || CRMSecurity.IsAdmin;
        }

        public bool CanRead(FileEntry file, Guid userId)
        {
            var eventDao = Global.DaoFactory.GetRelationshipEventDao();
            var tagDao = FilesIntegration.GetTagDao();
            var invoiceDao = Global.DaoFactory.GetInvoiceDao();

            var invoice = invoiceDao.GetByFileId(Convert.ToInt32(file.ID));
            if (invoice != null)
            {
                return CRMSecurity.CanAccessTo(invoice);
            }
            else
            {
                var eventIds = tagDao.GetTags(file.ID, FileEntryType.File, TagType.System)
                           .Where(x => x.TagName.StartsWith("RelationshipEvent_"))
                           .Select(x => Convert.ToInt32(x.TagName.Split(new[] { '_' })[1]));

                if (!eventIds.Any()) return false;

                var eventItem = eventDao.GetByID(eventIds.First());
                return CRMSecurity.CanAccessTo(eventItem);
            }
        }

        #endregion
    }

    public class FileSecurityProvider : IFileSecurityProvider
    {
        public IFileSecurity GetFileSecurity(string data)
        {
            return new FileSecurity();
        }

    }
}