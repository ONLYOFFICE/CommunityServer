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

namespace ASC.Common.Data.SQLite
{
    [SQLiteFunction(Name = "group_concat", Arguments = 2, FuncType = FunctionType.Aggregate)]
    public class GroupConcatFunction : SQLiteFunction
    {
        public override object Final(object contextData)
        {
            return contextData != null ? contextData.ToString() : null;
        }

        public override void Step(object[] args, int stepNumber, ref object contextData)
        {
            if (contextData == null)
            {
                contextData = new GroupConcater(1 < args.Length ? args[1] : ',');
            }
            ((GroupConcater) contextData).Step(args[0]);
        }

        private class GroupConcater
        {
            private readonly string separator;
            private string text;

            public GroupConcater(object separator)
            {
                this.separator = IsDBNull(separator) ? "," : separator.ToString();
            }

            public void Step(object arg)
            {
                if (!IsDBNull(arg))
                {
                    text += string.Format("{0}{1}", arg, separator);
                }
            }

            public override string ToString()
            {
                if (string.IsNullOrEmpty(text)) return null;
                string result = text.Remove(text.Length - separator.Length, separator.Length);
                return !string.IsNullOrEmpty(result) ? result : null;
            }

            private bool IsDBNull(object arg)
            {
                return arg == null || DBNull.Value.Equals(arg);
            }
        }
    }

    [SQLiteFunction(Name = "group_concat", Arguments = 1, FuncType = FunctionType.Aggregate)]
    public class GroupConcatFunction2 : GroupConcatFunction
    {
    }
}