using System.Text.Json;

namespace Apricot.Utils;

public static class JsonImport
{
    public static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
}
