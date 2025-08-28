using Apricot.Assets;
using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Sdl.Importers;

public static class Injection
{
    /// <summary>
    /// Adds importers that build artifacts suitable for use with SDL backend.
    /// </summary>
    public static IServiceCollection AddSdlImporters(this IServiceCollection services) => services
        .AddSingleton<IAssetsImporter, HlslSdlShaderImporter>();
}
