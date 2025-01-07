using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedLibrary.Helpers;

public static class PrimitiveHelper
{
    #region Number
    public static string BytesToString(this long byteCount) {
        string[] suf = { "b", "kb", "mb", "gb", "tb", "pb", "eb" }; //Longs run out around EB
        if(byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }
    #endregion

    #region String
    public static string RemoveNonLetterDigit(this string source) {
        return new string(source.Where(x => char.IsLetterOrDigit(x)).ToArray());
    }

    public static List<string> CleanListString(this List<string?> source) {
        return source.Where(s => !string.IsNullOrWhiteSpace(s))
                            .Select(s => s!.Trim())
                            .OrderBy(s => s)
                            .ToList();
    }

    const string vowel = "aeiou";
    const string semiVowel = "hrwy";
    const string consonant = "bcdfgjklmnpqstvxz";
    const int offsetStart = 1;

    public static string DiskCensor(this string source) {
        if(string.IsNullOrEmpty(source)) return source;

        var sb = new StringBuilder();
        int offset = offsetStart;
        foreach(char c in source) {
            int vowelIdx = vowel.IndexOf(c, StringComparison.OrdinalIgnoreCase);
            if(vowelIdx != -1) {
                var toAddChar = vowel[(vowelIdx + offset) % vowel.Length];
                sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                offset++;
                continue;
            }

            int semiVowelIdx = semiVowel.IndexOf(c, StringComparison.OrdinalIgnoreCase);
            if(semiVowelIdx != -1) {
                var toAddChar = semiVowel[(semiVowelIdx + offset) % semiVowel.Length];
                sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                offset++;
                continue;
            }

            var consonantIdx = consonant.IndexOf(c, StringComparison.OrdinalIgnoreCase);
            if(consonantIdx != -1) {
                var toAddChar = consonant[(consonantIdx + offset) % consonant.Length];
                sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                offset++;
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    public static string DiskDecensor(this string source) {
        if(string.IsNullOrEmpty(source)) return source;

        int GetClampedIndex(int len, int decrement) {
            if(decrement >= 0) return decrement;

            return GetClampedIndex(len, len + decrement);
        }

        var sb = new StringBuilder();
        int offset = -1 * offsetStart;
        foreach(char c in source) {
            int vowelIdx = vowel.IndexOf(c, StringComparison.OrdinalIgnoreCase);
            if(vowelIdx != -1) {
                var toAddChar = vowel[GetClampedIndex(vowel.Length, vowelIdx + offset)];
                sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                offset--;
                continue;
            }

            int semiVowelIdx = semiVowel.IndexOf(c, StringComparison.OrdinalIgnoreCase);
            if(semiVowelIdx != -1) {
                var toAddChar = semiVowel[GetClampedIndex(semiVowel.Length, semiVowelIdx + offset)];
                sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                offset--;
                continue;
            }

            var consonantIdx = consonant.IndexOf(c, StringComparison.OrdinalIgnoreCase);
            if(consonantIdx != -1) {
                var toAddChar = consonant[GetClampedIndex(consonant.Length, consonantIdx + offset)];
                sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                offset--;
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    public static string QuickHash(this string input) {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var inputHash = SHA256.HashData(inputBytes);
        return Convert.ToHexString(inputHash);
    }

    #endregion

    #region Collection
    public static bool ContainsContains(this string[] haystack, string needle) {
        foreach(string s in haystack) {
            if(s.ToLower().Contains(needle.ToLower())) {
                return true;
            }
        }
        return false;
    }

    public static bool ContainsContains(this IEnumerable<string> haystack, string needle, StringComparison comp) {
        foreach(string s in haystack) {
            if(s.Contains(needle, comp)) {
                return true;
            }
        }
        return false;
    }

    public static IOrderedEnumerable<T> OrderByAlphaNumeric<T>(this IEnumerable<T> source, Func<T, string> selector) {
        int max = source
            .SelectMany(i => Regex.Matches(selector(i), @"\d+").Cast<Match>().Select(m => (int?)m.Value.Length))
            .Max() ?? 0;

        return source.OrderBy(i => Regex.Replace(selector(i), @"\d+", m => m.Value.PadLeft(max, '0')));
    }
    #endregion

    #region DateTime
    public static string Format(this TimeSpan source) {
        return source.ToString(@"mm\:ss\.fffff");
    }
    #endregion
}
