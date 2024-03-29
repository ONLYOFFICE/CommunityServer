/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Text;

namespace ASC.Common.Data.Sql
{
    class SqlDebugView
    {
        private readonly ISqlInstruction instruction;


        public string Sql
        {
            get { return GetSqlWithParameters(); }
        }

        public IEnumerable<object> Parameters
        {
            get { return instruction.GetParameters(); }
        }


        public SqlDebugView(ISqlInstruction instruction)
        {
            this.instruction = instruction;
        }

        private string GetSqlWithParameters()
        {
            var sql = instruction.ToString();
            var parameters = instruction.GetParameters();
            var sb = new StringBuilder();
            var i = 0;
            var sqlParts = sql.Split('?');
            var pCount = parameters.Count();

            foreach (var p in parameters)
            {
                sb.Append(sqlParts[i]);
                if (i < pCount)
                {
                    if (p == null)
                    {
                        sb.Append("null");
                    }
                    else if (p is string || p is char || p is DateTime || p is Guid)
                    {
                        sb.AppendFormat("'{0}'", p);
                    }
                    else
                    {
                        sb.Append(p);
                    }
                }
                i++;
            }
            return sb.ToString();
        }
    }
}