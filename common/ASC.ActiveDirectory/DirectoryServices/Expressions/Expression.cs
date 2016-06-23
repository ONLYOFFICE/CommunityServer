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

namespace ASC.ActiveDirectory.Expressions
{
    /// <summary>
    /// Выражение
    /// </summary>
    public class Expression : ICloneable
    {
        private readonly Op _op;
        private bool _negative;
        private readonly string _attributeName;
        private readonly string _attributeValue;

        internal Expression() { }

        /// <summary>
        /// Для задания унарных операций
        /// </summary>
        /// <param name="op">оператор</param>
        /// <param name="attrbuteName">название атрибута</param>
        public Expression(string attrbuteName, Op op)
        {
            if (op != Op.Exists && op != Op.NotExists)
                throw new ArgumentException("op");

            if (String.IsNullOrEmpty(attrbuteName))
                throw new ArgumentException("attrbuteName");

            _op = op;
            _attributeName = attrbuteName;
            _attributeValue = "*";
        }

        /// <summary>
        /// Для задания бинарных операций
        /// </summary>
        /// <param name="op">оператор</param>
        /// <param name="attrbuteName">название атрибута</param>
        /// <param name="attrbuteValue">значение аттрибута</param>
        public Expression(string attrbuteName, Op op, string attrbuteValue)
        {
            if (op == Op.Exists || op == Op.NotExists)
                throw new ArgumentException("op");

            if (String.IsNullOrEmpty(attrbuteName))
                throw new ArgumentException("attrbuteName");

            _op = op;
            _attributeName = attrbuteName;
            _attributeValue = attrbuteValue;
        }

        /// <summary>
        /// Выражение в виде строки
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string sop = string.Empty;
            switch(_op)
            {
                case Op.NotExists:
                case Op.Exists:
                case Op.Equal:
                case Op.NotEqual:
                    sop = "=";
                    break;
                case Op.Greater:

                    sop =">";
                    break;
                case Op.GreaterOrEqual:
                    sop =">=";
                    break;
                case Op.Less:
                    sop ="<";
                    break;
                case Op.LessOrEqual:
                    sop ="<=";
                    break;
            }
                            
            string expressionString = "({0}{1}{2}{3})";
            expressionString = 
                String.Format(
                    expressionString,
                    //позитивная или негативная
                    (((int)_op & 0x010000) == 0x010000 || _negative) ? "!" : "",
                    _attributeName,
                    sop,
                    _attributeValue
                );

            return expressionString;
        }

        /// <summary>
        /// Отрицание
        /// </summary>
        /// <returns>Себя</returns>
        public Expression Negative()
        {
            _negative = !_negative;
            return this;
        }

        #region вспомогательные 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <returns></returns>
        public static Expression Exists(string attrbuteName)
        { return new Expression(attrbuteName, Op.Exists); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <returns></returns>
        public static Expression NotExists(string attrbuteName)
        { return new Expression(attrbuteName, Op.NotExists); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <param name="attrbuteValue"></param>
        /// <returns></returns>
        public static Expression Equal(string attrbuteName, string attrbuteValue)
        { return new Expression(attrbuteName, Op.Equal,attrbuteValue); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <param name="attrbuteValue"></param>
        /// <returns></returns>
        public static Expression NotEqual(string attrbuteName, string attrbuteValue)
        { return new Expression(attrbuteName, Op.NotEqual, attrbuteValue); }
        #endregion

        #region ICloneable Members

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
