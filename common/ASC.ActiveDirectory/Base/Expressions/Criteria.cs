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
using System.Collections.Generic;
using System.Linq;

namespace ASC.ActiveDirectory.Base.Expressions
{
    /// <summary>
    /// Criteria
    /// </summary>
    public class Criteria : ICloneable
    {
        private readonly CriteriaType _type;
        private readonly List<Expression> _expressions = new List<Expression>();
        private readonly List<Criteria> _nestedCriteras = new List<Criteria>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of critera</param>
        /// <param name="expressions">Expressions</param>
        public Criteria(CriteriaType type, params Expression[] expressions)
        {
            _expressions.AddRange(expressions);
            _type = type;
        }

        /// <summary>
        /// Add nested expressions as And criteria
        /// </summary>
        /// <param name="expressions">Expressions</param>
        /// <returns>Self</returns>
        public Criteria And(params Expression[] expressions)
        {
            _nestedCriteras.Add(All(expressions));
            return this;
        }

        /// <summary>
        /// Add nested expressions as Or criteria
        /// </summary>
        ///  <param name="expressions">Expressions</param>
        /// <returns>Self</returns>
        public Criteria Or(params Expression[] expressions)
        {
            _nestedCriteras.Add(Any(expressions));
            return this;
        }

        /// <summary>
        /// Add nested Criteria
        /// </summary>
        /// <param name="nested"></param>
        /// <returns>себя</returns>
        public Criteria Add(Criteria nested)
        {
            _nestedCriteras.Add(nested);
            return this;
        }

        /// <summary>
        ///  Criteria as a string
        /// </summary>
        /// <returns>Criteria string</returns>
        public override string ToString()
        {
            var criteria = "({0}{1}{2})";
            var expressions = _expressions.Aggregate(string.Empty, (current, expr) => current + expr.ToString());
            var criterias = _nestedCriteras.Aggregate(string.Empty, (current, crit) => current + crit.ToString());
            return string.Format(criteria, _type == CriteriaType.And ? "&" : "|", expressions, criterias);
        }

        /// <summary>
        /// Group of Expression union as And
        /// </summary>
        /// <param name="expressions">Expressions</param>
        /// <returns>new Criteria</returns>
        public static Criteria All(params Expression[] expressions)
        {
            return new Criteria(CriteriaType.And, expressions);
        }

        /// <summary>
        /// Group of Expression union as Or
        /// </summary>
        /// <param name="expressions">Expressions</param>
        /// <returns>new Criteria</returns>
        public static Criteria Any(params Expression[] expressions)
        {
            return new Criteria(CriteriaType.Or, expressions);
        }

        #region ICloneable Members

        /// <summary>
        /// ICloneable implemetation
        /// </summary>
        /// <returns>Clone object</returns>
        public object Clone()
        {
            var cr = new Criteria(_type);
            foreach (var ex in _expressions)
            {
                cr._expressions.Add(ex.Clone() as Expression);
            }
            foreach (var nc in _nestedCriteras)
            {
                cr._nestedCriteras.Add(nc.Clone() as Criteria);
            }
            return cr;
        }

        #endregion
    }
}
