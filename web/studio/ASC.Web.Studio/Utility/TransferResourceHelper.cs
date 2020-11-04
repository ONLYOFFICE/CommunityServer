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


namespace ASC.Web.Studio.Utility
{
    public static class TransferResourceHelper
    {
        public static string GetRegionDescription(string region)
        {
            region = region.ToLower().Trim();

            switch (region)
            {
                case "eu": return Resources.Resource.EuServerRegion;
                case "us": return Resources.Resource.UsServerRegion;
                case "sg": return Resources.Resource.SgServerRegion;
                case "org": return Resources.Resource.OrgServerRegion;
            }

            return string.Empty;
        }
    }
}
