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
using System.Linq;
using System.Runtime.Serialization;
using AjaxPro;
using ASC.Core.Common.Settings;

namespace ASC.Web.Studio.Core.HelpCenter
{
    [AjaxNamespace("UserVideoGuideUsage")]
    [Serializable]
    [DataContract]
    public class UserVideoSettings : BaseSettings<UserVideoSettings>
    {
        public override Guid ID
        {
            get { return new Guid("{CEBD4BA5-31B3-43a4-93BF-B4A110FE840F}"); }
        }

        [DataMember(Name = "VideoGuides")]
        private List<String> VideoGuides { get; set; }

        public override ISettings GetDefault()
        {
            return new UserVideoSettings
                {
                    VideoGuides = new List<string>()
                };
        }

        public static List<string> GetUserVideoGuide()
        {
            return LoadForCurrentUser().VideoGuides ?? new List<string>();
        }

        [AjaxMethod]
        public void SaveWatchVideo(String[] video)
        {
            var watched = GetUserVideoGuide();
            watched.AddRange(video);
            watched = watched.Distinct().ToList();

            new UserVideoSettings { VideoGuides = watched }.SaveForCurrentUser();
        }
    }
}