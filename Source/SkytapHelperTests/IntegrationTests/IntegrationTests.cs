// 
// IntegrationTests.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skytap.Cloud;
using Skytap.Cloud.Commands;
using Skytap.Cloud.Utilities;
using Skytap.Utilities;

namespace SkytapCLITests
{
    // FUTURE: Pull this configuration data out of a file so it doesn't require a re-compile to change
    public class SkytapUser
    {
        public string Username { get; set; }
        public string Key { get; set; }
        public string ConfigRunningTfs { get; set; }
        public string TargetTemplate { get; set; }
        public string VpnId { get; set; }

        public SkytapUser()
        {
            Username = "jamesw@crosslaketech.com";
            Key = "cda1474ad6e70121e8d3918399cade5817db2776";
            ConfigRunningTfs = "1363116";
            TargetTemplate = "322381";
            VpnId = null;
        }
    }

    public class SkytapVpnUser : SkytapUser
    {
        public SkytapVpnUser()
        {
            ConfigRunningTfs = "1392492";
            TargetTemplate = "337317";
            VpnId = "vpn-937406"; // VPN side 2 in test configuration
        }
    }

    [TestClass]
    public class IntegrationTests
    {
        private const string LogFilePrefix = "SkytapCLITest";
        private static Logger _logger;

        private static void InitializeLogFile()
        {
            LoggerFactory.Reset();
            LoggerFactory.LoggerType = LoggerTypes.Trace;

            _logger = LoggerFactory.GetLogger();
            Debug.Assert(_logger != null);
            Debug.Assert(_logger is TraceLogger);

            ((TraceLogger)(_logger)).LogFilePrefix = LogFilePrefix;
        }

        [TestInitialize]
        public void Initialize()
        {
            ConfigurationManager.AppSettings[ApplicationParameters.ParamNumRetries] = "3";
            ConfigurationManager.AppSettings[ApplicationParameters.ParamWaitTime] = "00:00:05";

            InitializeLogFile();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Integration_Api_CreateConfigToShutdownConfig()
        {
            const int numPollsForDesiredState = 20;

            var user = new SkytapUser();
            var timeBetweenPolls = new TimeSpan(0, 0, 0, 5);

            var tfsConfigIdParam = user.ConfigRunningTfs;
            var skytapTargetTemplateIdParam = user.TargetTemplate;
            var configName = "Integration_API_TestConfig_" + DateTime.Now.ToString("yy-MM-dd_hh.mm.ss");
            var credentials = new Credentials(user.Username, user.Key);

            // Get NetworkID of the TFS Configuration which is running in Skytap
            var tfsConfigNetworkId = SkytapApi.GetNetworkIdInConfiguration(credentials, tfsConfigIdParam);
            Assert.IsFalse(string.IsNullOrEmpty(tfsConfigNetworkId));

            // CreateConfiguration (returns ConfigID of the instantiated template)
            var newTargetConfig = SkytapApi.CreateConfiguration(credentials, skytapTargetTemplateIdParam, configName);
            Assert.IsNotNull(newTargetConfig);

            Retry.Execute(() => SkytapApi.CheckConfigurationForDesiredState(credentials, newTargetConfig.ConfigurationUrl, 
                                                                                new List<string> {ConfigurationStates.Suspended, ConfigurationStates.Stopped}), 
                                                                                numPollsForDesiredState, timeBetweenPolls);

            // CreateIcnrConnection (between the TFSConfigNetwork and the newly instantiated config. Note that it
            // is important that the source network ID be the new configuration and the target network ID be
            // the (existing) TFS configuration. If the two are reversed, some unexplainable 409 (conflict) 
            // errors occur. Following up and will update this if a good explanation is received.
            var icnrConnectionId = SkytapApi.CreateIcnrConnection(credentials, newTargetConfig.ConfigurationNetworkId, tfsConfigNetworkId);
            Assert.IsFalse(string.IsNullOrEmpty(icnrConnectionId));
            Retry.Execute(() => SkytapApi.CheckConfigurationForDesiredState(credentials, newTargetConfig.ConfigurationUrl,
                                                                                ConfigurationStates.Suspended), 
                                                                                numPollsForDesiredState, timeBetweenPolls);

            SkytapApi.SetConfigurationState(credentials, newTargetConfig.ConfigurationUrl, ConfigurationStates.Running);
            Retry.Execute(() => SkytapApi.CheckConfigurationForDesiredState(credentials, newTargetConfig.ConfigurationUrl,
                                                                                ConfigurationStates.Running),
                                                                                numPollsForDesiredState, timeBetweenPolls);

            SkytapApi.SaveAsSkytapTemplate(credentials, newTargetConfig.ConfigurationUrl);
            Retry.Execute(() => SkytapApi.CheckConfigurationForDesiredState(credentials, newTargetConfig.ConfigurationUrl,
                                                                                ConfigurationStates.Suspended),
                                                                                numPollsForDesiredState, timeBetweenPolls);

            SkytapApi.DeleteIcnrConnection(credentials, icnrConnectionId);
            Retry.Execute(() => SkytapApi.CheckConfigurationForDesiredState(credentials, newTargetConfig.ConfigurationUrl,
                                                                                ConfigurationStates.Suspended),
                                                                                numPollsForDesiredState, timeBetweenPolls);

            SkytapApi.ShutDownConfiguration(credentials, newTargetConfig.ConfigurationUrl);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Integration_Commands_CreateConfigToShutdownConfig()
        {
            var args = new Dictionary<string, string>();
            var user = new SkytapUser();

            args[Arguments.Username] = user.Username;
            args[Arguments.Password] = user.Key;
            args[Arguments.ConfigId] = user.ConfigRunningTfs;
            args[Arguments.TemplateId] = user.TargetTemplate;
            args[Arguments.ConfigName] = "Integration_Commands_TestConfig_" + DateTime.Now.ToString("yy-MM-dd_hh.mm.ss");

            var tfsStartupCommand = new TfsStartup();
            var tfsStartupCommandValidation = tfsStartupCommand.ValidateArgs(args);
            var tfsStartupResult = tfsStartupCommand.Invoke(args);

            // INSERT AN ARTIFICIAL DELAY to simulate a set of tests being run to make this reflect the build
            // process a little more closely. This is an optional delay and not required for the test.
            Thread.Sleep(5000 /* ms */);

            var tfsShutdownCommand = new TfsShutdown();
            var tfsShutdownCommandValidation = tfsShutdownCommand.ValidateArgs(args);
            var tfsShutdownResult = tfsShutdownCommand.Invoke(args);

            Assert.IsTrue(tfsStartupCommandValidation);
            Assert.AreEqual(0, tfsStartupResult);
            Assert.IsTrue(tfsShutdownCommandValidation);
            Assert.AreEqual(0, tfsShutdownResult);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Integration_Commands_CreateConfigToShutdownConfigWithVpn()
        {
            var args = new Dictionary<string, string>();
            var user = new SkytapVpnUser();

            args[Arguments.Username] = user.Username;
            args[Arguments.Password] = user.Key;
            args[Arguments.ConfigId] = user.ConfigRunningTfs;
            args[Arguments.TemplateId] = user.TargetTemplate;
            args[Arguments.VpnId] = user.VpnId;
            args[Arguments.ConfigName] = "Integration_Commands_TestConfig_" + DateTime.Now.ToString("yy-MM-dd_hh.mm.ss");

            var tfsStartupCommand = new TfsStartup();
            var tfsStartupCommandValidation = tfsStartupCommand.ValidateArgs(args);
            var tfsStartupResult = tfsStartupCommand.Invoke(args);

            var tfsShutdownCommand = new TfsShutdown();
            var tfsShutdownCommandValidation = tfsShutdownCommand.ValidateArgs(args);
            var tfsShutdownResult = tfsShutdownCommand.Invoke(args);

            Assert.IsTrue(tfsStartupCommandValidation);
            Assert.AreEqual(0, tfsStartupResult);
            Assert.IsTrue(tfsShutdownCommandValidation);
            Assert.AreEqual(0, tfsShutdownResult);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Integration_Commands_GetVpns()
        {
            var args = new Dictionary<string, string>();
            var user = new SkytapVpnUser();

            args[Arguments.Username] = user.Username;
            args[Arguments.Password] = user.Key;

            var getVpnsCommand = new GetVpns();
            var validateResult = getVpnsCommand.ValidateArgs(args);
            var commandResult = getVpnsCommand.Invoke(args);

            Assert.IsTrue(validateResult);
            Assert.AreEqual(CommandResults.Success, commandResult);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Integration_Program_StartupAndShutdown_MultipleRuns()
        {
            var user = new SkytapUser();

            for (var i = 0; i < 4; i++)
            {
                var configName = "Integration_Program_TestConfig_" + DateTime.Now.ToString("yy-MM-dd_hh.mm.ss");

                var startupCommandLineArgs =
                    string.Format("/action tfsstartup /username {0} /password {1} /configid {2} /vpnid /templateid {3} /configname {4}",
                    user.Username, user.Key, user.ConfigRunningTfs, user.TargetTemplate, configName);

                var shutdownCommandLineArgs =
                    string.Format("/action tfsshutdown /username {0} /password {1} /savetemplate false /configname {2}",
                    user.Username, user.Key, configName);

                var tfsStartupResult = Program.Main(startupCommandLineArgs.Split(' '));
                var tfsShutdownResult = Program.Main(shutdownCommandLineArgs.Split(' '));

                Assert.AreEqual(0, tfsStartupResult);
                Assert.AreEqual(0, tfsShutdownResult);
            }
        }

    }
}
