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
using System.Resources;
using System.Globalization;
using System.Collections;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

namespace TMResourceData
{
    public class DBResourceReader : IResourceReader
    {
        readonly string fileName = "";
        readonly string language = "";
        private Hashtable Data;

        public DBResourceReader(string fileName, CultureInfo culture)
        {
            this.fileName = fileName;
            language = culture.Name;
            Data = new Hashtable();

            if (language == "")
                language = "Neutral";

            using (var dbManager = new DbManager("tmresource"))
            {
                var sql = new SqlQuery("res_data")
                    .Select("res_data.title", "textValue")
                    .InnerJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileID"))
                    .LeftOuterJoin("res_cultures", Exp.EqColumns("res_cultures.title", "res_data.cultureTitle"))
                    .Where("ResName", fileName)
                    .Where("res_cultures.Title", language);

                var list = dbManager.ExecuteList(sql);

                foreach (var t in list)
                {
                    Data.Add(t[0], t[1]);
                }
            }
        }

        public bool DataIsEmpty
        {
            get
            {
                return Data.Count == 0;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        void IDisposable.Dispose()
        {
        }

        void IResourceReader.Close()
        { }
    }
}
