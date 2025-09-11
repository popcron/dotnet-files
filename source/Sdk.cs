using System;
using Unmanaged;

namespace DotNetFiles;

public readonly struct Sdk : IEquatable<Sdk>
{
    public static readonly Sdk MicrosoftNETSdk = new("Microsoft.NET.Sdk");
    public static readonly Sdk MicrosoftNETSdkWeb = new("Microsoft.NET.Sdk.Web");
    public static readonly Sdk[] All =
    [
        MicrosoftNETSdk,
        MicrosoftNETSdkWeb
    ];

    private readonly ASCIIText32 value;

    private Sdk(ASCIIText32 value)
    {
        this.value = value;
    }

    public readonly override bool Equals(object? obj)
    {
        return obj is Sdk sdk && Equals(sdk);
    }

    public readonly bool Equals(Sdk other)
    {
        return value.Equals(other.value);
    }

    public readonly override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public readonly override string ToString()
    {
        return value.ToString();
    }

    public static bool TryParse(ReadOnlySpan<char> text, out Sdk sdk)
    {
        sdk = new(text);
        if (Array.IndexOf(All, sdk) != -1)
        {
            return true;
        }
        else
        {
            sdk = default;
            return false;
        }
    }

    public static bool operator ==(Sdk left, Sdk right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Sdk left, Sdk right)
    {
        return !(left == right);
    }
}
