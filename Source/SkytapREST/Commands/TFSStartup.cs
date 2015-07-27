// 
// TFSStartup.cs
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
using System.Collections.Generic;
using Skytap.Cloud.Properties;
using Skytap.Cloud.Utilities;
using Skytap.Utilities;

namespace Skytap.Cloud.Commands
{
    internal class TfsStartup : ICommand
    {
        private const int NumRetriesCheckConfigState = 50;
        private const int NumRetriesStartConfig = 5;
        private readonly TimeSpan _retryIntervalCheckConfigState = new TimeSpan(0, 0, 0, 5 /* sec */);
        private readonly TimeSpan _retryIntervalStartConfig = new TimeSpan(0, 0, 1 /* min */, 0);

        private readonly string[] _args = { Arguments.ConfigId, Arguments.VpnId, Arguments.TemplateId, Arguments.ConfigName };
        private readonly string _help;

        public string Name { get { return "tfsstartup"; } }
        public string[] ArgNames { get { return _args; } }
        public string Help { get { return _help; } }

        public TfsStartup()
        {
            _help = Resources.TfsShutdown_HelpConfigId;    
        }

        public int Invoke(Dictionary<string, string> args)
        {
            var credentials = new Credentials(args[Arguments.Username], args[Arguments.Password]);
            var configId = args.ContainsKey(Arguments.ConfigId) ? args[Arguments.ConfigId] : null;
            var vpnId = args.ContainsKey(Arguments.VpnId) ? args[Arguments.VpnId] : null;
            var templateId = args[Arguments.TemplateId];
            var configName = args[Arguments.ConfigName];
            var configState = new ConfigurationState();

            var logger = LoggerFactory.GetLogger();

            // CreateConfiguration (returns ConfigID of the instantiated template)
            SkytapConfiguration newTargetConfig = SkytapApi.CreateConfiguration(credentials, templateId, configName);

            // Wait for Skytap to return the expected configuration state. Do this with a retry block 
            // to test for the desired state every second for 5 minutes.
            Retry.Execute(() => SkytapApi.CheckConfigurationForDesiredState(credentials, newTargetConfig.ConfigurationUrl, 
                                                                             new List<string> {ConfigurationStates.Suspended, ConfigurationStates.Stopped}),
                                                                             NumRetriesCheckConfigState, 
                                                                             _retryIntervalCheckConfigState);

            // If VPNID supplied, make a VPN connection, if a configid, create ICNR Connection
            if (String.IsNullOrEmpty(vpnId))
            {
                // CreateIcnrConnection (between the TFSConfigNetwork and the newly instantiated config. Note that it
                // is important that the source network ID be the new configuration and the target network ID be
                // the (existing) TFS configuration. If the two are reversed, a 409 (conflict) error may occur.
                var tfsConfigNetworkId = SkytapApi.GetNetworkIdInConfiguration(credentials, configId);
                var icnrId = SkytapApi.CreateIcnrConnection(credentials, newTargetConfig.ConfigurationNetworkId, tfsConfigNetworkId );
                newTargetConfig.IcnrId = icnrId;
            }
            else
            {
                SkytapApi.AttachVpnConnection(credentials, newTargetConfig.ConfigurationUrl, newTargetConfig.ConfigurationNetworkId, vpnId);
                SkytapApi.ConnectVpn(credentials, newTargetConfig.ConfigurationUrl, newTargetConfig.ConfigurationNetworkId, vpnId);
                newTargetConfig.VpnId = vpnId;
            }
            
            // Need to wait again for ICNR or VPN to complete. 
            //
            // Before starting the configuration, ensure that it is suspended. If it is not suspended (and perhaps stopped)
            // wait until it is in the desired state.
            var configurationState = SkytapApi.GetConfigurationState(credentials, newTargetConfig.ConfigurationUrl);

            if (configurationState != ConfigurationStates.Running)
            {
                // Attempt to start up the configuration using retry semantics just in case the first request doesn't work. This
                // could happen if the configuration tries to restart but the service is busy so the state is returned to 
                // suspended and we need to retry the start-up.
                Retry.Execute(() =>
                    {
                        // Start up the configuration by changing its state to running. It is assumed there is a change to the 
                        // runstate at this point, either to "busy" or "running". If the configuration ends up as "running", 
                        // nothing else to do - just continue. If the state goes back to "suspended", need to retry the start
                        // logic a few more times until the number of retries is exhausted. If it never comes back from "busy" or
                        // enters some other unknown state, just exit once the retry threshold is reached.
                        SkytapApi.SetConfigurationState(credentials, newTargetConfig.ConfigurationUrl, ConfigurationStates.Running);

                        Retry.Execute(() => SkytapApi.CheckConfigurationForDesiredState(credentials, newTargetConfig.ConfigurationUrl,
                                                                            new List<string> { ConfigurationStates.Running, ConfigurationStates.Suspended }),
                                                                            NumRetriesCheckConfigState, _retryIntervalCheckConfigState);
                                
                        // Re-get the configuration state so we can determine whether to give up attempting to start the 
                        // configuration or try again. The exception will trigger a retry.
                        var currentConfigState = SkytapApi.GetConfigurationState(credentials, newTargetConfig.ConfigurationUrl);
                        if (currentConfigState == ConfigurationStates.Suspended)
                        {
                            throw new ApplicationException(Resources.TfsStartup_UnexpectedReversionToSuspended);
                        }
                    }, 
                    NumRetriesStartConfig, _retryIntervalStartConfig);
            }

            // Persist the log file path so that successive invocations of the EXE can use the 
            // same log file.
            newTargetConfig.LogFilePath = ((TraceLogger) logger).LogFilePath;

            // Store Config Url so that we can run shutdown on it later since TFS isn't smart enough to do this for us
            var configStatePath = configState.Serialize(newTargetConfig);
            logger.LogInfo("Persisted configuration path: " + configStatePath);
            logger.LogInfo(newTargetConfig.ToString());

            return CommandResults.Success;
        }

        public bool ValidateArgs(Dictionary<string, string> args)
        {
            return (args.ContainsKey("configid") || args.ContainsKey("vpnid")) &&
                   args.ContainsKey("templateid") && args.ContainsKey("configname");
        }
    }
}
