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
using System.Data;
using System.Data.Common;
using System.Text;

namespace ASC.Common.Data.AdoProxy
{
    class ExecutedEventArgs : EventArgs
    {
        public TimeSpan Duration { get; private set; }

        public string SqlMethod { get; private set; }

        public string Sql { get; private set; }

        public string SqlParameters { get; private set; }


        public ExecutedEventArgs(string method, TimeSpan duration)
            : this(method, duration, null)
        {

        }

        public ExecutedEventArgs(string method, TimeSpan duration, DbCommand command)
        {
            SqlMethod = method;
            Duration = duration;
            if (command != null)
            {
                Sql = command.CommandText;
                
                if (0 < command.Parameters.Count)
                {
                    var stringBuilder = new StringBuilder();
                    foreach (IDbDataParameter p in command.Parameters)
                    {
                        if (!string.IsNullOrEmpty(p.ParameterName)) stringBuilder.AppendFormat("{0}=", p.ParameterName);
                        stringBuilder.AppendFormat("{0}, ", p.Value == null || DBNull.Value.Equals(p.Value) ? "NULL" : p.Value.ToString());
                    }
                    SqlParameters = stringBuilder.ToString(0, stringBuilder.Length - 2);
                }
            }
        }
    }
}