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
