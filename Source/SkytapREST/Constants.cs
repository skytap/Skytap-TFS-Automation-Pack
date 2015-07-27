// 
// Constants.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 
namespace Skytap.Cloud
{
    internal abstract class Arguments
    {
        public const string Username = "username";
        public const string Password = "password";
        public const string ConfigName = "configname";
        public const string ConfigId = "configid";
        public const string SaveTemplate = "savetemplate";
        public const string TemplateId = "templateid";
        public const string VpnId = "vpnid";
    }

    internal static class ConfigurationStates
    {
        public const string Busy = "busy";
        public const string Halted = "halted";
        public const string Running = "running";
        public const string Stopped = "stopped";
        public const string Suspended = "suspended";
    }

    internal static class CommandResults
    {
        public const int Success = 0;
        public const int Fail = 1;
    }
}
