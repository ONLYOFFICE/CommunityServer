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
using System.Linq;
using System.Threading;

using ASC.Common.Caching;
using ASC.Common.Logging;

namespace ASC.Files.ThumbnailBuilder
{
    internal class Worker
    {
        private readonly ConfigSection config;
        private readonly ILog logger;
        private readonly FileDataProvider fileDataProvider;
        private readonly Builder builder;
        private readonly CancellationTokenSource cancellationToken;

        private Timer timer;

        public Worker(ConfigSection configSection, ILog log, ICache ascCache, CancellationTokenSource cancellationTokenSource)
        {
            config = configSection;
            logger = log;
            fileDataProvider = new FileDataProvider(configSection, ascCache);
            builder = new Builder(configSection, logger);
            cancellationToken = cancellationTokenSource;
        }

        public void Start()
        {
            timer = new Timer(Procedure, null, 0, Timeout.Infinite);
        }

        public void Stop()
        {
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
                timer = null;
            }

            if (cancellationToken != null)
            {
                cancellationToken.Dispose();
            }
        }


        private void Procedure(object _)
        {
            if (cancellationToken != null && cancellationToken.IsCancellationRequested)
            {
                Stop();
                return;
            }

            timer.Change(Timeout.Infinite, Timeout.Infinite);
            logger.Trace("Procedure: Start.");

            var filesWithoutThumbnails = Launcher.Queue.Select(pair => pair.Value).ToList();

            if (!filesWithoutThumbnails.Any())
            {
                logger.TraceFormat("Procedure: Waiting for data. Sleep {0}.", config.LaunchFrequency);
                timer.Change(config.LaunchFrequency, TimeSpan.FromMilliseconds(-1));
                return;
            }

            var premiumTenants = fileDataProvider.GetPremiumTenants();

            filesWithoutThumbnails = filesWithoutThumbnails.OrderByDescending(fileData => Array.IndexOf(premiumTenants, fileData.TenantId)).ToList();

            builder.BuildThumbnails(filesWithoutThumbnails);

            logger.Trace("Procedure: Finish.");
            timer.Change(0, Timeout.Infinite);
        }
    }
}