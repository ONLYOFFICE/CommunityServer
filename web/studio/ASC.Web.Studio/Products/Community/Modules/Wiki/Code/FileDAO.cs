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