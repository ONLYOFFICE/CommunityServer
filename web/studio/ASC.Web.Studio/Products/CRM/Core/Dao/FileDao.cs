/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Files.Api;

namespace ASC.CRM.Core.Dao
{
    public class FileDao : AbstractDao
    {
        public FileDao(int tenantID)
            : base(tenantID)
        {
        }

        #region Public Methods

        public File GetFile(int id, int version)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                var file = 0 < version ? dao.GetFile(id, version) : dao.GetFile(id);
                return file;
            }
        }

        public void DeleteFile(int id)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                dao.DeleteFile(id);
            }
        }

        public object GetRoot()
        {
            return FilesIntegration.RegisterBunch("crm", "crm_common", "");
        }

        public object GetMy()
        {
            return FilesIntegration.RegisterBunch("files", "my", SecurityContext.CurrentAccount.ID.ToString());
        }

        public File SaveFile(File file, System.IO.Stream stream)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                return dao.SaveFile(file, stream);
            }
        }

        public List<int> GetEventsByFile(int id)
        {
            using (var tagdao = FilesIntegration.GetTagDao())
            {
                var tags = tagdao.GetTags(id, FileEntryType.File, TagType.System).ToList().FindAll(tag => tag.TagName.StartsWith("RelationshipEvent_"));
                return tags.Select(item => Convert.ToInt32(item.TagName.Split(new[] { '_' })[1])).ToList();
            }
        }

        #endregion
    }
}