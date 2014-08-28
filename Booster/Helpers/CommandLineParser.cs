using System;
using System.Collections.Generic;
using System.Globalization;

namespace Booster.Helpers
{
    internal static class CommandLineParser
    {
        internal static Dictionary<string, string> Parse(string[] args)
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            if (args != null)
            {
                foreach (string arg in args)
                {
                    int index = arg.IndexOf('=');
                    string key;
                    string value;
                    if (index >= 0)
                    {
                        key = arg.Substring(0, index).TrimStart('/').TrimStart('-').Trim().ToLower(CultureInfo.CurrentCulture);
                        value = arg.Substring(index + 1, arg.Length - index - 1).Trim(' ', '"');
                    }
                    else
                    {
                        key = arg.Substring(0, arg.Length).TrimStart('/').TrimStart('-').Trim().ToLower(CultureInfo.CurrentCulture);
                        value = String.Empty;
                    }
                    if (config.ContainsKey(key))
                    {
                        config[key] = value;
                    }
                    else
                    {
                        config.Add(key, value);
                    }
                }
            }
            return config;
        }
    }
}
