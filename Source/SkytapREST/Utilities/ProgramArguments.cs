// 
// ProgramArguments.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Skytap.Cloud
{
    class ProgramArguments
    {
        private readonly Dictionary<String, String> _argumentMap = new Dictionary<String, String>();
        private string[] _arguments;

        public Dictionary<String, String> ArgumentMap { get { return _argumentMap; } }

        public ProgramArguments(string[] args)
        {
            _arguments = args;

            // Load settings from AppConfig first
            for (var i = 0; i < ConfigurationManager.AppSettings.Count; i++)
            {
                _argumentMap.Add(ConfigurationManager.AppSettings.GetKey(i), ConfigurationManager.AppSettings[i]);
            }

            // Then override/add settings from the command line
            var key = String.Empty;
            foreach (var arg in args)
            {
                if (arg.StartsWith("/"))
                {
                    // This argument is a name that may have a value after it. Remove the "/" and extract
                    // the name. If there is already a name/value pair in the command map that reflects this
                    // parameter, clear it out as the command-line overrides it. The next argument in the loop
                    // reflects the value.
                    key = arg.Substring(1).ToLowerInvariant();
                    if (_argumentMap.ContainsKey(key))
                    {
                        _argumentMap.Remove(key);
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(key))
                    {
                        // A non-empty argument name was found, so ensure it is added to the argument map. Then
                        // ensure the value of that name is also reflected in the map.
                        if (!_argumentMap.ContainsKey(key))
                        {
                            _argumentMap.Add(key, String.Empty);
                        }

                        // NOTE: This was in the original code and we are not sure why it is here. Candidate for
                        // removal.
                        if (!String.IsNullOrEmpty(_argumentMap[key]))
                        {
                            _argumentMap[key] += " ";
                        }

                        _argumentMap[key] += arg;
                    }
                }
            }
        }

        public override string ToString()
        {
            return _arguments.Aggregate(string.Empty, (current, arg) => current + (arg + " "));
        }
    }
}
