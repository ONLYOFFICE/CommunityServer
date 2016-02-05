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
using System.Runtime.Serialization;

using ASC.Common.Data.Sql;
using ASC.Core;

namespace ASC.FullTextIndex.Service
{
    [DataContract]
    public class ModuleInfo
    {
        private string select = "id";
        private readonly List<ModuleAttr> additionalAttributes; 
        private readonly MatchSphinx match;
        private const int Limit = 10000;

        private string sqlQuery;
        private readonly SqlQuery query;

        public string Main { get { return Name + "_main"; } }
        public string Delta { get { return Name + "_delta"; } }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string SqlQuery
        {
            get
            {
                if (!string.IsNullOrEmpty(sqlQuery))
                    return sqlQuery;

                query.Select(select).From(Name);

                foreach (var attribute in additionalAttributes)
                {
                    query.Where(attribute.ToString());
                }

                return query
                    .Where(match.ToString())
                    .SetFirstResult(0)
                    .SetMaxResults(Limit - 1)
                       + " OPTION max_matches=" + Limit;
            }
            set { sqlQuery = value; }
        }

        public ModuleInfo(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            match = new MatchSphinx();
            query = new SqlQuery();
            Name = name;
            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            additionalAttributes = new List<ModuleAttr>();

            if (tenant != null)
            {
                AddAttribute("tenant_id", tenant.TenantId);
            }
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
            match.Add(text, columns.ToList());
            return this;
        }

        public ModuleInfo AddAttribute(string title, string value)
        {
            additionalAttributes.Add(ModuleAttr.CreateAttr(title, value));
            return this;
        }

        public ModuleInfo AddAttribute(string title, int[] values)
        {
            additionalAttributes.Add(ModuleAttr.CreateAttr(title, values));
            return this;
        }

        public ModuleInfo AddAttribute(string title, int value)
        {
            additionalAttributes.Add(ModuleAttr.CreateAttr(title, value));
            return this;
        }

        public string GetChunk(int chunk)
        {
            return string.Format("{0}_{1}", Name, chunk);
        }

        public string GetChunkByTenantId(int tenantId, int chunks, int dimension)
        {
            var index = tenantId / dimension;
            if (index >= chunks)
                index = chunks - 1;

            return GetChunk(index + 1);
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var mi = obj as ModuleInfo;
            return mi != null && Name == mi.Name;
        }
    }

    class MatchSphinx
    {
        private readonly List<MatchSphinxItem> items;

        public MatchSphinx()
        {
            items = new List<MatchSphinxItem>();
        }

        public void Add(string text, List<string> columns)
        {
            items.Add(new MatchSphinxItem(text, columns));
        }

        public override string ToString()
        {
            return !items.Any() 
                ? "match('')" 
                : string.Format("match('({0})')", string.Join(" ", items.Select(r=> r.ToString())));
        }
    }

    class MatchSphinxItem
    {
        private readonly string text;
        private readonly List<string> columns;

        public MatchSphinxItem(string text, List<string> columns)
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
            return columns.Count == 0
                ? "*"
                : string.Format("({0})", string.Join(",", columns));
        }

        private static string EscapeString(string text)
        {
            var escapeListFrom = new[] { "\\", "(", ")", "|", "-", "!", "@", "~", "\"", "&", "//", "^", "$", "=", "'", "\x00", "\n", "\r", "\x1a" };
            var escapeListTo = new[] { "\\\\", @"\\\(", @"\\\)", @"\\\|", @"\\\-", @"\\\!", @"\\\@", @"\\\~", "\\\"", @"\\\&", @"\\\/", @"\\\^", @"\\\$", @"\\\=", "\\'", "\\x00", "\\n", "\\r", "\\x1a" };
            for (var i = 0; i < escapeListFrom.Length; i++ )
            {
                text = text.Replace(escapeListFrom[i], escapeListTo[i]);
            }

            if(!(text.StartsWith("\"") && text.EndsWith("\"")))
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

    internal class ModuleAttr
    {
        private string Title { get; set; }
        private string Value { get; set; }
        private string Format { get; set; }

        private ModuleAttr(string title, string value, string format)
        {
            Title = title;
            Value = value;
            Format = format;
        }

        public static ModuleAttr CreateAttr(string title, string value)
        {
            return new ModuleAttr(title, value, "{0}='{1}'");
        }

        public static ModuleAttr CreateAttr(string title, int value)
        {
            return new ModuleAttr(title, value.ToString(), "{0}={1}");
        }

        public static ModuleAttr CreateAttr(string title, int[] values)
        {
            return new ModuleAttr(title, string.Join(",", values.Select(r=> r.ToString())), "{0} in({1})");
        }

        public override string ToString()
        {
            return string.Format(Format, Title, Value);
        }
    }
}
