// 
// ProgramArgumentsTests.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 

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
