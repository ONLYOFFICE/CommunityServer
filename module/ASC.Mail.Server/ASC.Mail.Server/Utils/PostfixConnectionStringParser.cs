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
using ASC.Mail.Server.Exceptions;
using Newtonsoft.Json.Linq;

namespace ASC.Mail.Server.Utils
{
    public class PostfixConnectionStringParser
    {
        public PostfixConnectionStringParser(string connectionString)
        {
            Parse(connectionString);
        }

        private void Parse(string connectionString)
        {
            try
            {
                var parsedConnection = JObject.Parse(connectionString);
                PostfixAdminDbConnectionString = parsedConnection["DbConnection"].ToString();
            }
            catch (NullReferenceException)
            {
                throw new InvalidPostfixConnectionStringException("Invalid connection string. Some keys wasn't founded");
            }
            catch (Exception)
            {
                throw new InvalidPostfixConnectionStringException("Invalid connection string");
            }
        }

        public string PostfixAdminDbConnectionString { get; private set; }
    }
}
