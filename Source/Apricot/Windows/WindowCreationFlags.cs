using System.Text.Json.Serialization;

namespace Apricot.Windows;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WindowCreationFlags
{
    None = 0,
    Resizable = 1,
    Fullscreen = 1 << 1,
    AlwaysOnTop = 1 << 2,
    HiDpi = 1 << 3
}
