// 
// TFSShutdown.cs
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
using System.Linq;
using Skytap.Cloud.Properties;
using Skytap.Cloud.Utilities;
using Skytap.Utilities;

namespace Skytap.Cloud.Commands
{
    internal class TfsShutdown : ICommand
    {
        private const int NumRetriesCheckConfigState = 50;
        private readonly TimeSpan _retryIntervalCheckConfigState = new TimeSpan(0, 0, 0, 5 /* sec */);

        private readonly string[] _args = { Arguments.SaveTemplate, Arguments.ConfigName };
        private readonly string _help;

        public string Name { get { return "tfsshutdown"; } }
        public string[] ArgNames { get { return _args; } }
        public string Help { get { return _help; } }

        public TfsShutdown()
        {
            _help = Resources.TfsShutdown_HelpSaveTemplate;
        }

        public int Invoke(Dictionary<string, string> args)
        {
            var saveAsTemplate = args.Keys.Contains(Arguments.SaveTemplate) &&
                                 Boolean.Parse(args[Arguments.SaveTemplate]);
            var credentials = new Credentials(args[Arguments.Username], args[Arguments.Password]);
            var configName = args[Arguments.ConfigName];
            var logger = LoggerFactory.GetLogger();

            // Deserialize the persisted values from a previous run. The previous run would have saved off
            // a config URL, any IDs for reference, among other things.
            var configState = new ConfigurationState();
            var config = configState.Deserialize(configName);

            // FUTURE: May want to consider cleaning up the interim persistence XML file here after every run,
            // although it can be useful for debugging purposes.

            // FUTURE: Use the log file path in the persisted data to append to an existing log file instead of
            // creating a new one. Note that this will need to be done at the program level as opposed to individual
            // commands, which is more work. Suggest moving the deserialization logic earlier (program level) and 
            // setting up the log file once.

            logger.LogInfo(config.ToString());

            // Saving a template will automatically put the configuration into a suspended state. If a template is
            // not being saved, then force the suspended state. Although not strictly necessary, this makes deleting
            // connections and subsequently shutdown more robust.
            if (saveAsTemplate)
            {
                SkytapApi.SaveAsSkytapTemplate(credentials, config.ConfigurationUrl);
            }
            else
            {
                SkytapApi.SetConfigurationState(credentials, config.ConfigurationUrl, ConfigurationStates.Suspended);
            }

            // Wait for Skytap to return the expected configuration state. Do this with a retry block 
            // to test for the desired state every second for 5 minutes.
            Retry.Execute(() => SkytapApi.CheckConfigurationForDesiredState(credentials, config.ConfigurationUrl,
                                                                                ConfigurationStates.Suspended),
                                                                                NumRetriesCheckConfigState,
                                                                                _retryIntervalCheckConfigState);

            // Prior to deleting a configuration, query the current connection state and disconnect 
            // any existing ICNR or VPN connection. This helps prevent state issues on the Skytap side 
            // with not being able to recreate the configuration once again.
            CleanupConnections(credentials, config);

            // HACKHACK: this state check is an attempted workaround at resolving a response not received when deleting a configuration
            Retry.Execute(() => SkytapApi.CheckConfigurationForDesiredState(credentials, config.ConfigurationUrl,
                                                                                ConfigurationStates.Suspended),
                                                                                NumRetriesCheckConfigState,
                                                                                _retryIntervalCheckConfigState);

            // NOTE: It was determined by the engineering team that shutting down the configuration need not
            // be in a finally block. If the configuration does not shut down due to a previous error, we can 
            // live with it.
            SkytapApi.ShutDownConfiguration(credentials, config.ConfigurationUrl);

            return CommandResults.Success;
        }

        public bool ValidateArgs(Dictionary<string, string> args)
        {
            return args.Keys.Contains(Arguments.ConfigName);
        }

        private static void CleanupConnections(Credentials credentials, SkytapConfiguration config)
        {
            // FUTURE: may need to account for more than one connection, but that change should be
            // relatively straightforward. Add an array to the persisted values and simply loop 
            // over all of them to clean-up.
            if (!string.IsNullOrEmpty(config.IcnrId))
            {
                SkytapApi.DeleteIcnrConnection(credentials, config.IcnrId);
            }

            if (!string.IsNullOrEmpty(config.VpnId))
            {
                SkytapApi.DisconnectVpn(credentials, config.ConfigurationUrl, config.ConfigurationNetworkId, config.VpnId);
                SkytapApi.DetachVpn(credentials, config.ConfigurationUrl, config.ConfigurationNetworkId, config.VpnId);
            }
        }

    }
}
