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


using System.Collections.Generic;

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