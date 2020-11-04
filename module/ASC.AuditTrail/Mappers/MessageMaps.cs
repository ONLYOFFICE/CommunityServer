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


namespace ASC.AuditTrail.Mappers
{
    internal class MessageMaps
    {
        public string ActionTypeTextResourceName { get; set; }
        public string ActionTextResourceName { get; set; }
        public string ProductResourceName { get; set; }
        public string ModuleResourceName { get; set; }

        public string GetActionTypeText()
        {
            try
            {
                return AuditReportResource.ResourceManager.GetString(ActionTypeTextResourceName);
            }
            catch
            {
                return null;
            }
        }

        public string GetActionText()
        {
            try
            {
                return AuditReportResource.ResourceManager.GetString(ActionTextResourceName);
            }
            catch
            {
                return null;
            }
        }

        public string GetProduct()
        {
            try
            {
                return AuditReportResource.ResourceManager.GetString(ProductResourceName);
            }
            catch
            {
                return null;
            }
        }

        public string GetModule()
        {
            try
            {
                return AuditReportResource.ResourceManager.GetString(ModuleResourceName);
            }
            catch
            {
                return null;
            }
        }
    }
}