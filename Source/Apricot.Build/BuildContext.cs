using Cake.Common;
using Cake.Core;
using Cake.Frosting;

public class BuildContext(ICakeContext context) : FrostingContext(context)
{
    public string SdlCmakeGenerator { get; } = context.Argument("sdlCmakeGenerator", "Ninja");

    public string? ShadercrossBinary { get; set; } = context.Argument<string?>("shadercrossBinary", null);
}