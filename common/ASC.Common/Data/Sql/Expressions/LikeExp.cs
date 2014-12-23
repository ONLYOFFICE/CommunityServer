/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;

namespace ASC.Common.Data.Sql.Expressions
{
    [Flags]
    public enum SqlLike
    {
        StartWith = 1,
        EndWith = 2,
        AnyWhere = StartWith | EndWith,
    }

    public class LikeExp : Exp
    {
        private readonly string column;
        private readonly string str;

        public LikeExp(string column, string str, SqlLike like)
        {
            this.column = column;
            if (str != null)
            {
                if ((like & SqlLike.StartWith) == SqlLike.StartWith) str += "%";
                if ((like & SqlLike.EndWith) == SqlLike.EndWith) str = "%" + str;
            }
            this.str = str;
        }

        public override string ToString(ISqlDialect dialect)
        {
            return str != null
                       ? string.Format("{0} {1}like ?", column, Not ? "not " : string.Empty)
                       : string.Format("{0} is {1}null", column, Not ? "not " : string.Empty);
        }

        public override object[] GetParameters()
        {
            return str == null ? new object[0] : new object[] {str};
        }
    }
}