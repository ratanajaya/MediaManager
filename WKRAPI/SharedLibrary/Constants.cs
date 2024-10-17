namespace SharedLibrary;

public static class Constants
{
    #region Query Connectors
    public const char ConContain = ':';
    public const char ConEqual = '=';
    public const char ConNot = '!';
    public const char ConGreater = '>';
    public const char ConLesser = '<';
    #endregion

    public const string OnOff_On = "On";
    public const string OnOff_Off = "Off";

    public const string AppTypeApi = "Api";
    public const string AppTypeMetadataEditor = "MetadataEditor";

    public const string Kc_AlbumVm = "AlbumVM";
    public const string Kc_CensorshipStatus = "CStatus";
    public const string Kc_LastModified = "LastModified";

    public static class FileSystem 
    {
        public const string JsonFileName = "_album.json";
        public const string SourceAndContentFileName = "sourceAndContent.json";
    }

    public static class Extension 
    {
        public const string Jpg = ".jpg";
        public const string Jpeg = ".jpeg";
        public const string Jfif = ".jfif";
        public const string Png = ".png";
        public const string Webp = ".webp";

        public const string Gif = ".gif";
        public const string Webm = ".webm";
        public const string Mp4 = ".mp4";
    }

    public static class Orientation 
    {
        public const string Portrait = "Portrait";
        public const string Landscape = "Landscape";
        public const string Auto = "Auto";
    }

    public static class Language 
    {
        public const string English = "English";
        public const string Japanese = "Japanese";
        public const string Chinese = "Chinese";
        public const string Other = "Other";
    }
}