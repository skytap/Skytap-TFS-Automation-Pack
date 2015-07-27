// 
// ConfigurationParametersTests.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 

using System.Collections.Specialized;
using System.Configuration.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skytap.Utilities;

namespace SkytapCLITests
{
    [TestClass]
    public class ApplicationParametersTests
    {
        //
        // NOTE: It is important to use the ShimConfigurationManager for these tests as changing AppSettings
        // directly could have an effect on other tests since there is one static copy of AppSettings per
        // instance, and the collection of tests is considered one instance. The shim creates a temporary
        // copy of the AppSettings collection for local use in the test. 
        //

        [TestMethod]
        public void ApplicationParameters_ValidAppSettings()
        {
            using (ShimsContext.Create())
            {
                var appSettings = new NameValueCollection
                                  {
                                      {ApplicationParameters.ParamNumRetries, "3"},
                                      {ApplicationParameters.ParamWaitTime, "00:05:00"},
                                      {ApplicationParameters.ParamHttpTimeout, "12345"},
                                      {ApplicationParameters.ParamSkytapHostUrl,"http://somebody.com"}
                                  };
                ShimConfigurationManager.AppSettingsGet = () => appSettings;

                var config = new ApplicationParameters();

                Assert.AreEqual(3, config.RetryNumRetries);
                Assert.AreEqual(5, config.RetryWaitTime.Minutes);
                Assert.AreEqual(12345, config.HttpTimeout);
                Assert.AreEqual("http://somebody.com", config.SkytapHostUrl);
            }
        }

        [TestMethod]
        public void ApplicationParameters_NoAppSettings()
        {
            using (ShimsContext.Create())
            {
                // Need to shim the AppSettings collection since previous tests may have wrote particular values to
                // the collection, which is statically shared across tests.
                ShimConfigurationManager.AppSettingsGet = () => new NameValueCollection();

                var config = new ApplicationParameters();

                Assert.AreEqual(ApplicationParameters.DefaultNumRetries, config.RetryNumRetries);
                Assert.AreEqual(ApplicationParameters.DefaultRetryWaitTime.Ticks, config.RetryWaitTime.Ticks);
                Assert.AreEqual(ApplicationParameters.DefaultHttpTimeout, config.HttpTimeout);
                Assert.AreEqual(ApplicationParameters.DefaultSkytapHostUrl, config.SkytapHostUrl);
            }
        }

        [TestMethod]
        public void ApplicationParameters_EmptyAppSettings()
        {
            using (ShimsContext.Create())
            {
                var appSettings = new NameValueCollection
                                  {
                                      {ApplicationParameters.ParamNumRetries, string.Empty},
                                      {ApplicationParameters.ParamWaitTime, string.Empty},
                                      {ApplicationParameters.ParamHttpTimeout, string.Empty},
                                      {ApplicationParameters.ParamSkytapHostUrl, string.Empty}
                                  };
                ShimConfigurationManager.AppSettingsGet = () => appSettings;

                var config = new ApplicationParameters();

                Assert.AreEqual(ApplicationParameters.DefaultNumRetries, config.RetryNumRetries);
                Assert.AreEqual(ApplicationParameters.DefaultRetryWaitTime.Ticks, config.RetryWaitTime.Ticks);
                Assert.AreEqual(ApplicationParameters.DefaultHttpTimeout, config.HttpTimeout);
                Assert.AreEqual(ApplicationParameters.DefaultSkytapHostUrl, config.SkytapHostUrl);
            }
        }

        [TestMethod]
        public void ApplicationParameters_BadTimeSpan()
        {
            using (ShimsContext.Create())
            {
                var appSettings = new NameValueCollection {{ApplicationParameters.ParamWaitTime, "xyzzy"}};
                ShimConfigurationManager.AppSettingsGet = () => appSettings;

                var config = new ApplicationParameters();

                Assert.AreEqual(ApplicationParameters.DefaultRetryWaitTime.Ticks, config.RetryWaitTime.Ticks);
            }
        }
    }
}
