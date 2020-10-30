/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Text;

namespace ASC.ActiveDirectory.Base.Expressions
{
    public class Expression : ICloneable
    {
        private readonly Op _op;
        private bool _negative;
        private readonly string _attributeName;
        private readonly string _attributeValue;

        private const string EQUIAL = "=";
        private const string APPROXIMATELY_EQUIAL = "~=";
        private const string GREATER = ">";
        private const string GREATER_OR_EQUAL = ">=";
        private const string LESS = "<";
        private const string LESS_OR_EQUAL = "<=";

        internal Expression()
        {
        }

        public string Name {
            get { return _attributeName; }
        }

        public string Value
        {
            get { return _attributeValue; }
        }

        public Op Operation
        {
            get { return _op; }
        }

        /// <summary>
        /// To specify unary operations
        /// </summary>
        /// <param name="op">Operator</param>
        /// <param name="attrbuteName">Attribute name</param>
        public Expression(string attrbuteName, Op op)
        {
            if (op != Op.Exists && op != Op.NotExists)
                throw new ArgumentException("op");

            if (string.IsNullOrEmpty(attrbuteName))
                throw new ArgumentException("attrbuteName");

            _op = op;
            _attributeName = attrbuteName;
            _attributeValue = "*";
        }

        /// <summary>
        /// To specify binary operations
        /// </summary>
        /// <param name="op">Operator</param>
        /// <param name="attrbuteName">Attribute name</param>
        /// <param name="attrbuteValue">Attribute value</param>
        public Expression(string attrbuteName, Op op, string attrbuteValue)
        {
            if (op == Op.Exists || op == Op.NotExists)
                throw new ArgumentException("op");

            if (string.IsNullOrEmpty(attrbuteName))
                throw new ArgumentException("attrbuteName");

            _op = op;
            _attributeName = attrbuteName;
            _attributeValue = attrbuteValue;
        }

        /// <summary>
        /// Expression as a string
        /// </summary>
        /// <returns>Expression string</returns>
        public override string ToString()
        {
            string sop;
            switch (_op)
            {
                case Op.NotExists:
                case Op.Exists:
                case Op.Equal:
                case Op.NotEqual:
                    sop = EQUIAL;
                    break;
                case Op.Greater:
                    sop = GREATER;
                    break;
                case Op.GreaterOrEqual:
                    sop = GREATER_OR_EQUAL;
                    break;
                case Op.Less:
                    sop = LESS;
                    break;
                case Op.LessOrEqual:
                    sop = LESS_OR_EQUAL;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var expressionString = "({0}{1}{2}{3})";
            expressionString = string.Format(expressionString,
                //positive or negative
                (((int) _op & 0x010000) == 0x010000 || _negative) ? "!" : "", _attributeName, sop, 
                EscapeLdapSearchFilter(_attributeValue));

            return expressionString;
        }

        /// <summary>
        /// Escapes the LDAP search filter to prevent LDAP injection attacks.
        /// </summary>
        /// <param name="searchFilter">The search filter.</param>
        /// <see cref="https://blogs.oracle.com/shankar/entry/what_is_ldap_injection" />
        /// <see cref="http://msdn.microsoft.com/en-us/library/aa746475.aspx" />
        /// <returns>The escaped search filter.</returns>
        private static string EscapeLdapSearchFilter(string searchFilter)
        {
            var escape = new StringBuilder(); // If using JDK >= 1.5 consider using StringBuilder
            foreach (var current in searchFilter)
            {
                switch (current)
                {
                    case '\\':
                        escape.Append(@"\5c");
                        break;
                    case '*':
                        escape.Append(@"\2a");
                        break;
                    case '(':
                        escape.Append(@"\28");
                        break;
                    case ')':
                        escape.Append(@"\29");
                        break;
                    case '\u0000':
                        escape.Append(@"\00");
                        break;
                    case '/':
                        escape.Append(@"\2f");
                        break;
                    default:
                        escape.Append(current);
                        break;
                }
            }

            return escape.ToString();
        }

        /// <summary>
        /// Negation
        /// </summary>
        /// <returns>Self</returns>
        public Expression Negative()
        {
            _negative = !_negative;
            return this;
        }

        /// <summary>
        /// Existence
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <returns>New Expression</returns>
        public static Expression Exists(string attrbuteName)
        {
            return new Expression(attrbuteName, Op.Exists);
        }

        /// <summary>
        /// Non-Existence
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <returns>New Expression</returns>
        public static Expression NotExists(string attrbuteName)
        {
            return new Expression(attrbuteName, Op.NotExists);
        }

        /// <summary>
        /// Equality
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <param name="attrbuteValue"></param>
        /// <returns>New Expression</returns>
        public static Expression Equal(string attrbuteName, string attrbuteValue)
        {
            return new Expression(attrbuteName, Op.Equal, attrbuteValue);
        }

        /// <summary>
        /// Not equality
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <param name="attrbuteValue"></param>
        /// <returns></returns>
        public static Expression NotEqual(string attrbuteName, string attrbuteValue)
        {
            return new Expression(attrbuteName, Op.NotEqual, attrbuteValue);
        }

        public static Expression Parse(string origin)
        {
            string spliter = null;
            var op = Op.Equal;

            var index = origin.IndexOf(EQUIAL, StringComparison.Ordinal);

            if (index > -1)
            {
                spliter = EQUIAL;
                op = Op.Equal;
            }
            else if ((index = origin.IndexOf(GREATER, StringComparison.Ordinal)) > -1)
            {
                spliter = GREATER;
                op = Op.Greater;
            }
            else if ((index = origin.IndexOf(GREATER_OR_EQUAL, StringComparison.Ordinal)) > -1)
            {
                spliter = GREATER_OR_EQUAL;
                op = Op.GreaterOrEqual;
            }
            else if ((index = origin.IndexOf(LESS, StringComparison.Ordinal)) > -1)
            {
                spliter = LESS;
                op = Op.Less;
            }
            else if ((index = origin.IndexOf(LESS_OR_EQUAL, StringComparison.Ordinal)) > -1)
            {
                spliter = LESS_OR_EQUAL;
                op = Op.LessOrEqual;
            }
            else if ((index = origin.IndexOf(APPROXIMATELY_EQUIAL, StringComparison.Ordinal)) > -1)
            {
                spliter = APPROXIMATELY_EQUIAL;
                op = Op.Exists;
            }

            if (string.IsNullOrEmpty(spliter))
                return null;

            var attributeName = origin.Substring(0, index);
            var attributeValue = origin.Substring(index + 1);

            if (string.IsNullOrEmpty(attributeName) || string.IsNullOrEmpty(attributeValue))
                return null;

            return new Expression(attributeName, op, attributeValue);
        }

        #region ICloneable Members
        /// <summary>
        /// ICloneable implemetation
        /// </summary>
        /// <returns>Clone object</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
