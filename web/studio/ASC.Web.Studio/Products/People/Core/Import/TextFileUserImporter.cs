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
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using ASC.Web.People.Classes;
using ASC.Web.People.Resources;

namespace ASC.Web.People.Core.Import
{
    public class FileParameters
    {
        public int Encode { get; set; }
        public int Separator { get; set; }
        public int Delimiter { get; set; }
        public int Position { get; set; }

        public string UserHeader { get; set; }

        public bool IsRaw { get; set; }
    }

    public class MultiFormatTextFileUserImporter : TextFileUserImporter
    {
        public MultiFormatTextFileUserImporter(Stream stream, FileParameters param)
            : base(stream, param)
        {
        }

        protected override ContactInfo GetExportedUser(string line, IDictionary<int, PropertyInfo> mappedProperties, int fieldsCount)
        {
            try
            {
                var address = new MailAddress(line);
                var info = new ContactInfo { Email = address.Address };

                if (!string.IsNullOrEmpty(address.DisplayName))
                {
                    if (address.DisplayName.Contains(' '))
                    {
                        info.FirstName = address.DisplayName.Split(' ')[0];
                        info.LastName = address.DisplayName.Split(' ')[1];
                    }
                    else
                    {
                        info.FirstName = address.DisplayName;
                    }
                }
                return info;

            }
            catch (Exception)
            {
                //thats bad. Failed to parse an address
            }
            return base.GetExportedUser(line, mappedProperties, fieldsCount);
        }
    }

    public class TextFileUserImporter : IUserImporter
    {
        private readonly Stream stream;

        protected Dictionary<string, string> NameMapping { get; set; }
        protected IList<string> ExcludeList { get; private set; }

        internal bool HasHeader { get; set; }

        internal string Separators { get; set; }
        internal string TextDelmiter { get; set; }
        internal string UserHeader { get; set; }

        internal int FirstPosition { get; set; }

        private Encoding Encoding { get; set; }

        public TextFileUserImporter(Stream stream, FileParameters param)
        {
            this.stream = stream;
            ExcludeList = new List<string> { "ID", "Status" };

            Encoding = EncodingParameters.GetById(param.Encode);
            Separators = SeparatorParameters.GetById(param.Separator);
            TextDelmiter = DelimiterParameters.GetById(param.Delimiter);

            FirstPosition = param.Position;
            UserHeader = param.UserHeader;
        }

        public List<Tuple<string[], string[]>> GetRawUsers()
        {
            var users = new List<Tuple<string[], string[]>>();
            var fileLines = new List<string>();

            if (stream != null)
            {
                using (var reader = new StreamReader(stream, Encoding))
                {
                    fileLines.AddRange(reader.ReadToEnd().Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
                }
            }

            for (var i = FirstPosition; i < fileLines.Count; i++)
            {
                users.Add(new Tuple<string[], string[]>(GetDataFields(fileLines[0]), GetDataFields(fileLines[i])));
            }

            return users;
        }

        public IEnumerable<ContactInfo> GetDiscoveredUsers()
        {
            var users = new List<ContactInfo>();
            var fileLines = new List<string>();
            var splitedHeader = new List<string>();
            var splitedFileLines = new List<string>();
            var replaceComplHeader = new List<string>();
            var columns = ContactInfo.GetColumns();

            if (stream != null)
            {
                using (var reader = new StreamReader(stream, Encoding))
                {
                    fileLines.AddRange(reader.ReadToEnd().Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
                }

                splitedHeader = new List<string>(fileLines[0].Split(new[] { Separators }, StringSplitOptions.None));

                replaceComplHeader = new List<string>(UserHeader.Split(new[] { "," }, StringSplitOptions.None));

                for (var i = 0; i < replaceComplHeader.Count; i++)
                {
                    splitedHeader[i] = replaceComplHeader[i].Replace(" ", "");
                }

                UserHeader = string.Join(Separators, splitedHeader);

                if (FirstPosition == 0)
                {
                    fileLines.Insert(FirstPosition, UserHeader);
                }
                else
                {
                    fileLines.Insert(FirstPosition - 1, UserHeader);
                }
            }

            if (0 < fileLines.Count)
            {
                var lastPosition = fileLines.Count;
                var mappedProperties = new Dictionary<int, PropertyInfo>();
                var infos = typeof(ContactInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var fieldsCount = GetFieldsMapping(fileLines[0], infos, mappedProperties);

                for (int i = FirstPosition; i < fileLines.Count; i++)
                {
                    users.Add(GetExportedUser(fileLines[i], mappedProperties, fieldsCount));
                }
            }

            return users;
        }

        protected virtual ContactInfo GetExportedUser(string line, IDictionary<int, PropertyInfo> mappedProperties, int fieldsCount)
        {
            var exportedUser = new ContactInfo();
            var dataFields = GetDataFields(line);
            for (int j = 0; j < Math.Min(fieldsCount, dataFields.Length); j++)
            {
                var propinfo = mappedProperties[j];
                if (propinfo != null)
                {
                    var value = ConvertFromString(dataFields[j], propinfo.PropertyType);
                    if (value != null)
                    {
                        value = Regex.Replace(value.ToString(), "(^')|(^\")|(\"$)|('$)", String.Empty);

                        propinfo.SetValue(exportedUser, value, null);
                    }
                }
            }

            try
            {
                if (string.IsNullOrEmpty(exportedUser.FirstName) && string.IsNullOrEmpty(exportedUser.LastName) && !string.IsNullOrEmpty(exportedUser.Email))
                {
                    var username = exportedUser.Email.Contains('@') ? exportedUser.Email.Substring(0, exportedUser.Email.IndexOf('@')) : exportedUser.Email;
                    if (username.Contains('.'))
                    {
                        exportedUser.FirstName = username.Split('.')[0];
                        exportedUser.LastName = username.Split('.')[1];
                    }
                }
            }
            catch { }

            return exportedUser;
        }

        private string[] GetDataFields(string line)
        {
            var regFormat = "[{0}](?=(?:[^\"]*" + TextDelmiter + "[^\"]*" + TextDelmiter + ")*(?![^\"]*" + TextDelmiter + "))";
            var pattern = String.Format(regFormat, string.Join("|", Separators));
            var result = Regex.Split(line, pattern);

            return result.Select(
                original => original.StartsWith(TextDelmiter) && original.EndsWith(TextDelmiter)
                    ? original.Substring(1, original.Length - 2).Trim()
                    : original.Trim()
            ).ToArray();
        }

        private int GetFieldsMapping(string firstLine, IEnumerable<PropertyInfo> infos, IDictionary<int, PropertyInfo> mappedProperties)
        {
            var fields = firstLine.Split(new string[] { Separators }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                foreach (var info in infos)
                {
                    var propertyField = field.Trim();
                    propertyField = propertyField.Trim(Convert.ToChar(TextDelmiter));
                    var title = info.GetCustomAttribute<ResourceAttribute>().Title;

                    if (NameMapping != null && NameMapping.ContainsKey(propertyField))
                    {
                        propertyField = NameMapping[propertyField];
                    }

                    propertyField.Replace(" ", "");

                    if (!string.IsNullOrEmpty(propertyField) 
                        && !ExcludeList.Contains(propertyField) 
                        &&(propertyField.Equals(PeopleResource.ResourceManager.GetString(title), StringComparison.OrdinalIgnoreCase)
                            || propertyField.Equals(info.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        mappedProperties.Add(i, info);
                    }
                }

                if (!mappedProperties.ContainsKey(i))
                {
                    mappedProperties.Add(i, null);
                }
            }

            return fields.Length;
        }

        private static object ConvertFromString(string value, Type type)
        {
            var converter = TypeDescriptor.GetConverter(type);
            return converter.CanConvertFrom(typeof(string)) ? converter.ConvertFromString(value) : null;
        }
    }
}