/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.FullTextIndex.Service.Config;
using log4net;
using System;
using System.IO;
using System.Linq;

namespace ASC.FullTextIndex.Service
{
    class TextSearcherService : ITextIndexService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TextSearcherService));
        private readonly TextIndexCfg configuration;
        private readonly TenantsProvider tenantsProvider;


        public TextSearcherService()
        {
            log.Debug("TextSearcherService constructor: start");
            configuration = new TextIndexCfg();
            tenantsProvider = new TenantsProvider(configuration.ConnectionStringName, configuration.UserActivityDays);
            log.Debug("TextSearcherService constructor: end");
        }


        public bool SupportModule(string[] modules)
        {
            log.DebugFormat("SupportModule({0})", string.Join(",", modules ?? new string[0]));
            return modules != null && 0 < modules.Length && modules.All(m => configuration.Modules.Any(mi => mi.Name == m));
        }

        public string[] Search(int tenantId, string query, string module)
        {
            log.DebugFormat("Search({0}, {1}, {2})", tenantId, 50 < (query ?? string.Empty).Length ? query.Substring(0, 50) + "..." : query, module);

            try
            {
                if (string.IsNullOrEmpty(module))
                {
                    throw new ArgumentNullException("module");
                }
                if (string.IsNullOrEmpty(query))
                {
                    return new string[0];
                }
                if (TextIndexCfg.MaxQueryLength < query.Length)
                {
                    query = query.Substring(0, TextIndexCfg.MaxQueryLength);
                }

                var tenant = tenantsProvider.GetTenant(tenantId);
                var path = configuration.GetIndexPath(tenantId, module);
                if (tenant == null || !Directory.Exists(path))
                {
                    return new string[0];
                }

                var searcher = new TextSearcher(module, path);
                return searcher.Search(query, tenant);
            }
            catch (Lucene.Net.QueryParsers.ParseException ex)
            {
                log.ErrorFormat("Search error: query = {0}, tenantId = {1}, module = {2} - {3}", query, tenantId, module, ex);
                throw new ArgumentException(ex.Message);
            }
        }
    }
}
