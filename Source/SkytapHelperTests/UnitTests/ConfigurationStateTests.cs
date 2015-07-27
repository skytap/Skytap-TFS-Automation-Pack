// 
// ConfigurationStateTests.cs
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

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skytap.Cloud;
using Skytap.Cloud.Utilities;

namespace SkytapCLITests
{
    internal class ConfigurationStateTestHelper : ConfigurationState
    {
        public const string TestConfigFilename = "SkytapTest.xml";

        public string GetPath()
        {
            return GeneratePath(string.Empty);
        }

        protected override string GeneratePath(string configName)
        {
            return Path.Combine(Path.GetTempPath(), TestConfigFilename);
        }
    }

    [TestClass]
    public class ConfigurationStateTests
    {
        [TestMethod]
        public void ConfigurationState_Deserialize()
        {
            const string configName = "MyConfig";
            const string configUrl = "http://blah.com";
            const string configVpnId = "1234ABCD";

            var config = new SkytapConfiguration { Name = configName, ConfigurationUrl = configUrl, VpnId = configVpnId };
            ConfigurationStateTestHelper configState = null;

            try
            {
                configState = new ConfigurationStateTestHelper();

                configState.Serialize(config);
                var deserializedConfigState = configState.Deserialize(configName);
                var output = deserializedConfigState.ToString();

                Assert.IsNotNull(deserializedConfigState);
                StringAssert.StartsWith(deserializedConfigState.Name, configName);
                StringAssert.StartsWith(deserializedConfigState.ConfigurationUrl, configUrl);
                StringAssert.StartsWith(deserializedConfigState.VpnId, configVpnId);
                Assert.IsFalse(string.IsNullOrEmpty(output));
            }
            finally
            {
                // Make sure to clean-up any files that were left around by this test.
                if (configState != null && File.Exists(configState.GetPath()))
                {
                    File.Delete(configState.GetPath());
                }
            }
        }

    }
}
