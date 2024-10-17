using CoreAPI.AL.Models;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using c = SharedLibrary.Constants;

namespace CoreAPI.AL.Helpers;

public static class QueryHelpers
{
    private static char[] _cons = { c.ConContain, c.ConEqual, c.ConNot, c.ConGreater, c.ConLesser };

    public static List<QuerySegment> GetQuerySegments(string query) {
        var modifiedQuery = query != null && !ContainsConnector(query) ? $"fulltitle:{query}" : query;
        var segmenStrs = !string.IsNullOrEmpty(modifiedQuery) ? modifiedQuery.Split(',') : new string[] { };

        var segments = segmenStrs.Select(s => {
            var segStr = s.Trim();
            int connectorIndex = segStr.IndexOfAny(_cons);
            var connector = segStr[connectorIndex];
            var key = segStr.Substring(0, connectorIndex).ToUpper();
            var val = segStr.Substring(connectorIndex + 1, segStr.Length - (connectorIndex + 1));

            return new QuerySegment { 
                Con = connector, Key = key, Val = val
            };
        }).ToList();

        return segments;
    }

    public static string CombineQuerySegments(List<QuerySegment> source) => String.Join(',', source.Select(a => a.Text));

    static readonly string[] arrayKeys = { "TAG", "POV", "FOCUS", "OTHER", "RARE", "QUALITY", "ARTIST", "CHARACTER", "LANGUAGE" };
    static readonly string[] stringKeys = { "FULLTITLE", "TITLE", "CATEGORY", "ORIENTATION", "NOTE", "TIER", "ISWIP", "ISREAD", "ENTRYDATE" };

    public static bool MatchAllQueries(Album album, List<QuerySegment> querySegs, string[] featuredArtists, string[] featuredCharacters, DateTime now, DateTime? earliestRecentBatch = null) {
        var comparer = StringComparison.OrdinalIgnoreCase;

        bool IsMatch(Album album, QuerySegment querySeg) {
            var connector = querySeg.Con;
            var key = querySeg.Key;
            var val = querySeg.Val;

            if(arrayKeys.Contains(key)) {
                Func<List<string>, string, bool> arrOperator = 
                    connector == ':' ? (a, b) => a.Any(c => strContains(c, b)) 
                    : connector == '=' ? (a, b) => a.Any(c => strEqual(c, b))
                    : (a, b) => a.All(c => !strEqual(c, b));

                var attributeArr = key switch
                        {
                            "TAG" => album.Povs.Concat(album.Focuses).Concat(album.Others).Concat(album.Rares).Concat(album.Qualities).ToList(),
                            "POV" => album.Povs,
                            "FOCUS" => album.Focuses,
                            "OTHER" => album.Others,
                            "RARE" => album.Rares,
                            "QUALITY" => album.Qualities,
                            "ARTIST" => album.Artists,
                            "CHARACTER" => album.Characters,
                            "LANGUAGE" => album.Languages,
                            _ => new List<string>()
                        };

                if(val.IndexOf('|') != -1) {
                    var splitVals = val.Split('|');
                    return splitVals.Any(v => arrOperator(attributeArr, v));
                }
                return arrOperator(attributeArr, val);
            }

            if(stringKeys.Contains(key)) {
                Func<string, string, bool> strOperator = 
                    connector == ':' ? (a, b) => a.Contains(b, comparer) 
                    : connector == '=' ? (a, b) => a.Equals(b, comparer)
                    : (a, b) => !a.Equals(b, comparer);

                var attribute = key switch
                {
                    "FULLTITLE" => album.GetFullTitleDisplay(),
                    "TITLE" => album.Title,
                    "CATEGORY" => album.Category,
                    "ORIENTATION" => album.Orientation,
                    "NOTE" => album.Note ?? "",
                    "TIER" => album.Tier.ToString(),
                    "ISWIP" => album.IsWip.ToString(),
                    "ISREAD" => album.IsRead.ToString(),
                    "ENTRYDATE" => album.EntryDate.ToString("yyyyMMdd"),
                    _ => ""
                };

                return strOperator(attribute, val);
            }
            
            if(key.Equals("SPECIAL")) {
                if(val.Equals("Tier>0OrNew", comparer)) {
                    return album.Tier > 0 || !album.IsRead;
                }
                if(val.Equals("Tier>1OrNew", comparer)) {
                    return album.Tier > 1 || !album.IsRead;
                }
                if(val.Equals("ToDelete", comparer)) {
                    return ((album.Tier == 0 && string.IsNullOrEmpty(album.Note))
                            || (album.Tier == 1 && string.IsNullOrEmpty(album.Note)
                                && !album.Artists.Any(artist => featuredArtists.ContainsContains(artist, comparer))
                                && !album.Characters.Any(character => featuredCharacters.ContainsContains(character, comparer)))
                        ) && album.IsRead;
                }
                if(val.Equals("Flagged", comparer)) {
                    return !string.IsNullOrEmpty(album.Note)
                        && !(new string[] { "🌟", "HP", "HERITAGE" }).Contains(album.Note);
                }
                if(val.Equals("FeaturedArtist", comparer)) {
                    return connector == ':' ? album.Artists.Any(artist => featuredArtists.ContainsContains(artist, comparer))
                        : connector == '!' ? album.Artists.Any(artist => !featuredArtists.ContainsContains(artist, comparer))
                        : false;
                }
                if(val.Equals("Recent", comparer) && earliestRecentBatch != null) {
                    return album.EntryDate >= earliestRecentBatch;
                }
            }

            if(key.Equals("WITHIN")) {
                int iVal = int.Parse(val);
                return (now - album.EntryDate) <= TimeSpan.FromDays(iVal);
            }

            throw new Exception("Invalid key: " + key);
        }

        foreach(var querySeg in querySegs) {
            if(!IsMatch(album, querySeg))
                return false;
        }
        return true;
    }

    public static AlbumVM Get(this List<AlbumVM> albumVMs, string path) => albumVMs.FirstOrDefault(a => a.Path == path);

    public static bool ContainsConnector(string source) => source.IndexOfAny(_cons) > -1;

    private static bool strContains(string haystack, string needle) => haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);

    private static bool strEqual(string haystack, string needle) => haystack.Equals(needle, StringComparison.OrdinalIgnoreCase);
}

public class QuerySegment
{
    public char Con { get; set; }
    public string Key { get; set; }
    public string Val { get; set; }

    public string Text { 
        get {
            return $"{Key}{Con}{Val}";
        } 
    }
}