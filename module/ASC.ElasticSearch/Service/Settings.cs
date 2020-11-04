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
using ASC.ElasticSearch.Config;

namespace ASC.ElasticSearch.Service
{
    public class Settings
    {
        private static readonly Settings DefaultSettings;

        static Settings()
        {
            DefaultSettings = new Settings
            {
                Scheme = "http",
                Host = "localhost",
                Port = 9200,
                Period = 1,
                MemoryLimit = 10 * 1024 * 1024L
            };


            var cfg = ConfigurationManagerExtension.GetSection("elastic") as ElasticSection;
            if (cfg == null) return;

            DefaultSettings = new Settings
            {
                Scheme = cfg.Scheme,
                Host = cfg.Host,
                Port = cfg.Port,
                Period = cfg.Period,
                MemoryLimit = cfg.MemoryLimit
            };

        }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Scheme { get; set; }

        public int Period { get; set; }

        public long MemoryLimit { get; set; }

        public static Settings Default
        {
            get { return DefaultSettings; }
        }
    }
}