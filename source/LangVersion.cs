using System;
using Unmanaged;

namespace DotNetFiles;

public readonly struct LangVersion : IEquatable<LangVersion>
{
    public static readonly LangVersion Preview = new("preview");
    public static readonly LangVersion Latest = new("latest");
    public static readonly LangVersion LatestMajor = new("latestMajor");
    public static readonly LangVersion Default = new("default");
    public static readonly LangVersion CSharp14 = new("14.0");
    public static readonly LangVersion CSharp13 = new("13.0");
    public static readonly LangVersion CSharp12 = new("12.0");
    public static readonly LangVersion CSharp11 = new("11.0");
    public static readonly LangVersion CSharp10 = new("10.0");
    public static readonly LangVersion CSharp9 = new("9.0");
    public static readonly LangVersion CSharp8 = new("8.0");
    public static readonly LangVersion CSharp7_3 = new("7.3");
    public static readonly LangVersion CSharp7_2 = new("7.2");
    public static readonly LangVersion CSharp7_1 = new("7.1");
    public static readonly LangVersion CSharp7 = new("7.0");
    public static readonly LangVersion CSharp6 = new("6.0");
    public static readonly LangVersion CSharp5 = new("5.0");
    public static readonly LangVersion CSharp4 = new("4.0");
    public static readonly LangVersion CSharp3 = new("3.0");
    public static readonly LangVersion CSharp2 = new("2.0");
    public static readonly LangVersion CSharp1 = new("1.0");
    public static readonly LangVersion[] All =
    [
        Preview,
        Latest,
        LatestMajor,
        Default,
        CSharp14,
        CSharp13,
        CSharp12,
        CSharp11,
        CSharp10,
        CSharp9,
        CSharp8,
        CSharp7_3,
        CSharp7_2,
        CSharp7_1,
        CSharp7
    ];

    private readonly ASCIIText16 value;

    private LangVersion(ASCIIText16 value)
    {
        this.value = value;
    }

    public readonly override string ToString()
    {
        return value.ToString();
    }

    public readonly override bool Equals(object? obj)
    {
        return obj is LangVersion version && Equals(version);
    }

    public readonly bool Equals(LangVersion other)
    {
        return value.Equals(other.value);
    }

    public readonly override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public static bool TryParse(ReadOnlySpan<char> text, out LangVersion langVersion)
    {
        langVersion = new(text);
        if (Array.IndexOf(All, langVersion) != -1)
        {
            return true;
        }
        else
        {
            langVersion = default;
            return false;
        }
    }

    public static LangVersion Parse(ReadOnlySpan<char> text)
    {
        if (TryParse(text, out LangVersion langVersion))
        {
            return langVersion;
        }
        else
        {
            throw new($"`{text}` is not a valid lang version");
        }
    }

    public static bool operator ==(LangVersion left, LangVersion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LangVersion left, LangVersion right)
    {
        return !(left == right);
    }
}
