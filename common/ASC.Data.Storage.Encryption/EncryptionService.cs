﻿/*
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


using ASC.Common.Threading.Progress;
using ASC.Core.Common.Contracts;
using ASC.Core.Encryption;

namespace ASC.Data.Storage.Encryption
{
    class EncryptionService : IEncryptionService, IHealthCheckService
    {
        public void Start(EncryptionSettings encryptionSettings, string serverRootPath)
        {
            EncryptionWorker.Start(encryptionSettings, serverRootPath);
        }

        public double GetProgress()
        {
            var progress = (ProgressBase)EncryptionWorker.GetProgress();

            return progress != null ? progress.Percentage : -1;
        }

        public void Stop()
        {
            EncryptionWorker.Stop();
        }

        public HealthCheckResponse CheckHealth()
        {
            return HealthCheckResult.ToResponse(new HealthCheckResult
            {
                Message = "Service Encryption is OK! Warning: Method is not implement. Always return the Healthy status",
                Status = HealthStatus.Healthy
            });
        }

    }
}
