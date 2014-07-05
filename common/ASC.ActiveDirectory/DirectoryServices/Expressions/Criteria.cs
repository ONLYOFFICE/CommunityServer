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
using System.Collections.Generic;

namespace ASC.ActiveDirectory.Expressions
{
    /// <summary>
    
    /// </summary>
    public class Criteria : ICloneable
    {
        private CriteriaType _type = CriteriaType.And;
        private List<Expression> _expressions = new List<Expression>();
        private List<Criteria> _nestedCriteras = new List<Criteria>();
        /// <summary>
        
        /// </summary>
        
        
        public Criteria(CriteriaType type, params Expression[] expressions)
        {
            _expressions.AddRange(expressions);
            _type = type;
        }
        /// <summary>
        
        /// </summary>
        /// <param name="expressions"></param>
        
        public Criteria And(params Expression[] expressions)
        {
            _nestedCriteras.Add(Criteria.All(expressions));
            return this;
        }
        /// <summary>
        
        /// </summary>
        ///  <param name="expressions"></param>
        
        public Criteria Or(params Expression[] expressions)
        {
            _nestedCriteras.Add(Criteria.Any(expressions));
            return this;
        }
        /// <summary>
        
        /// </summary>
        /// <param name="nested"></param>
        
        public Criteria Add(Criteria nested)
        {
            _nestedCriteras.Add(nested);
            return this;
        }

        /// <summary>
        
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string criteria = "({0}{1}{2})";
            string expressions = string.Empty;
            foreach (var expr in _expressions)
            {
                expressions += expr.ToString();
            }
            string criterias = string.Empty;
            foreach (var crit in _nestedCriteras)
            {
                criterias += crit.ToString();
            }
            return String.Format(criteria, _type == CriteriaType.And ? "&" : "|", expressions, criterias);
        }


        /// <summary>
        
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static Criteria All(params Expression[] expressions)
        {
            return new Criteria(CriteriaType.And, expressions);
        }
        /// <summary>
        
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static Criteria Any(params Expression[] expressions)
        {
            return new Criteria(CriteriaType.Or, expressions);
        }

        #region ICloneable Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Criteria cr = new Criteria(_type);
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
