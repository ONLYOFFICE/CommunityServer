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

namespace ASC.ElasticSearch
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class JoinAttribute : Attribute
    {
        public JoinTypeEnum JoinType { get; private set; }
        public string[] ColumnsFrom { get; private set; }
        public string[] ColumnsTo { get; private set; }

        public JoinAttribute(JoinTypeEnum joinType, params string[] columns)
        {
            JoinType = joinType;
            ColumnsFrom = new string[columns.Length];
            ColumnsTo = new string[columns.Length];

            for (var i = 0; i< columns.Length; i++)
            {
                var column = columns[i].Split(':');
                ColumnsFrom[i] = column[0];
                ColumnsTo[i] = column[1];
            }
        }
    }

    public enum JoinTypeEnum
    {
        Inner,
        Sub
    }
}
