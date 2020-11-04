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
using System.Text;
using ASC.Common.Logging;
using ASC.Core;
using ASC.CRM.Core;
using ASC.ElasticSearch;
using Newtonsoft.Json.Linq;

namespace ASC.Web.CRM.Core.Search
{
    public class InfoWrapper : Wrapper
    {
        [ColumnLastModified("last_modifed_on")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnCondition("contact_id", 1)]
        public int ContactId { get; set; }

        [ColumnMeta("type", 2)]
        public int Type { get; set; }

        [Column("data", 3)]
        public string Data 
        {
            get
            {
                if (Type == (int)ContactInfoType.Address && !string.IsNullOrEmpty(data))
                {
                    try
                    {
                        var result = new StringBuilder();
                        var obj = JObject.Parse(data);
                        foreach (var o in obj.Values())
                        {
                            var val = o.ToString();
                            if(!string.IsNullOrEmpty(val))
                            {
                                result.AppendFormat("{0} ", val);
                            }
                        }
                        return result.ToString().TrimEnd();
                    }
                    catch (Exception e)
                    {
                        LogManager.GetLogger("ASC").Error("Index Contact Adrress Parse", e);
                    }

                    return "";
                }

                return data;
            }
            set
            {
                data = value;
            } 
        }

        protected override string Table { get { return "crm_contact_info"; } }

        private string data;

        public static implicit operator InfoWrapper(ContactInfo cf)
        {
            return new InfoWrapper
            {
                Id = cf.ID,
                ContactId = cf.ContactID,
                Data = cf.Data,
                Type = (int)cf.InfoType,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId
            };
        }
    }
}