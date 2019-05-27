/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Nest;

namespace ASC.ElasticSearch
{
    public class Selector<T> where T : Wrapper, new()
    {
        private readonly QueryContainerDescriptor<T> queryContainerDescriptor = new QueryContainerDescriptor<T>();
        private SortDescriptor<T> sortContainerDescriptor = new SortDescriptor<T>();
        private QueryContainer queryContainer = new QueryContainer();
        private int limit = 1000, offset;

        public Selector<T> Where<TProperty>(Expression<Func<T, TProperty>> selector, TProperty value)
        {
            queryContainer = queryContainer && +Wrap(selector, (w,r)=> r.Term(w, value));
            return this;
        }

        public Selector<T> Where(Expression<Func<T, Guid>> selector, Guid value)
        {
            queryContainer = queryContainer && +Wrap(selector, (a, w) => w.Match(r => r.Field(a).Query(value.ToString())));
            return this;
        }

        public Selector<T> Gt(Expression<Func<T, object>> selector, double? value)
        {
            queryContainer = queryContainer && +Wrap(selector, (w, r)=> r.Range(a => a.Field(w).GreaterThan(value)));
            return this;
        }

        public Selector<T> Lt(Expression<Func<T, object>> selector, double? value)
        {
            queryContainer = queryContainer && +Wrap(selector, (w, r)=> r.Range(a => a.Field(w).LessThan(value)));
            return this;
        }

        public Selector<T> Gt(Expression<Func<T, object>> selector, DateTime? value)
        {
            queryContainer = queryContainer && +Wrap(selector, (w,r) => r.DateRange(a => a.Field(w).GreaterThan(value)));
            return this;
        }

        public Selector<T> Ge(Expression<Func<T, object>> selector, DateTime? value)
        {
            queryContainer = queryContainer && +Wrap(selector, (w,r) => r.DateRange(a => a.Field(w).GreaterThanOrEquals(value)));
            return this;
        }

        public Selector<T> Lt(Expression<Func<T, object>> selector, DateTime? value)
        {
            queryContainer = queryContainer && +Wrap(selector, (w,r) => r.DateRange(a => a.Field(w).LessThan(value)));
            return this;
        }

        public Selector<T> Le(Expression<Func<T, object>> selector, DateTime? value)
        {
            queryContainer = queryContainer && +Wrap(selector, (w,r) => r.DateRange(a => a.Field(w).LessThanOrEquals(value)));
            return this;
        }

        public Selector<T> In<TValue>(Expression<Func<T, object>> selector, TValue[] values)
        {
            queryContainer = queryContainer && +Wrap(selector, (w,r) => r.Terms(a => a.Field(w).Terms(values)));

            return this;
        }

        public Selector<T> InAll<TValue>(Expression<Func<T, object>> selector, TValue[] values)
        {
            foreach (var v in values)
            {
                var v1 = v;
                queryContainer = queryContainer && +Wrap(selector, (w, r) => r.Term(a => a.Field(w).Value(v1)));
            }
            return this;
        }

        public Selector<T> Match(Expression<Func<T, object>> selector, string value)
        {
            value = value.PrepareToSearch();

            if (IsExactlyPhrase(value))
            {
                queryContainer = queryContainer & Wrap(selector, (a,w) => w.MatchPhrase(r => r.Field(a).Query(value.TrimQuotes())));
            }
            else if (value.HasOtherLetter() || IsExactly(value))
            {
                queryContainer = queryContainer & Wrap(selector, (a,w) => w.Match(r => r.Field(a).Query(value.TrimQuotes())));
            }
            else
            {
                if (IsPhrase(value))
                {
                    var phrase = value.Split(' ');
                    foreach (var p in phrase)
                    {
                        var p1 = p;
                        queryContainer = queryContainer & Wrap(selector, (a,w) => w.Wildcard(r => r.Field(a).Value(p1.WrapAsterisk())));
                    }
                }
                else
                {
                    queryContainer = queryContainer & Wrap(selector, (a,w) =>w.Wildcard(r => r.Field(a).Value(value.WrapAsterisk())));
                }

            }

            if (IsExactly(value))
            {
                queryContainer = queryContainer | Wrap(selector, (a,w) => w.MatchPhrase(r => r.Field(a).Query(value)));
            }

            return this;
        }

        private void Match(Func<Fields> propsFunc, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            value = value.PrepareToSearch();

            var props = propsFunc();

            if (IsExactlyPhrase(value))
            {
                queryContainer = queryContainer && MultiPhrase(props, value.TrimQuotes());
            }
            else if (value.HasOtherLetter() || IsExactly(value))
            {
                queryContainer = queryContainer && MultiMatch(props, value.TrimQuotes());
            }
            else
            {
                if (IsPhrase(value))
                {
                    var phrase = value.Split(' ');
                    foreach (var p in phrase)
                    {
                        queryContainer = queryContainer && MultiWildCard(props, p.WrapAsterisk());
                    }
                }
                else
                {
                    queryContainer = queryContainer && MultiWildCard(props, value.WrapAsterisk());
                }
            }

            if (IsExactly(value))
            {
                queryContainer = queryContainer || MultiPhrase(props, value);
            }
        }

        public Selector<T> Match(Expression<Func<T, object[]>> selector, string value)
        {
            Match(() => ((NewArrayExpression)selector.Body).Expressions.ToArray(), value);

            return this;
        }

        public Selector<T> MatchAll(string value)
        {
            Match(() => new T().GetContentProperties(), value);

            return this;
        }

        public Selector<T> Sort(Expression<Func<T, object>> selector, bool asc)
        {
            sortContainerDescriptor = sortContainerDescriptor.Field(selector, asc ? SortOrder.Ascending : SortOrder.Descending);

            return this;
        }

        public Selector<T> Limit(int newOffset = 0, int newLimit = 1000)
        {
            offset = newOffset;
            limit = newLimit;

            return this;
        }


        public Selector<T> Or(Expression<Func<Selector<T>, Selector<T>>> selectorLeft, Expression<Func<Selector<T>, Selector<T>>> selectorRight)
        {
            return new Selector<T>
            {
                queryContainer = queryContainer &
                    (selectorLeft.Compile()(new Selector<T>()).queryContainer |
                    selectorRight.Compile()(new Selector<T>()).queryContainer)
            };
        }

        public static Selector<T> Or(Selector<T> selectorLeft, Selector<T> selectorRight)
        {
            return new Selector<T>
            {
                queryContainer = selectorLeft.queryContainer | selectorRight.queryContainer
            };
        }

        public Selector<T> Not(Expression<Func<Selector<T>, Selector<T>>> selector)
        {
            return new Selector<T>
            {
                queryContainer = queryContainer & !selector.Compile()(new Selector<T>()).queryContainer
            };
        }

        public static Selector<T> Not(Selector<T> selector)
        {
            return new Selector<T>
            {
                queryContainer = !selector.queryContainer
            };
        }

        public static Selector<T> operator &(Selector<T> selectorLeft, Selector<T> selectorRight)
        {
            return new Selector<T>
            {
                queryContainer = selectorLeft.queryContainer & selectorRight.queryContainer
            };
        }

        public static Selector<T> operator |(Selector<T> selectorLeft, Selector<T> selectorRight)
        {
            return Or(selectorLeft, selectorRight);
        }

        public static Selector<T> operator !(Selector<T> selector)
        {
            return Not(selector);
        }

        internal Func<SearchDescriptor<T>, ISearchRequest> GetDescriptor(BaseIndexer<T> indexer, bool onlyId = false)
        {
            return s =>
            {
                var result = s
                .Query(q => queryContainer)
                .Index(indexer.IndexName)
                .Sort(r => sortContainerDescriptor)
                .From(offset)
                .Size(limit);

                if (onlyId)
                {
                    result = result.Source(r => r.Includes(a => a.Field("id")));
                }

                return result;
            };
        }

        internal Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> GetDescriptorForDelete(BaseIndexer<T> indexer, bool immediately = true)
        {
            return s =>
            {
                var result = s
                    .Query(q => queryContainer)
                    .Index(indexer.IndexName);
                if (immediately)
                {
                    result.Refresh();
                }
                return result;
            };
        }

        internal Func<UpdateByQueryDescriptor<T>, IUpdateByQueryRequest> GetDescriptorForUpdate(BaseIndexer<T> indexer, Func<ScriptDescriptor, IScript> script, bool immediately = true)
        {
            return s =>
            {
                var result = s
                    .Query(q => queryContainer)
                    .Index(indexer.IndexName)
                    .Script(script);

                if (immediately)
                {
                    result.Refresh();
                }

                return result;
            };
        }

        private QueryContainer Wrap(Field fieldSelector, Func<Field,QueryContainerDescriptor<T>, QueryContainer> selector)
        {
            var path = IsNested(fieldSelector);

            if (string.IsNullOrEmpty(path) && 
                !string.IsNullOrEmpty(fieldSelector.Name) &&
                fieldSelector.Name.StartsWith(JoinTypeEnum.Sub + ":") && 
                fieldSelector.Name.IndexOf(".", StringComparison.InvariantCulture) > 0)
            {
                var splitted = fieldSelector.Name.Split(':')[1];
                path = splitted.Split('.')[0];
                fieldSelector = new Field(splitted);
            }

            if (!string.IsNullOrEmpty(path))
            {
                return queryContainerDescriptor.Nested(a => a.Query(b => selector(fieldSelector, b)).Path(path));
            }

            return selector(fieldSelector, queryContainerDescriptor);
        }

        private string IsNested(Field selector)
        {
            var lambdaExpression = selector.Expression as LambdaExpression;
            if (lambdaExpression == null) return null;

            var methodCallExpression = lambdaExpression.Body as MethodCallExpression;
            if (methodCallExpression != null && methodCallExpression.Arguments.Count > 1)
            {
                var pathMember = methodCallExpression.Arguments[0] as MemberExpression;
                if (pathMember == null) return null;

                return pathMember.Member.Name.ToLowerCamelCase();
            }

            return null;
        }

        private bool IsPhrase(string searchText)
        {
            return searchText.Contains(" ") || searchText.Contains("\r\n") || searchText.Contains("\n");
        }

        private bool IsExactlyPhrase(string searchText)
        {
            return IsPhrase(searchText) && IsExactly(searchText);
        }

        private bool IsExactly(string searchText)
        {
            return searchText.StartsWith("\"") && searchText.EndsWith("\"");
        }

        private QueryContainer MultiMatch(Fields fields, string value)
        {
            var qcWildCard = new QueryContainer();

            foreach (var field in fields)
            {
                var field1 = field;
                qcWildCard = qcWildCard || Wrap(field1, (a, w) => w.Match(r => r.Field(a).Query(value.ToLower())));
            }

            return qcWildCard;
        }

        private QueryContainer MultiWildCard(Fields fields, string value)
        {
            var qcWildCard = new QueryContainer();

            foreach (var field in fields)
            {
                qcWildCard = qcWildCard || Wrap(field, (a, w) => w.Wildcard(r => r.Field(a).Value(value)));
            }

            return qcWildCard;
        }

        private QueryContainer MultiPhrase(Fields fields, string value)
        {
            var qcWildCard = new QueryContainer();

            foreach (var field in fields)
            {
                qcWildCard = qcWildCard || Wrap(field, (a, w) => w.MatchPhrase(r => r.Field(a).Query(value.ToLower())));
            }

            return qcWildCard;
        }
    }

    internal static class StringExtension
    {
        private static bool Any(this string value, UnicodeCategory category)
        {
            return !string.IsNullOrWhiteSpace(value)
            && value.Any(@char => char.GetUnicodeCategory(@char) == category);
        }

        public static bool HasOtherLetter(this string value)
        {
            return value.Any(UnicodeCategory.OtherLetter);
        }

        public static string WrapAsterisk(this string value)
        {
            var result = value;

            if (!value.Contains("*") && !value.Contains("?"))
            {
                result = "*" + result + "*";
            }

            return result;
        }

        public static string ReplaceBackslash(this string value)
        {
            return value.Replace("\\", "\\\\");
        }

        public static string TrimQuotes(this string value)
        {
            return value.Trim('\"');
        }

        public static string PrepareToSearch(this string value)
        {
            return value.ReplaceBackslash().ToLowerInvariant().Replace('ё','е').Replace('Ё', 'Е');
        }
    }
}
