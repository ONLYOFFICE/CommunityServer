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
                            "price", "price"
                        },
                    };
            }
        }
    }
}