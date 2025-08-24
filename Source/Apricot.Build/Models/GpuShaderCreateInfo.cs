using System.Text.Json.Serialization;

namespace Apricot.Build.Models;

/// <summary>
/// Represents JSON output of SDL_shadercross.
/// </summary>
public class SdlShaderProgramInfo
{
    [JsonPropertyName("samplers")]
    public int SamplersCount { get; set; }

    [JsonPropertyName("storage_textures")]
    public int StorageTexturesCount { get; set; }

    [JsonPropertyName("uniform_buffers")]
    public int UniformBuffersCount { get; set; }

    [JsonPropertyName("inputs")]
    public Resource[] Inputs { get; set; } = [];

    [JsonPropertyName("outputs")]
    public Resource[] Outputs { get; set; } = [];

    public struct Resource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("location")]
        public int Location { get; set; }
    }
}
