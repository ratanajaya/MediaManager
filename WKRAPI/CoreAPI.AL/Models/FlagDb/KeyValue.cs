using SQLite;

namespace CoreAPI.AL.Models.FlagDb;

public class KeyValue
{
    public const string KeyLastModified = "LastModified";
    public const string KeyCensorshipStatus = "CensorshipStatus";
    public const string OnOff_On = "On";
    public const string OnOff_Off = "Off";

    public const string AppTypeApi = "Api";
    public const string AppTypeMetadataEditor = "MetadataEditor";

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}