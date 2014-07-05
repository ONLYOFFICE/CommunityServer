/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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