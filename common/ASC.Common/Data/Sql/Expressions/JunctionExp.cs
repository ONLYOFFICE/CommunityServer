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

#region usings

using System.Collections.Generic;

#endregion

namespace ASC.Common.Data.Sql.Expressions
{
    public class JunctionExp : Exp
    {
        private readonly bool and;
        private readonly Exp exp1;
        private readonly Exp exp2;

        public JunctionExp(Exp exp1, Exp exp2, bool and)
        {
            this.exp1 = exp1;
            this.exp2 = exp2;
            this.and = and;
        }

        public override string ToString(ISqlDialect dialect)
        {
            string format = exp1 is JunctionExp && ((JunctionExp) exp1).and != and ? "({0})" : "{0}";
            format += " {1} ";
            format += exp2 is JunctionExp && ((JunctionExp) exp2).and != and ? "({2})" : "{2}";
            return Not
                       ? string.Format(format, (!exp1).ToString(dialect), and ? "or" : "and",
                                       (!exp2).ToString(dialect))
                       : string.Format(format, exp1.ToString(dialect), and ? "and" : "or", exp2.ToString(dialect));
        }

        public override object[] GetParameters()
        {
            var parameters = new List<object>();
            parameters.AddRange(exp1.GetParameters());
            parameters.AddRange(exp2.GetParameters());
            return parameters.ToArray();
        }
    }
}