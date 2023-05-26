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
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using ASC.Common.Data.Sql;

namespace ASC.Common.Data
{
    public static class DataExtensions
    {
        public static DbCommand CreateCommand(this DbConnection connection, string sql, params object[] parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.AddParameters(parameters);
            return command;
        }


        public static DbCommand AddParameter(this DbCommand command, string name, object value)
        {
            var p = command.CreateParameter();
            if (!string.IsNullOrEmpty(name))
            {
                p.ParameterName = name.StartsWith("@") ? name : "@" + name;
            }

            p.Value = GetParameterValue(value);

            command.Parameters.Add(p);
            return command;
        }

        public static object GetParameterValue(object value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            var @enum = value as Enum;
            if (@enum != null)
            {
                return @enum.ToString("d");
            }

            if (value is DateTime)
            {
                var d = (DateTime)value;
                return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, DateTimeKind.Unspecified);
            }
            return value;
        }

        public static DbCommand AddParameters(this DbCommand command, params object[] parameters)
        {
            if (parameters == null) return command;
            foreach (var value in parameters)
            {
                if (value != null && IsAnonymousType(value.GetType()))
                {
                    foreach (var p in value.GetType().GetProperties())
                    {
                        command.AddParameter(p.Name, p.GetValue(value, null));
                    }
                }
                else
                {
                    command.AddParameter(null, value);
                }
            }
            return command;
        }

        public static List<object[]> ExecuteList(this DbCommand command)
        {
            return ExecuteList(command, command.CommandText, null);
        }

        public static Task<List<object[]>> ExecuteListAsync(this DbCommand command)
        {
            return ExecuteListAsync(command, command.CommandText, null);
        }

        public static List<object[]> ExecuteList(this DbCommand command, string sql, params object[] parameters)
        {
            return ExecuteListReader(command.PrepareCommand(sql, parameters));
        }

        public static Task<List<object[]>> ExecuteListAsync(this DbCommand command, string sql, params object[] parameters)
        {
            return ExecuteListReaderAsync(command.PrepareCommand(sql, parameters));
        }

        private static List<object[]> ExecuteListReader(DbCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                return ExecuteListReaderResult(reader);
            }
        }

        private static async Task<List<object[]>> ExecuteListReaderAsync(DbCommand command)
        {
            using (var reader = await command.ExecuteReaderAsync())
            {
                return ExecuteListReaderResult(reader);
            }
        }

        private static List<object[]> ExecuteListReaderResult(IDataReader reader)
        {
            var result = new List<object[]>();
            var fieldCount = reader.FieldCount;
            while (reader.Read())
            {
                var row = new object[fieldCount];
                for (var i = 0; i < fieldCount; i++)
                {
                    row[i] = reader[i];
                    if (DBNull.Value.Equals(row[i])) row[i] = null;
                }
                result.Add(row);
            }
            return result;
        }

        public static T ExecuteScalar<T>(this DbCommand command)
        {
            return ExecuteScalar<T>(command, command.CommandText, null);
        }

        public static T ExecuteScalar<T>(this DbCommand command, string sql, params object[] parameters)
        {
            command.PrepareCommand(sql, parameters);

            var scalar = command.ExecuteScalar();

            if (scalar == null || scalar == DBNull.Value)
            {
                return default(T);
            }
            var scalarType = typeof(T);
            if (scalarType == typeof(object))
            {
                return (T)scalar;
            }
            if (scalarType.Name == "Nullable`1")
            {
                scalarType = scalarType.GetGenericArguments()[0];
            }
            return (T)Convert.ChangeType(scalar, scalarType);
        }

        public static int ExecuteNonQuery(this DbCommand command, string sql, params object[] parameters)
        {
            command.PrepareCommand(sql, parameters);
            return command.ExecuteNonQuery();
        }

        public static Task<int> ExecuteNonQueryAsync(this DbCommand command, string sql, params object[] parameters)
        {
            command.PrepareCommand(sql, parameters);
            return command.ExecuteNonQueryAsync();
        }

        public static List<object[]> ExecuteList(this DbManager manager, ISqlInstruction sql, ISqlDialect dialect)
        {
            var command = manager.ApplySqlInstruction(sql, dialect);
            return command.ExecuteList();
        }

        public static Task<List<object[]>> ExecuteListAsync(this DbManager manager, ISqlInstruction sql, ISqlDialect dialect)
        {
            var command = manager.ApplySqlInstruction(sql, dialect);
            return command.ExecuteListAsync();
        }

        public static List<T> ExecuteList<T>(this DbManager manager, ISqlInstruction sql, ISqlDialect dialect, Converter<IDataRecord, T> mapper)
        {
            var command = manager.ApplySqlInstruction(sql, dialect);
            var result = new List<T>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(mapper(reader));
                }
            }
            return result;
        }

        public static T ExecuteScalar<T>(this DbManager manager, ISqlInstruction sql, ISqlDialect dialect)
        {
            var command = manager.ApplySqlInstruction(sql, dialect);
            return command.ExecuteScalar<T>();
        }

        public static int ExecuteNonQuery(this IDbManager manager, ISqlInstruction sql, ISqlDialect dialect)
        {
            var command = ApplySqlInstruction(manager, sql, dialect);
            return command.ExecuteNonQuery();
        }

        private static DbCommand ApplySqlInstruction(this IDbManager manager, ISqlInstruction sql, ISqlDialect dialect)
        {
            var sqlStr = sql.ToString(dialect);
            var parameters = sql.GetParameters();

            var sqlParts = sqlStr.Split('?');
            var sqlBuilder = new StringBuilder();
            var i = 0;

            var commandParameters = new Dictionary<string, object>();

            foreach (var p in parameters)
            {
                var name = $"p{i}";
                sqlBuilder.AppendFormat("{0}@{1}", sqlParts[i], name);
                commandParameters.Add(name, p);
                i++;
            }

            sqlBuilder.Append(sqlParts[sqlParts.Length - 1]);

            var command = manager.Command;
            command.Parameters.Clear();
            foreach (var p in commandParameters)
            {
                command.AddParameter(p.Key, p.Value);
            }
            command.CommandText = sqlBuilder.ToString();
            return command;
        }


        public static T Get<T>(this IDataRecord r, int i)
        {
            if (r.IsDBNull(i))
            {
                return default(T);
            }

            var value = r.GetValue(i);
            if (typeof(T) == typeof(Guid))
            {
                value = r.GetGuid(i);
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static T Get<T>(this IDataRecord r, string name)
        {
            return Get<T>(r, r.GetOrdinal(name));
        }

        private static bool IsAnonymousType(Type type)
        {
            return type.IsGenericType
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic
                   && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                   && (type.Name.Contains("AnonymousType") || type.Name.Contains("AnonType"))
                   && Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false);
        }

        private static DbCommand PrepareCommand(this DbCommand command, string sql, params object[] parameters)
        {
            command.CommandText = sql;
            if (parameters != null)
            {
                command.Parameters.Clear();
                command.AddParameters(parameters);
            }
            return command;
        }
    }
}