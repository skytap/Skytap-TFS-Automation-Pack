// 
// ICommand.cs
// 
// Copyright (c) Skytap, Inc. All rights reserved.
// 

using System;
using System.Collections.Generic;

namespace Skytap.Cloud
{
    interface ICommand
    {
        int Invoke(Dictionary<String, String> args);
        bool ValidateArgs(Dictionary<String, String> args);
        string Name { get; }
        string[] ArgNames { get; }
        string Help { get; }
    }
}
