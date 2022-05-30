using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common;

public static class CommonSettings
{
    private static Stream? GetSettings()
    {
        var assembly = typeof(CommonSettings).Assembly;
        var memoryStream = new MemoryStream();

        var name = assembly.GetManifestResourceNames().First(name => name.EndsWith(".commonSettings.json"));
        assembly.GetManifestResourceStream(name)?.CopyTo(memoryStream);

        return memoryStream;
    }

    public static IConfigurationBuilder AddCommonSettings(this IConfigurationBuilder builder)
    {
        var stream = GetSettings();
        builder.AddJsonStream(stream);
        return builder;
    }
}
