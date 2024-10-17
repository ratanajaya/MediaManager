using System.Text.Json;

namespace SharedLibrary.Helpers;

public static class ObjectCloner
{
    public static T Clone<T>(this T source) {
        var bytes = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<T>(bytes);
    }
}