using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class for computing the Levenshtein distance
/// https://en.wikipedia.org/wiki/Levenshtein_distance
/// </summary>
public static class LevenshteinDistance
{
    /// <summary>
    /// Computes the Levenshtein distance between the two given strings
    /// This is a metric for measuring how close the two character sequences are to each other
    /// </summary>
    /// <param name="s">The first string</param>
    /// <param name="t">The second string</param>
    /// <returns>Returns the Levenshtein distance, i.e. the number of steps necessary to make the two strings the same</returns>
    public static int Compute(string s, string t)
    {
        if (string.IsNullOrEmpty(s))
        {
            if (string.IsNullOrEmpty(t))
                return 0;
            return t.Length;
        }

        if (string.IsNullOrEmpty(t))
        {
            return s.Length;
        }

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // initialize the top and right of the table to 0, 1, 2, ...
        for (int i = 0; i <= n; d[i, 0] = i++);
        for (int j = 1; j <= m; d[0, j] = j++);

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                int min1 = d[i - 1, j] + 1;
                int min2 = d[i, j - 1] + 1;
                int min3 = d[i - 1, j - 1] + cost;
                d[i, j] = System.Math.Min(System.Math.Min(min1, min2), min3);
            }
        }
        return d[n, m];
    }
}
