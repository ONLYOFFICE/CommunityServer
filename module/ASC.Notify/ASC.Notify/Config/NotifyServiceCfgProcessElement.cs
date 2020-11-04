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
using System;

namespace ASC.Notify.Config
{
    public class NotifyServiceCfgProcessElement : ConfigurationElement
    {
        [ConfigurationProperty("maxThreads")]
        public int MaxThreads
        {
            get { return (int)base["maxThreads"]; }
        }

        [ConfigurationProperty("bufferSize", DefaultValue = 10)]
        public int BufferSize
        {
            get { return (int)base["bufferSize"]; }
        }

        [ConfigurationProperty("maxAttempts", DefaultValue = 10)]
        public int MaxAttempts
        {
            get { return (int)base["maxAttempts"]; }
        }

        [ConfigurationProperty("attemptsInterval", DefaultValue = "0:5:0")]
        public TimeSpan AttemptsInterval
        {
            get { return (TimeSpan)base["attemptsInterval"]; }
        }
    }
}
