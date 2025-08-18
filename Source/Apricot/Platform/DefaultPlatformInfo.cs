using System.Runtime.InteropServices;
using Apricot.Common;
using Apricot.Graphics;

namespace Apricot.Platform;

// todo: graphics as a dependency is not very nice
public class DefaultPlatformInfo(IGraphics graphics) : IPlatformInfo
{
    public RuntimePlatform Platform
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return RuntimePlatform.OSX;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return RuntimePlatform.Linux;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return RuntimePlatform.Windows;

            if (OperatingSystem.IsBrowser())
                return RuntimePlatform.Web;

            return RuntimePlatform.Unknown;
        }
    }

    public GraphicDriver GraphicDriver => graphics.Driver;
}
