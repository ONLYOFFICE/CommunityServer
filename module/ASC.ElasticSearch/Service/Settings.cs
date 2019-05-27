/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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


            var cfg = ConfigurationManager.GetSection("elastic") as ElasticSection;
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