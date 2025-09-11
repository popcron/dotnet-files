using System;

namespace DotNetFiles;

[Flags]
public enum Assets
{
    All = 1,
    Compile = 2,
    Runtime = 4,
    ContentFiles = 8,
    Build = 16,
    BuildMultiTargeting = 32,
    BuildTransitive = 64,
    Analyzers = 128,
    Native = 256,
    None = 512
}
