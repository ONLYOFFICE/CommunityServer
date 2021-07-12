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


using System.Collections.Concurrent;
using System.Configuration;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Common.Module;
using ASC.Web.Studio.Utility;

namespace ASC.Files.ThumbnailBuilder
{
    public class Launcher : IServiceController
    {
        private Worker worker;
        private CancellationTokenSource cancellationToken;
        private ServiceHost host;

        internal static readonly ConcurrentDictionary<object, FileData> Queue = new ConcurrentDictionary<object, FileData>();

        public void Start()
        {
            var configSection = (ConfigSection)ConfigurationManager.GetSection("thumbnailBuilder") ?? new ConfigSection();

            CommonLinkUtility.Initialize(configSection.ServerRoot, false);

            host = new ServiceHost(typeof(Service));
            host.Open();

            var logger = LogManager.GetLogger("ASC.Files.ThumbnailBuilder");
            var ascCache = AscCache.Default;

            cancellationToken = new CancellationTokenSource();

            worker = new Worker(configSection, logger, ascCache, cancellationToken);

            var task = new Task(
                () =>
                {
                    worker.Start();
                },
                cancellationToken.Token,
                TaskCreationOptions.LongRunning);

            task.Start();
        }

        public void Stop()
        {
            if (cancellationToken != null)
            {
                cancellationToken.Cancel();
            }

            if (worker != null)
            {
                worker.Stop();
                worker = null;
            }

            if (host != null)
            {
                host.Close();
                host = null;
            }
        }
    }
}