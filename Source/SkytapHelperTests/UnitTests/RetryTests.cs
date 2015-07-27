// 
// RetryTests.cs
/**
 * Copyright 2014 Skytap Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 **/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skytap.Utilities;

namespace SkytapHelperTests
{
    [TestClass]
    public class RetryTests
    {
        // Set a very low retry interval to keep the tests running quickly
        private readonly TimeSpan _testingTimeSpan = new TimeSpan(10);

        [TestMethod]
        public void Retry_Execute_OneTrySuccess()
        {
            Retry.Execute(() => { });

            Assert.AreEqual(0, Retry.NumRetries);
        }

        [TestMethod]
        public void Retry_Execute_TwoRetriesSuccess()
        {
            var retryNum = 0;

            Retry.Execute(() =>
                          {
                              if (retryNum++ < 1)
                              {
                                  throw new ArgumentOutOfRangeException();
                              }
                          }, 3, _testingTimeSpan);

            Assert.AreEqual(1, Retry.NumRetries);
        }

        [TestMethod]
        public void Retry_Execute_TwoRetriesFail()
        {
            try
            {
                Retry.Execute(() =>
                {
                    throw new ArgumentOutOfRangeException();
                }, 2, _testingTimeSpan);
            }
            catch (AggregateException)
            {
                // Expecting this exception, so just sink it. Do not use the ExpectedException
                // attribute on the test itself because we still want to assert on some state
                // below.
            }

            Assert.AreEqual(2, Retry.NumRetries);
            Assert.AreEqual(2, Retry.Exceptions.Count);
            Assert.AreEqual(2 * _testingTimeSpan.Ticks, Retry.TotalRetryTime.Ticks);
        }

        [TestMethod]
        public void Retry_Execute_ThreeRetriesWithReturnValueSuccess()
        {
            const string expectedReturnValue = "SomeString";

            var retryNum = 0;

            var returnValue = Retry.Execute(() =>
            {
                if (retryNum++ < 2)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return expectedReturnValue;
            }, 3, _testingTimeSpan);

            Assert.AreEqual(expectedReturnValue, returnValue);
        }

    }
}
