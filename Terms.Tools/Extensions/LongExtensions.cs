using System;
using System.Collections.Generic;

namespace Terms.Tools.Extensions;

public static class LongExtensions
{
    public static string ToFormattedSize(this long number)
    {
        string formattedSize = number.ToString();

        List<string> suffixes = new() { "B", "KB", "MB", "GB", "TB", "PB" };

        for (int suffixIndex = 0; suffixIndex < suffixes.Count; suffixIndex++)
        {
            long newFormattedSize = number / (long)Math.Pow(1024, suffixIndex + 1);

            if (newFormattedSize == 0)
            {
                formattedSize = $"{number / (long)Math.Pow(1024, suffixIndex)} {suffixes[suffixIndex]}";
                break;
            }
        }

        return formattedSize;
    }
}