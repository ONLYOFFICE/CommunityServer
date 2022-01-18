/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Web.Studio.Core;

namespace ASC.Specific.WarmupApi
{
    public class WarmUpEntryPoint : IApiEntryPoint
    {
        /// <summary>
        /// Entry point name
        /// </summary>
        public string Name
        {
            get { return "warmup"; }
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="context"></param>
        public WarmUpEntryPoint(ApiContext context)
        {
        }

        /// <summary>
        /// Request of warmup progress
        /// </summary>
        /// <visible>false</visible>
        [Read(@"progress", false, false)] //NOTE: this method doesn't requires auth!!!
        public string GetWarmupProgress()
        {
            try
            {
                return WarmUp.Instance.GetSerializedProgress();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <visible>false</visible>
        [Read(@"restart", false, false)] //NOTE: this method doesn't requires auth!!!
        public string Restart()
        {
            try
            {
                WarmUp.Instance.Restart();
                return "Ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
