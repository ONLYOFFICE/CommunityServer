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
using System.Threading.Tasks;
using System.Web;

using ASC.Common.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Polly.CircuitBreaker;

namespace ASC.Common.Tests.Web
{
    [TestClass]
    public class ResiliencePoliciesTest
    {
        private string REQUEST_IDENTIFIER;

        private const string TEST_MESSAGE = "test";
        private int retryCounter;

        [TestInitialize()]
        public void Setup()
        {
            REQUEST_IDENTIFIER = Guid.NewGuid().ToString();
            retryCounter = 0;
        }

        [TestMethod]
        public async Task CheckSuccessfulCase()
        {
            var response = await ResiliencePolicyManager.GetStringWithPoliciesAsync(REQUEST_IDENTIFIER, GetSuccessfullFunction());
            Assert.AreEqual(response, TEST_MESSAGE);
        }

        [TestMethod]
        public async Task CheckFailedCase()
        {
            try
            {
                var response = await ResiliencePolicyManager.GetStringWithPoliciesAsync(REQUEST_IDENTIFIER, GetFailedFunction());
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, TEST_MESSAGE);
            }
        }

        [TestMethod]
        public async Task CheckRetryPolicy()
        {
            await ResiliencePolicyManager.GetStringWithPoliciesAsync(REQUEST_IDENTIFIER, GetSuccessfullFunction());
            Assert.AreEqual(retryCounter, 1);
            retryCounter = 0;

            try
            {
                await ResiliencePolicyManager.GetStringWithPoliciesAsync(REQUEST_IDENTIFIER, GetFailedFunction());
            }
            catch (Exception)
            {
                Assert.AreEqual(retryCounter, ResiliencePolicyManager.RETRY_COUNT + 1);
            }
        }
     

        [TestMethod]
        public async Task CheckCircuitBreakerPolicy()
        {
            for (int i = 0; i < ResiliencePolicyManager.MINIMUM_THROUGHPUT + 1; i++)
            {
                try
                {
                    await ResiliencePolicyManager.GetStringWithPoliciesAsync(REQUEST_IDENTIFIER, GetFailedFunction());
                }
                catch(HttpException)
                {
                    continue;
                }
                catch (BrokenCircuitException)
                {
                    break;
                }
            }

            Assert.AreEqual(retryCounter, ResiliencePolicyManager.MINIMUM_THROUGHPUT);
        }


        [TestMethod]
        public async Task CheckMixedSequenceRequests()
        {
            var numberSuccessedRequests = ResiliencePolicyManager.MINIMUM_THROUGHPUT 
                * (1 - ResiliencePolicyManager.FAILURE_THRESHOLD) 
                / ResiliencePolicyManager.RETRY_COUNT;

            for (int i = 0; i < ResiliencePolicyManager.MINIMUM_THROUGHPUT + 1; i++)
            {
                try
                {
                    if(i < numberSuccessedRequests)
                    {
                        await ResiliencePolicyManager.GetStringWithPoliciesAsync(REQUEST_IDENTIFIER, GetSuccessfullFunction());
                    }
                    else
                    {
                        await ResiliencePolicyManager.GetStringWithPoliciesAsync(REQUEST_IDENTIFIER, GetFailedFunction());
                    }
                }
                catch (HttpException)
                {
                    continue;
                }
                catch (BrokenCircuitException)
                {
                    break;
                }
            }

            Assert.AreEqual(retryCounter, ResiliencePolicyManager.MINIMUM_THROUGHPUT);
        }

        private Func<Task<string>> GetSuccessfullFunction()
        {
            return async () =>
            {
                retryCounter++;
                return await Task.FromResult(TEST_MESSAGE);
            };
        }

        private Func<Task<string>> GetFailedFunction()
        {
            return () =>
            {
                retryCounter++;
                throw new HttpException(TEST_MESSAGE);
            };
        }
    }
}
