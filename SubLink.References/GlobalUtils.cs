using System.IO;

namespace xyz.yewnyx.SubLink;

public static class GlobalUtils
{
    private const string CSettingsDir = "Settings";

    public static string GetSettingsFilePath(string platformName) {
        if (!Directory.Exists(CSettingsDir))
            Directory.CreateDirectory(CSettingsDir);

        return Path.Combine(CSettingsDir, $"{platformName}.json");
    }
}
