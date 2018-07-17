using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Mail.Aggregator.Common
{
    public class CrmContactEntity
    {
        public int Id { get; set; }
        public EntityTypes Type { get; set; }

        public string EntityTypeName {
            get
            {
                switch (Type)
                {
                    case EntityTypes.Contact:
                        return CrmEntityTypeNames.contact;
                    case EntityTypes.Case:
                        return CrmEntityTypeNames.Case;
                    case EntityTypes.Opportunity:
                        return CrmEntityTypeNames.opportunity;
                    default:
                        throw new ArgumentException(string.Format("Invalid CrmEntityType: {0}", Type), "type");
                }
            }
        }

        public enum EntityTypes
        {
            Contact = 1,
            Case = 2,
            Opportunity = 3
        }

        public static class CrmEntityTypeNames
        {
            public const string contact = "contact";
            public const string Case = "case";
            public const string opportunity = "opportunity";
        }

        /*public static string StringName(this EntityTypes type)
        {
            switch (type)
            {
                case CrmContactEntity.EntityTypes.Contact:
                    return CrmEntityTypeNames.contact;
                case CrmContactEntity.EntityTypes.Case:
                    return CrmEntityTypeNames.Case;
                case CrmContactEntity.EntityTypes.Opportunity:
                    return CrmEntityTypeNames.opportunity;
                default:
                    throw new ArgumentException(String.Format("Invalid CrmEntityType: {0}", type), "type");
            }
        }*/
    }
}
