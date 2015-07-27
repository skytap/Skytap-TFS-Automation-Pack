// 
// ProgramArgumentsTests.cs
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

using System.Collections.Specialized;
using System.Configuration.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skytap.Cloud;

namespace SkytapCLITests
{
    [TestClass]
    public class ProgramArgumentsTests
    {
        [TestMethod]
        public void ProgramArguments_Constructor_BasicProcessingSuccess()
        {
            using (ShimsContext.Create())
            {
                // Need to shim the AppSettings collection since previous tests may have wrote particular values to
                // the collection, which is statically shared across tests.
                ShimConfigurationManager.AppSettingsGet = () => new NameValueCollection();

                var args = new [] {"/action", "someaction", "/savetemplate", "false"};

                var programArgs = new ProgramArguments(args);

                CollectionAssert.Contains(programArgs.ArgumentMap.Keys, "action");
                CollectionAssert.Contains(programArgs.ArgumentMap.Keys, "savetemplate");
                CollectionAssert.DoesNotContain(programArgs.ArgumentMap.Keys, "someaction");
                CollectionAssert.DoesNotContain(programArgs.ArgumentMap.Keys, "false");
                Assert.AreEqual(programArgs.ArgumentMap["action"], "someaction");
                Assert.AreEqual(programArgs.ArgumentMap["savetemplate"], "false");
            }
        }

        [TestMethod]
        public void ProgramArguments_Constructor_OverrideAppSetting()
        {
            using (ShimsContext.Create())
            {
                // Need to shim the AppSettings collection since previous tests may have wrote particular values to
                // the collection, which is statically shared across tests.
                var appSettings = new NameValueCollection {{"savetemplate", "false"}};
                ShimConfigurationManager.AppSettingsGet = () => appSettings;

                var args = new [] { "/savetemplate", "true" };

                var programArgs = new ProgramArguments(args);

                CollectionAssert.Contains(programArgs.ArgumentMap.Keys, "savetemplate");
                Assert.AreEqual(programArgs.ArgumentMap["savetemplate"], "true");
            }
        }

        [TestMethod]
        public void ProgramArguments_Constructor_UseAppSetting()
        {
            using (ShimsContext.Create())
            {
                // Need to shim the AppSettings collection since previous tests may have wrote particular values to
                // the collection, which is statically shared across tests.
                var appSettings = new NameValueCollection { { "savetemplate", "false" } };
                ShimConfigurationManager.AppSettingsGet = () => appSettings;

                var args = new[] { "/action", "someaction" };

                var programArgs = new ProgramArguments(args);

                CollectionAssert.Contains(programArgs.ArgumentMap.Keys, "savetemplate");
                Assert.AreEqual(programArgs.ArgumentMap["savetemplate"], "false");
            }
        }

    }
}
