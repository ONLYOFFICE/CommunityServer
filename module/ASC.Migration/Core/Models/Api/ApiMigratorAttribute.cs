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

namespace ASC.Migration.Core.Models.Api
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ApiMigratorAttribute : Attribute
    {
        public string Name { get; private set; }
        public bool RequiresFolder { get; private set; }
        public int NumberOfSteps { get; private set; }
        public bool ArchivesIsMultiple { get; private set; }

        public string[] RequiredFileTypes { get; private set; }

        public ApiMigratorAttribute(string name, string[] fileTypes)
        {
            Name = name;
            RequiredFileTypes = fileTypes;
        }

        public ApiMigratorAttribute(string name, int numberOfSteps, bool archivesIsMultiple)
        {
            Name = name;
            NumberOfSteps = numberOfSteps;
            ArchivesIsMultiple = archivesIsMultiple;
        }

        public ApiMigratorAttribute(string name)
        {
            Name = name;
        }
    }
}
