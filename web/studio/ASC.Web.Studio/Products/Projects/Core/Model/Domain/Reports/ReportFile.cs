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

namespace ASC.Projects.Core.Domain.Reports
{
    public class ReportFile
    {
        ///<example type="int">123</example>
        public int Id { get; set; }

        ///<example>Name</example>
        public string Name { get; set; }

        ///<example type="int">1</example>
        public int ReportTemplate { get; set; }

        ///<example>92eb52c8-bb93-4caf-87fb-46ea11530899</example>
        public Guid CreateBy { get; set; }

        ///<example>2019-07-26T00:00:00</example>
        public DateTime CreateOn { get; set; }

        ///<example type="int">1</example>
        public ReportType ReportType { get; set; }

        ///<example>null</example>
        public object FileId { get; set; }
    }
}