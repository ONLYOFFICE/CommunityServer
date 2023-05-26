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

using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Web.Studio.Core;

namespace ASC.Specific.WarmupApi
{
    public class WarmUpEntryPoint : IApiEntryPoint
    {
        /// <summary>
        /// Warmup API.
        /// </summary>
        /// <name>warmup</name>
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
        /// Returns the warmup progress.
        /// </summary>
        /// <short>
        /// Get warmup progress
        /// </short>
        /// <returns>Warmup progress</returns>
        /// <path>api/2.0/warmup/progress</path>
        /// <httpMethod>GET</httpMethod>
        /// <requiresAuthorization>false</requiresAuthorization>
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

        /// <summary>
        /// Restarts the warmup process.
        /// </summary>
        /// <short>
        /// Restart warmup
        /// </short>
        /// <returns>The "Ok" message if the operation is successful</returns>
        /// <path>api/2.0/warmup/restart</path>
        /// <httpMethod>GET</httpMethod>
        /// <requiresAuthorization>false</requiresAuthorization>
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
