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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ASC.Core.Users;

namespace ASC.Web.Core.Users.Import
{
    public class TextFileUserImporter : IUserImporter
    {
        private readonly Stream stream;

        protected Dictionary<string, string> NameMapping { get; set; }

        protected IList<string> ExcludeList { get; private set; }


        public Encoding Encoding { get; set; }

        public char Separator { get; set; }

        public bool HasHeader { get; set; }

        public string TextDelmiter { get; set; }

        public string DefaultHeader { get; set; }


        public TextFileUserImporter(Stream stream)
        {
            this.stream = stream;
            Encoding = Encoding.UTF8;
            Separator = ';';
            HasHeader = false;
            TextDelmiter = "\"";
            ExcludeList = new List<string> { "ID", "Status" };
        }


        public virtual IEnumerable<UserInfo> GetDiscoveredUsers()
        {
            var users = new List<UserInfo>();

            var fileLines = new List<string>();
            using (var reader = new StreamReader(stream, Encoding, true))
            {
                fileLines.AddRange(reader.ReadToEnd().Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            }

            if (!string.IsNullOrEmpty(DefaultHeader))
            {
                fileLines.Insert(0, DefaultHeader);
            }
            if (0 < fileLines.Count)
            {
                var mappedProperties = new Dictionary<int, PropertyInfo>();
                //Get the map
                var infos = typeof(UserInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var fieldsCount = GetFieldsMapping(fileLines[0], infos, mappedProperties);

                //Begin read file
                for (int i = 1; i < fileLines.Count; i++)
                {
                    users.Add(GetExportedUser(fileLines[i], mappedProperties, fieldsCount));
                }
            }
            return users;
        }

        private UserInfo GetExportedUser(string line, IDictionary<int, PropertyInfo> mappedProperties, int fieldsCount)
        {
            var exportedUser = new UserInfo();
            exportedUser.ID = Guid.NewGuid();

            var dataFields = GetDataFields(line);
            for (int j = 0; j < Math.Min(fieldsCount, dataFields.Length); j++)
            {
                //Get corresponding property
                var propinfo = mappedProperties[j];
                if (propinfo != null)
                {
                    //Convert value
                    object value = ConvertFromString(dataFields[j], propinfo.PropertyType);
                    if (value != null)
                    {
                        propinfo.SetValue(exportedUser, value, new object[] { });
                    }
                }
            }
            return exportedUser;
        }

        private string[] GetDataFields(string line)
        {
            var pattern = String.Format("{0}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))", Separator);
            var result = Regex.Split(line, pattern);

            //remove TextDelmiter
            result = Array.ConvertAll<string, string>(result,
                original =>
                {
                    if (original.StartsWith(TextDelmiter) && original.EndsWith(TextDelmiter))
                        return original.Substring(1, original.Length - 2);
                    return original;
                }
             );

            return result;
        }

        private int GetFieldsMapping(string firstLine, IEnumerable<PropertyInfo> infos, IDictionary<int, PropertyInfo> mappedProperties)
        {
            var fields = firstLine.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                //Find apropriate field in UserInfo
                foreach (var info in infos)
                {
                    var propertyField = field.Trim();
                    if (NameMapping != null && NameMapping.ContainsKey(propertyField))
                    {
                        propertyField = NameMapping[propertyField];
                    }
                    if (!string.IsNullOrEmpty(propertyField) && !ExcludeList.Contains(propertyField) && propertyField.Equals(info.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        //Add to map
                        mappedProperties.Add(i, info);
                    }
                }
                if (!mappedProperties.ContainsKey(i))
                {
                    //No property was found
                    mappedProperties.Add(i, null);
                }
            }
            return fields.Length;
        }

        private static object ConvertFromString(string value, Type type)
        {
            var converter = TypeDescriptor.GetConverter(type);
            return converter != null && converter.CanConvertFrom(typeof(string)) ? converter.ConvertFromString(value) : null;
        }
    }
}