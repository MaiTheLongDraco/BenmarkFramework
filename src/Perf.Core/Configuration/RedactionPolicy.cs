using System;
using System.Text.RegularExpressions;

namespace Perf.Core.Configuration;

public static class RedactionPolicy
{
    private static readonly Regex _creditCardRegex = new Regex(@"\b(?:\d[ -]*?){13,16}\b", RegexOptions.Compiled);
    private static readonly Regex _emailRegex = new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled);
    private static readonly Regex _jwtRegex = new Regex(@"eyJ[A-Za-z0-9-_=]+\.[A-Za-z0-9-_=]+\.?[A-Za-z0-9-_.+/=]*", RegexOptions.Compiled);

    public static string Redact(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        input = _creditCardRegex.Replace(input, "***CC***");
        input = _emailRegex.Replace(input, "***EMAIL***");
        input = _jwtRegex.Replace(input, "***JWT***");

        return input;
    }
}