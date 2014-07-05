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
using System.Data.SQLite;
using System.Text;
using SqliteFunction = System.Data.SQLite.SQLiteFunction;
using SqliteFunctionAttribute = System.Data.SQLite.SQLiteFunctionAttribute;

namespace ASC.Common.Data.SQLite
{
    [SqliteFunction(Name = "concat", Arguments = -1, FuncType = FunctionType.Scalar)]
    public class ConcatFunction : SqliteFunction
    {
        public override object Invoke(object[] args)
        {
            var result = new StringBuilder();
            foreach (object arg in args)
            {
                result.Append(arg);
            }
            return result.Length != 0 ? result.ToString() : null;
        }
    }

    [SqliteFunction(Name = "concat_ws", Arguments = -1, FuncType = FunctionType.Scalar)]
    public class ConcatWSFunction : SqliteFunction
    {
        public override object Invoke(object[] args)
        {
            if (args.Length < 2 || args[0] == null || DBNull.Value.Equals(args[0])) return null;
            var result = new StringBuilder();
            for (int i = 1; i < args.Length; i++)
            {
                result.AppendFormat("{0}{1}", args[i], i == args.Length - 1 ? string.Empty : args[0]);
            }
            return result.Length != 0 ? result.ToString() : null;
        }
    }
}