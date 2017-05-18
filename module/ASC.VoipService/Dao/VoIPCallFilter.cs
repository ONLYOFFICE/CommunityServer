/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections.Generic;

namespace ASC.VoipService.Dao
{
    public class VoipCallFilter
    {
        public string Type { get; set; }

        public DateTime? FromDate { get; set; }
        
        public DateTime? ToDate { get; set; }

        public Guid? Agent { get; set; }

        public int? Client { get; set; }

        public int? ContactID { get; set; }

        public string Id { get; set; }

        public string ParentId { get; set; }

        public string SortBy { get; set; }

        public bool SortOrder { get; set; }

        public string SearchText { get; set; }

        public long Offset { get; set; }

        public long Max { get; set; }

        public int? TypeStatus
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Type)) return null;
                if (TypeStatuses.ContainsKey(Type)) return TypeStatuses[Type];

                return null;
            }
        }

        public string SortByColumn
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SortBy)) return null;
                return SortColumns.ContainsKey(SortBy) ? SortColumns[SortBy] : null;
            }
        }

        private static Dictionary<string, int> TypeStatuses
        {
            get
            {
                return new Dictionary<string, int>
                    {
                        {
                            "answered", (int)VoipCallStatus.Answered
                        },
                        {
                            "missed", (int)VoipCallStatus.Missed
                        },
                        {
                            "outgoing", (int)VoipCallStatus.Outcoming
                        }
                    };
            }
        }

        private static Dictionary<string, string> SortColumns
        {
            get
            {
                return new Dictionary<string, string>
                    {
                        {
                            "date", "dial_date"
                        },
                        {
                            "duration", "dial_duration"
                        },
                        {
                            "cost", "dial_duration"
                        },
                    };
            }
        }
    }
}