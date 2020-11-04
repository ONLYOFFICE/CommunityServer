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

namespace ASC.Common.Data.Sql
{
    class SqlInstructionDebuggerDisplay
    {
        private readonly ISqlInstruction i;


        public SqlInstructionDebuggerDisplay(ISqlInstruction i)
        {
            this.i = i;
        }

        public override string ToString()
        {
            if (i == null) return null;

            var parts = i.ToString().Split('?');
            var result = new StringBuilder(parts[0]);
            var counter = 0;
            foreach (var p in i.GetParameters())
            {
                counter++;
                if (p == null)
                {
                    result.Append("null");
                }
                else
                {
                    var format = "{0}";
                    if (p is DateTime || p is Guid || p is string) format = "'{0}'";
                    result.AppendFormat(format, p);
                }
                if (counter < parts.Length) result.Append(parts[counter]);
            }
            return result.ToString();
        }
    }
}
