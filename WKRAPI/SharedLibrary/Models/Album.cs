using SharedLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedLibrary.Models
{
    //Used for universal _album.json file
    //Should not contains any informations that: 
    //-Can be infered through filesystem (ie. path, page count)
    //-Is volatile (ie. last page opened)
    //Fields that are inferred from other fields and requires logic to output must be exposed as methods
    public class Album
    {
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string Orientation { get; set; } = "";

        public List<string> Artists { get; set; } = [];
        [Obsolete]
        public List<string> Tags { get; set; } = [];
        public List<string> Povs { get; set; } = [];
        public List<string> Focuses { get; set; } = [];
        public List<string> Others { get; set; } = [];
        public List<string> Rares { get; set; } = [];
        public List<string> Qualities { get; set; } = [];
        public List<string> Characters { get; set; } = [];
        public List<string> Languages { get; set; } = [];
        public string? Note { get; set; } = "";

        public int Tier { get; set; } = 0;

        public bool IsWip { get; set; } = false;
        public bool IsRead { get; set; } = false;

        public DateTime EntryDate { get; set; } = DateTime.Now;

        public Dictionary<string,int> ChapterTier { get; set; } = [];

        public List<Source> Sources { get; set; } = new List<Source>();

        public string GetArtistsDisplay() { return string.Join(", ", Artists); }
        public string GetCharactersDisplay() { return string.Join(", ", Characters); }
        public string GetLanguagesDisplay() { return string.Join(", ", Languages); }

        private string? _fullTitleDisplay;
        public string GetFullTitleDisplay() {
            if(_fullTitleDisplay == null) _fullTitleDisplay = "[" + GetArtistsDisplay() + "] " + Title;
            return _fullTitleDisplay; 
        }

        public void ValidateAndCleanup() {
            Artists = Artists!.CleanListString();
            Characters = Characters!.CleanListString();
            Languages = Languages!.CleanListString();
            Povs = Povs!.CleanListString();
            Focuses = Focuses!.CleanListString();
            Others = Others!.CleanListString();
            Rares = Rares!.CleanListString();
            Qualities = Qualities!.CleanListString();

            Note = !string.IsNullOrWhiteSpace(Note) ? Note : null;
            Sources = Sources.OrderBy(a => a.Title).ThenBy(a => a.Url).ToList();
        }
    }

    public class Source {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public required string Url { get; set; }
    }
}
