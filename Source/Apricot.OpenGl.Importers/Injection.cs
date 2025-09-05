using Apricot.Assets;
using Microsoft.Extensions.DependencyInjection;

namespace Apricot.OpenGl.Importers;

public static class Injection
{
    public static IServiceCollection AddOpenGlImporters(this IServiceCollection services) => services
        .AddSingleton<IAssetsImporter, HlslToGlslImporter>();
}
