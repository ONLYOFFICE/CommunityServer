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


using System.Data;
using System.Data.Common;

namespace ASC.Data.Backup.Extensions
{
    public static class DataExtensions
    {
        public static DbCommand WithTimeout(this DbCommand command, int timeout)
        {
            if (command != null)
            {
                command.CommandTimeout = timeout;
            }
            return command;
        }

        public static DbConnection Fix(this DbConnection connection)
        {
            if (connection != null && connection.State != ConnectionState.Open)
            {
                connection.Close();
                connection.Open();
            }
            return connection;
        }
    }
}
