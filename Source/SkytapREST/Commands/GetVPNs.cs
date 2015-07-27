// 
// GetVPNs.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 

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
