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

using ASC.Common.Data;
using ASC.Common.Utils;
using ASC.Core.Tenants;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ASC.FullTextIndex.Service
{
    class TextIndexer
    {
        private readonly string path;

        private readonly Tenant tenant;

        private readonly ModuleInfo module;


        public TextIndexer(string path, Tenant tenant, ModuleInfo module)
        {
            this.path = path;
            this.tenant = tenant;
            this.module = module;
        }

        public int FindChangedAndIndex()
        {
            var lastDateTimeFile = Path.Combine(path, "last.time");
            var lastDateTime = DateTime.MinValue;
            try
            {
                if (File.Exists(lastDateTimeFile))
                {
                    lastDateTime = DateTime.Parse(File.ReadAllText(lastDateTimeFile)).ToUniversalTime();
                }
            }
            catch (FormatException) { }
            catch (ArgumentNullException) { }

            var copyLastDateTime = lastDateTime;
            lastDateTime = DateTime.UtcNow;

            var texts = SelectTextsForIndex(copyLastDateTime, true);

            if (0 < texts.Count)
            {
                var directory = GetOrCreateDirectory(path);
                var analyzer = new AnalyzersProvider().GetAnalyzer(tenant.GetCulture().TwoLetterISOLanguageName);
                var create = directory.ListAll().Length == 0;
                var index = new IndexWriter(directory, analyzer, create, IndexWriter.MaxFieldLength.UNLIMITED);
                try
                {
                    foreach (var t in texts)
                    {
                        var term = new Term("Id", t.Item1);
                        if (string.IsNullOrEmpty(t.Item2))
                        {
                            index.DeleteDocuments(term);
                        }
                        else
                        {
                            var doc = new Document();
                            doc.Add(new Field("Id", t.Item1, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.YES));
                            doc.Add(new Field("Text", t.Item2, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO));
                            index.UpdateDocument(term, doc);
                        }
                    }
                }
                finally
                {
                    index.Optimize();
                    index.Commit();
                    index.Close();
                }

                File.WriteAllText(lastDateTimeFile, lastDateTime.ToString("o"));
            }

            return texts.Count;
        }

        public int FindRemovedAndIndex()
        {
            var textIds = SelectTextsForIndex(DateTime.MinValue, false).ConvertAll(i => i.Item1);
            var affected = 0;
            var directory = GetOrCreateDirectory(path);
            if (0 < directory.ListAll().Length)
            {
                var reader = IndexReader.Open(directory, false);
                try
                {
                    var terms = reader.Terms();
                    try
                    {
                        while (terms.Next())
                        {
                            var term = terms.Term();
                            if (term.Field() == "Id" && !textIds.Contains(term.Text()))
                            {
                                reader.DeleteDocuments(term);
                                affected++;
                            }
                        }
                    }
                    finally
                    {
                        terms.Close();
                    }
                }
                finally
                {
                    reader.Commit();
                    reader.Close();
                }
            }
            return affected;
        }


        private Lucene.Net.Store.Directory GetOrCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var directory = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(path));
            
            if (IndexWriter.IsLocked(directory))
            {
                IndexWriter.Unlock(directory);
            }

            return directory;
        }


        private List<Tuple<string, string>> SelectTextsForIndex(DateTime date, bool withContent)
        {
            using (var db = new DbManager(module.ConnectionStringName))
            {
                return db.Connection
                    .CreateCommand(withContent ? module.Select : ModifySelect(module.Select))
                    .AddParameter("tenant", tenant.TenantId)
                    .AddParameter("lastModified", date)
                    .ExecuteList()
                    .ConvertAll(r =>
                    {
                        var text = default(string);
                        if (withContent)
                        {
                            text = (string)r[1];
                            var html = 2 < r.Length && r[2] != null ? Convert.ToBoolean(r[2]) : false;
                            if (html && !string.IsNullOrEmpty(text))
                            {
                                text = HtmlUtil.GetText(text);
                            }
                        }
                        return Tuple.Create(r[0].ToString(), text);
                    });
            }
        }

        private string ModifySelect(string sql)
        {
            //cut text columns from sql select
            var select = new StringBuilder();
            var inselect = false;
            var start = -1;
            var brackets = 0;
            for (int i = 0; i < sql.Length; i++)
            {
                var c = sql[i];
                select.Append(c);

                if (c == '(') brackets++;
                if (c == ')') brackets--;

                if (brackets == 0 && select.ToString().EndsWith("select", StringComparison.InvariantCultureIgnoreCase))
                {
                    inselect = true;
                }
                if (inselect && start == -1 && c == ',' && brackets == 0)
                {
                    start = i;
                }
                if (brackets == 0 && inselect && start != -1 && select.ToString().EndsWith("from", StringComparison.InvariantCultureIgnoreCase))
                {
                    select.Remove(select.Length - (i - start + 1), i - start + 1);
                    select.Append(" from");

                    start = -1;
                    inselect = false;
                }
            }
            return select.ToString();
        }
    }
}
