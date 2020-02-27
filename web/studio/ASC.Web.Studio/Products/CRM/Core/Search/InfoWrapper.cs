/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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