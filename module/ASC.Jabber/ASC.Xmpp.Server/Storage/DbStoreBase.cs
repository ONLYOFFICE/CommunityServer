/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Xmpp.Server.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ASC.Xmpp.Server.Storage
{
    public abstract class DbStoreBase : IConfigurable
    {
        private const int ATTEMPTS_COUNT = 2;
        protected static readonly int MESSAGE_COLUMN_LEN = (int)Math.Pow(2, 24) - 1;

        private string dbid;

        public virtual void Configure(IDictionary<string, string> properties)
        {
            if (!properties.ContainsKey("connectionStringName"))
            {
                throw new ConfigurationErrorsException("Cannot create database connection: no connectionString or connectionStringName properties.");
            }

            dbid = properties["connectionStringName"];
            using (var db = new DbManager(dbid, false))
            {
                if (!properties.ContainsKey("generateSchema") || Convert.ToBoolean(properties["generateSchema"]))
                {
                    var creates = GetCreateSchemaScript();
                    if (creates != null && creates.Any())
                    {
                        foreach (var c in creates)
                        {
                            db.ExecuteNonQuery(c);
                        }
                    }
                }
            }
        }


        protected virtual SqlCreate[] GetCreateSchemaScript()
        {
            return new SqlCreate[0];
        }


        protected List<object[]> ExecuteList(ISqlInstruction sql)
        {
            return ExecWithAttempts(db => db.ExecuteList(sql), ATTEMPTS_COUNT);
        }

        protected T ExecuteScalar<T>(ISqlInstruction sql)
        {
            return ExecWithAttempts(db => db.ExecuteScalar<T>(sql), ATTEMPTS_COUNT);
        }

        protected int ExecuteNonQuery(ISqlInstruction sql)
        {
            return ExecWithAttempts(db => db.ExecuteNonQuery(sql), ATTEMPTS_COUNT);
        }

        protected int ExecuteBatch(IEnumerable<ISqlInstruction> batch)
        {
            return ExecWithAttempts(db => db.ExecuteBatch(batch), ATTEMPTS_COUNT);
        }

        private T ExecWithAttempts<T>(Func<DbManager, T> action, int attempsCount)
        {
            var counter = 0;
            while(true)
            {
                try
                {
                    using (var db = new DbManager(dbid, false))
                    {
                        return action(db);
                    }
                }
                catch (Exception err)
                {
                    if (attempsCount <= ++counter || err is TimeoutException || err.InnerException is TimeoutException)
                    {
                        throw;
                    }
                }
            }
        }
    }
}