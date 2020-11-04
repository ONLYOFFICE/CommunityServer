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
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;

namespace ASC.Web.Studio.Core
{
    [Serializable]
    [DataContract]
    public class StudioDefaultPageSettings : BaseSettings<StudioDefaultPageSettings>
    {
        [DataMember(Name = "DefaultProductID")]
        public Guid DefaultProductID { get; set; }

        public override Guid ID
        {
            get { return new Guid("{F3FF27C5-BDE3-43ae-8DD0-2E8E0D7044F1}"); }
        }

        public Guid FeedModuleID
        {
            get { return new Guid("{48328C27-4C85-4987-BA0E-D6BB17356B10}"); }
        }

        public override ISettings GetDefault()
        {
            return new StudioDefaultPageSettings { DefaultProductID = Guid.Empty };
        }
    }
}
