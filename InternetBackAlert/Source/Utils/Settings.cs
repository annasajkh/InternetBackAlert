using InternetBackAlert.Source.Data;
using System.Text.Json;

namespace InternetBackAlert.Source.Utils;

internal static class Settings
{
    static string saveDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "InternetBackAlert");
    static string saveDataFilePath = Path.Combine(saveDataPath, "settings.json");

    internal static SettingsData? Load()
    {
        if (Directory.Exists(saveDataPath))
        {
            return JsonSerializer.Deserialize(File.ReadAllText(saveDataFilePath), SourceGenerationContext.Default.SettingsData);
        }

        return null;
    }

    internal static void Save(SettingsData settingsData)
    {
        string settingsDataStr = JsonSerializer.Serialize(settingsData, SourceGenerationContext.Default.SettingsData);

        if (!Directory.Exists(saveDataPath))
        {
            Directory.CreateDirectory(saveDataPath);
        }

        File.WriteAllText(saveDataFilePath, settingsDataStr);
    }
}
