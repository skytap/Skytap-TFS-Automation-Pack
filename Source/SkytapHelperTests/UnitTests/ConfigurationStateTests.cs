// 
// ConfigurationStateTests.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 

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
