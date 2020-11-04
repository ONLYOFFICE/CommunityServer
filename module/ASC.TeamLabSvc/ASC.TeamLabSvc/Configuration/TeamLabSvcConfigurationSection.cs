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


using System.Configuration;

namespace ASC.TeamLabSvc.Configuration
{
    class TeamLabSvcConfigurationSection : ConfigurationSection
    {
        public const string SECTION_NAME = "teamlab";

        public const string SERVICES = "services";

        public const string TYPE = "type";


        [ConfigurationProperty(SERVICES)]
        public TeamLabSvcConfigurationCollection TeamlabServices
        {
            get { return (TeamLabSvcConfigurationCollection)this[SERVICES]; }
        }


        public static TeamLabSvcConfigurationSection GetSection()
        {
            return (TeamLabSvcConfigurationSection)ConfigurationManagerExtension.GetSection(TeamLabSvcConfigurationSection.SECTION_NAME);
        }
    }
}
