using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using C = SharedLibrary.Constants;

namespace SharedLibrary.Helpers;

public static class AlbumHelper
{
    private static string jpRegex = @"/[\u3000-\u303F]|[\u3040-\u309F]|[\u30A0-\u30FF]|[\uFF00-\uFFEF]|[\u4E00-\u9FAF]|[\u2605-\u2606]|[\u2190-\u2195]|\u203B/g";

    public static (string, List<string>, List<string>, bool) InferMetadataFromName(string name, List<string> subDirNames) {
        string[] elements = name.Split('[', ']', '(', ')', '=');

        var title = elements.Length >= 3 ? elements[2].Trim() : string.Empty;
        var artists = elements.Length >= 3 
            ? elements[1].Split(',').Select(a => a.Trim()).ToList()
            : new List<string>();

        var langFromName = elements.ContainsContains("eng") ? new List<string> { C.Language.English } :
                        elements.ContainsContains("chinese") ? new List<string> { C.Language.Chinese } :
                        Regex.IsMatch(title, jpRegex) ? new List<string> { C.Language.Japanese } :
                        new List<string> { C.Language.English };

        var langFromSubDirNames = new Func<List<string>>(() => {
            bool containEng = subDirNames.Any(a => a.Contains("eng"));
            bool containJp = containEng && subDirNames.Any(a => !a.Contains("eng"));

            var result = new List<string>();
            if(containEng) result.Add(C.Language.English);
            if(containJp) result.Add(C.Language.Japanese);
            return result;
        })();

        var languages = langFromName.Concat(langFromSubDirNames).Distinct().ToList();

        bool isWip = elements.ContainsContains("wip") || elements.ContainsContains("ongoing");

        return (title, artists, languages, isWip);
    }
}