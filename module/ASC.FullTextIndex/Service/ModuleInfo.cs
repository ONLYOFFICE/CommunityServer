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


using ASC.Common.Data.Sql;
using ASC.Core;
using System.Collections.Generic;
using System.Linq;

namespace ASC.FullTextIndex
{
    public class ModuleInfo
    {
        private const int LIMIT = 1000;

        private readonly List<string> addWhere = new List<string>();
        private readonly List<MatchSphinxItem> matches = new List<MatchSphinxItem>();
        private readonly SqlQuery query = new SqlQuery();

        private string select = "id";

        public string Name { get; private set; }
        public string Main { get; private set; }
        public string Delta { get; private set; }


        public ModuleInfo(string name)
        {
            Name = name;
            Main = Name + "_main";
            Delta = Name + "_delta";
        }

        public ModuleInfo Where(string exp)
        {
            query.Where(exp);
            return this;
        }

        public ModuleInfo Select(string column)
        {
            select = column;
            return this;
        }

        public ModuleInfo OrderBy(string column, bool asc)
        {
            query.OrderBy(column, asc);
            return this;
        }

        public ModuleInfo Match(string text, params string[] columns)
        {
            matches.Add(new MatchSphinxItem(text, columns));
            return this;
        }

        public ModuleInfo AddAttribute(string title, string value)
        {
            addWhere.Add(string.Format("{0}='{1}'", title, value));
            return this;
        }

        public ModuleInfo AddAttribute(string title, params int[] values)
        {
            if (values != null && 1 < values.Length)
            {
                addWhere.Add(string.Format("{0} in({1})", title, string.Join(",", values)));
            }
            else if (values != null && values.Length == 1)
            {
                addWhere.Add(string.Format("{0}={1}", title, values[0]));
            }
            return this;
        }

        public string GetChunk(int chunk)
        {
            return GetChunk(Name, chunk);
        }

        public static string GetChunk(string name, int chunk)
        {
            return string.Format("{0}_{1}", name, chunk);
        }


        internal string GetSqlQuery()
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            if (tenant != null)
            {
                AddAttribute("tenant_id", tenant.TenantId);
            }

            query
                .Select(select)
                .From(Name);

            addWhere.ForEach(w => query.Where(w));

            var match = matches.Any() ? string.Format("match('({0})')", string.Join(" ", matches)) : "match('')";

            return query
                .Where(match)
                .SetFirstResult(0)
                .SetMaxResults(LIMIT - 1) + " OPTION max_matches=" + LIMIT;
        }


        class MatchSphinxItem
        {
            private readonly string text;
            private readonly string[] columns;

            public MatchSphinxItem(string text, string[] columns)
            {
                this.columns = columns;
                this.text = text;
            }

            public override string ToString()
            {
                return string.Format("@{0} {1}", GetColumn(), EscapeString(text));
            }

            private string GetColumn()
            {
                return columns.Length == 0
                    ? "*"
                    : string.Format("({0})", string.Join(",", columns));
            }

            private static string EscapeString(string text)
            {
                var escapeListFrom = new[] { "\\", "(", ")", "|", "-", "!", "@", "~", "\"", "&", "/", "^", "$", "=", "'", "\x00", "\n", "\r", "\x1a" };
                var escapeListTo = new[] { "\\\\", @"\\\(", @"\\\)", @"\\\|", @"\\\-", @"\\\!", @"\\\@", @"\\\~", "\\\"", @"\\\&", @"\\/", @"\\\^", @"\\\$", @"\\\=", "\\'", "\\x00", "\\n", "\\r", "\\x1a" };
                for (var i = 0; i < escapeListFrom.Length; i++)
                {
                    text = text.Replace(escapeListFrom[i], escapeListTo[i]);
                }

                if (!(text.StartsWith("\"") && text.EndsWith("\"")))
                {
                    text = ExpandKeyword(text);
                }

                return text.ToLower();
            }

            private static string ExpandKeyword(string text)
            {
                return string.Format("( ({0}) | (*{0}*) | (={0}) )", text);
            }
        }
    }
}