namespace Mod.Helpers;

using System.Threading;

/// <summary> Fixes parses for decimal numbers so they aren't region dependant </summary>
public static class UniversalFloatParse
{
    public static void Fix() => Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
}