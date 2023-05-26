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
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;

namespace ASC.Common.Web
{
    public static class ResiliencePolicyManager
    {
        public const int RETRY_COUNT = 2;
        public const int FUNCTION_BASIS = 2;

        public const double FAILURE_THRESHOLD = 0.5;
        public const int SAMPLINF_DURATION_SEC = 30;
        public const int MINIMUM_THROUGHPUT = 10;
        public const int DURATION_OF_BREAK_SEC = 60;

        public const int OPERATION_TIMEOUT_SEC = 2;

        private static ConcurrentDictionary<string, AsyncPolicyWrap> policiesStorage = new ConcurrentDictionary<string, AsyncPolicyWrap>();

        public async static Task<string> GetStringWithPoliciesAsync(string requestIdentifier, Func<Task<string>> function)
        {
            var policy = GetAsyncPolicy(requestIdentifier);

            var responseString = await policy
               .ExecuteAsync(function);

            return responseString;
        }

        private static AsyncPolicyWrap GetAsyncPolicy(string policyEntryName)
        {
            if (policiesStorage.TryGetValue(policyEntryName, out var policy))
            {
                return policy;
            }

            var policyHandler = Policy.Handle<Exception>(ex => ex.GetType() != typeof(BrokenCircuitException));
            //catch all errors except for errors from the breaker

            var retryPolicy = policyHandler.WaitAndRetryAsync(RETRY_COUNT, retryAttempt =>
            {
                return TimeSpan.FromSeconds(Math.Pow(FUNCTION_BASIS, retryAttempt));
            });
            //retry with exponential delay

            var circuitPolicy = policyHandler
                .AdvancedCircuitBreakerAsync(FAILURE_THRESHOLD, TimeSpan.FromSeconds(SAMPLINF_DURATION_SEC), MINIMUM_THROUGHPUT, TimeSpan.FromSeconds(DURATION_OF_BREAK_SEC));
            //circuit breaker accumulates statistics and starts canceling requests after a certain number of failures

            var operationTimeoutPolicy = Policy.TimeoutAsync(OPERATION_TIMEOUT_SEC);
            //timeout for one operation

            policy = Policy
               .WrapAsync(retryPolicy, circuitPolicy, operationTimeoutPolicy);

            policiesStorage.TryAdd(policyEntryName, policy);

            return policy;
        }
    }
}
