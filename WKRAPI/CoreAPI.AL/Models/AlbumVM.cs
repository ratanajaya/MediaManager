using SharedLibrary.Models;

namespace CoreAPI.AL.Models;

public class AlbumVM
{
    public required string Path { get; set; }
    public int PageCount { get; set; }
    public int LastPageIndex { get; set; }
    public int CorrectablePageCount { get; set; }
    public FileInfoModel? CoverInfo { get; set; }
    public required Album Album { get; set; }

    public int GetChapterTier(string chapterName) { 
        if(Album.ChapterTier.ContainsKey(chapterName))
            return Album.ChapterTier[chapterName];

        return 0;
    }

    public void UpdateChapterTier(string chapterName, int tier) {
        if(Album.ChapterTier.ContainsKey(chapterName))
            Album.ChapterTier[chapterName] = tier;
        else
            Album.ChapterTier.Add(chapterName, tier);
    }

    public void RemoveChapter(string chapterName) {
        if(Album.ChapterTier.ContainsKey(chapterName))
            Album.ChapterTier.Remove(chapterName);
    }

    public void RenameChapter(string oldName, string newName) {
        var oldTier = GetChapterTier(oldName);
        RemoveChapter(oldName);
        UpdateChapterTier(newName, oldTier);
    }
}