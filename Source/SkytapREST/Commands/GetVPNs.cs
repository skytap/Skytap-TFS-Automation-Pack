// 
// GetVPNs.cs
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
using System.Diagnostics;
using Skytap.Cloud.Utilities;
using Skytap.Utilities;

namespace Skytap.Cloud.Commands
{
    internal class GetVpns : ICommand
    {
        private readonly string[] _args = { };
        private readonly string _help;

        public string Name { get { return "getvpns"; } }
        public string[] ArgNames { get { return _args; } }
        public string Help { get { return _help; } }

        public GetVpns()
        {
            _help = string.Empty;
        }

        public int Invoke(Dictionary<string, string> args)
        {
            var logger = LoggerFactory.GetLogger();
            var credentials = new Credentials(args[Arguments.Username], args[Arguments.Password]);

            var responseContent = SkytapApi.GetVpns(credentials);
            Debug.Assert(!string.IsNullOrEmpty(responseContent));

            return CommandResults.Success;
        }

        public bool ValidateArgs(Dictionary<string, string> args)
        {
            return true;
        }
    }
}
