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

using System;
using System.IO;
using ASC.Core.Tenants;
using ASC.FullTextIndex.Service.Config;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace ASC.FullTextIndex.Service
{
    class TextSearcher
    {
        private readonly string module;
        private readonly string path;


        public TextSearcher(string module, string path)
        {
            if (string.IsNullOrEmpty(module)) throw new ArgumentNullException("module");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");

            this.module = module;
            this.path = path;
        }

        public TextSearchResult Search(string query, Tenant tenant)
        {
            var result = new TextSearchResult(module);

            if (string.IsNullOrEmpty(query) || !Directory.Exists(path) || Directory.GetFiles(path, "*.*").Length == 0)
            {
                return result;
            }

            var dir = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(path));
            var searcher = new IndexSearcher(dir, false);
            try
            {
                var analyzer = new AnalyzersProvider().GetAnalyzer(tenant.GetCulture().TwoLetterISOLanguageName);
                var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Text", analyzer);
                parser.SetDefaultOperator(QueryParser.Operator.AND);
                if (TextIndexCfg.MaxQueryLength < query.Length)
                {
                    query = query.Substring(0, TextIndexCfg.MaxQueryLength);
                }
                Query q = null;
                try
                {
                    q = parser.Parse(query);
                }
                catch (Lucene.Net.QueryParsers.ParseException)
                {
                    q = parser.Parse(QueryParser.Escape(query));
                }

#pragma warning disable 618
                var hits = searcher.Search(q);
#pragma warning restore 618
                for (int i = 0; i < hits.Length(); i++)
                {
                    var doc = hits.Doc(i);
                    result.AddIdentifier(doc.Get("Id"));
                }
            }
            finally
            {
                searcher.Close();
                dir.Close();
            }
            return result;
        }
    }
}
