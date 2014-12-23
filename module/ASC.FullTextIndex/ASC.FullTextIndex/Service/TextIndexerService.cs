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
using System.Threading;

namespace ASC.FullTextIndex.Service
{
    class TextIndexerService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TextIndexerService));

        private readonly TextIndexCfg configuration;
        private readonly TenantsProvider tenantsProvider;

        private readonly Thread worker;
        private readonly ManualResetEvent stop;


        public TextIndexerService()
        {
            configuration = new TextIndexCfg();
            tenantsProvider = new TenantsProvider(configuration.ConnectionStringName, configuration.UserActivityDays);

            worker = new Thread(DoWork) { Priority = ThreadPriority.Lowest, Name = "Full Text Indexer", };
            stop = new ManualResetEvent(false);
        }


        public void Start()
        {
            worker.Start();
        }

        public void Stop()
        {
            stop.Set();
            worker.Join();
            stop.Close();
        }


        private void DoWork()
        {
            var period = TimeSpan.FromSeconds(1);
            var action = TextIndexAction.None;
            do
            {
                try
                {
                    if (stop.WaitOne(period))
                    {
                        return;
                    }

                    DoIndex(action);

                    var now = DateTime.UtcNow;
                    var indexDateTime = configuration.ChangedCron.GetTimeAfter(now) ?? DateTime.MaxValue;
                    var removeDateTime = configuration.RemovedCron.GetTimeAfter(now) ?? DateTime.MaxValue;

                    action = TextIndexAction.Index | TextIndexAction.Remove;
                    if (indexDateTime < removeDateTime) action = TextIndexAction.Index;
                    if (indexDateTime > removeDateTime) action = TextIndexAction.Remove;

                    period = ((indexDateTime < removeDateTime ? indexDateTime : removeDateTime) - now).Add(TimeSpan.FromSeconds(1));

                    log.DebugFormat("Next action '{0}' over {1}", action, period);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error in DoIndex: {0}", ex);
                    period = TimeSpan.FromSeconds(5);
                }
            }
            while (true);
        }

        private void DoIndex(TextIndexAction action)
        {
            if (action == TextIndexAction.None) return;
            foreach (var t in tenantsProvider.GetTenants())
            {
                foreach (var m in configuration.Modules)
                {
                    if (stop.WaitOne(TimeSpan.Zero))
                    {
                        return;
                    }

                    var indexPath = configuration.GetIndexPath(t.TenantId, m.Name);
                    var indexer = new TextIndexer(indexPath, t, m);
                    try
                    {
                        if (TextIndexAction.Index == (action & TextIndexAction.Index))
                        {
                            var affected = indexer.FindChangedAndIndex();
                            log.DebugFormat("Indexed {0} objects at tenant {1} in module {2}", affected, t, m);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error FindChangedAndIndex in tenant {0}: {1}", t, ex);
                    }

                    try
                    {
                        if (TextIndexAction.Remove == (action & TextIndexAction.Remove))
                        {
                            var affected = indexer.FindRemovedAndIndex();
                            log.DebugFormat("Removed {0} objects at tenant {1} in module {2}", affected, t, m);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error FindRemovedAndIndex in tenant {0}: {1}", t, ex);
                    }
                }
            }
        }


        [Flags]
        enum TextIndexAction
        {
            None = 0,
            Index = 1,
            Remove = 2,
        }
    }
}
