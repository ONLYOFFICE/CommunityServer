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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;

namespace ASC.Web.Studio.UserControls.Common.ThirdPartyBanner
{
    [Serializable]
    [DataContract]
    public class ThirdPartyBannerSettings : BaseSettings<ThirdPartyBannerSettings>
    {
        [DataMember(Name = "ClosedSetting")]
        public string ClosedSetting { get; set; }

        public override ISettings GetDefault()
        {
            return new ThirdPartyBannerSettings
                {
                    ClosedSetting = null,
                };
        }

        public override Guid ID
        {
            get { return new Guid("{B6E9B080-4B14-4C54-95E7-C2E9E87965EB}"); }
        }

        public static bool CheckClosed(string banner)
        {
            return (LoadForCurrentUser().ClosedSetting ?? "").Split('|').Contains(banner);
        }

        public static void Close(string banner)
        {
            var closed = (LoadForCurrentUser().ClosedSetting ?? "").Split('|').ToList();
            if (closed.Contains(banner)) return;
            closed.Add(banner);

            new ThirdPartyBannerSettings { ClosedSetting = string.Join("|", closed.ToArray()) }.SaveForCurrentUser();
        }
    }
}