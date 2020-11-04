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


using ASC.Web.CRM.Core.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ASC.Web.CRM.Classes
{
    public class ImportCSVSettings
    {
        public ImportCSVSettings(String jsonStr)
        {
            var json = JObject.Parse(jsonStr);

            if (json == null) return;

            HasHeader = json["has_header"].Value<bool>();
            DelimiterCharacter = Convert.ToChar(json["delimiter_character"].Value<int>());
            Encoding = Encoding.GetEncoding(json["encoding"].Value<int>());
            QuoteType = Convert.ToChar(json["quote_character"].Value<int>());

            JToken columnMappingToken;

            if (json.TryGetValue("column_mapping", out columnMappingToken))
                ColumnMapping = (JObject)columnMappingToken;

            JToken duplicateRecordRuleToken;

            if (json.TryGetValue("removing_duplicates_behavior", out duplicateRecordRuleToken))
                DuplicateRecordRule = duplicateRecordRuleToken.Value<int>();

            JToken isPrivateToken;
            if (json.TryGetValue("is_private", out isPrivateToken))
            {
                IsPrivate = isPrivateToken.Value<bool>();
                AccessList = json["access_list"].Values<String>().Select(item => new Guid(item)).ToList();
            }

            JToken shareTypeToken;
            if (json.TryGetValue("share_type", out shareTypeToken))
            {
                ShareType = (ShareType)(shareTypeToken.Value<int>());
                ContactManagers = json["contact_managers"].Values<String>().Select(item => new Guid(item)).ToList();
            }

            if (json["tags"] != null)
            {
                Tags = json["tags"].Values<String>().ToList();
            }
        }

        public bool IsPrivate { get; set; }

        public ShareType ShareType { get; set; }

        public int DuplicateRecordRule { get; set; }

        public bool HasHeader { get; set; }

        public char DelimiterCharacter { get; set; }

        public char QuoteType { get; set; }

        public Encoding Encoding { get; set; }

        public List<Guid> AccessList { get; set; }

        public List<Guid> ContactManagers { get; set; }

        public JObject ColumnMapping { get; set; }

        public List<String> Tags { get; set; }
    }

}