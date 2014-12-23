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
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;

namespace ASC.Web.UserControls.Wiki.Data
{
    class FileDAO : BaseDao
    {
        public FileDAO(string dbid, int tenant)
            : base(dbid, tenant)
        {
        }


        public List<File> GetFiles(IEnumerable<string> names)
        {
            return GetFiles(names == null ? Exp.Empty : Exp.In("filename", names.ToArray()));
        }

        public List<File> FindFiles(string name)
        {
            if (string.IsNullOrEmpty(name)) return new List<File>();

            return GetFiles(Exp.Like("filename", name, SqlLike.StartWith));
        }

        public File SaveFile(File file)
        {
            if (file == null) throw new ArgumentNullException("file");

            var i = Insert("wiki_files")
                .InColumnValue("filename", file.FileName)
                .InColumnValue("uploadfilename", file.UploadFileName)
                .InColumnValue("version", file.Version)
                .InColumnValue("userid", file.UserID.ToString())
                .InColumnValue("date", TenantUtil.DateTimeToUtc(file.Date))
                .InColumnValue("filelocation", file.FileLocation)
                .InColumnValue("filesize", file.FileSize);

            db.ExecuteNonQuery(i);

            return file;
        }

        public void RemoveFile(string fileName)
        {
            var d1 = Delete("wiki_files").Where("filename", fileName);
            var d2 = Delete("wiki_comments").Where("pagename", fileName);

            db.ExecuteBatch(new[] { d1, d2 });
        }


        private List<File> GetFiles(Exp where)
        {
            var q = Query("wiki_files")
                .Select("filename", "uploadfilename", "version", "userid", "date", "filelocation", "filesize")
                .Where(where)
                .OrderBy("filename", true);

            return db
                .ExecuteList(q)
                .ConvertAll(r => ToFile(r))
                .GroupBy(f => f.FileName)
                .Select(g => g.OrderByDescending(f => f.Version).First())
                .ToList();
        }

        private File ToFile(object[] r)
        {
            return new File
            {
                FileName = (string)r[0],
                UploadFileName = (string)r[1],
                Version = Convert.ToInt32(r[2]),
                UserID = new Guid((string)r[3]),
                Date = TenantUtil.DateTimeFromUtc((DateTime)r[4]),
                FileLocation = (string)r[5],
                FileSize = Convert.ToInt32(r[6]),
                Tenant = this.tenant,
            };
        }
    }
}