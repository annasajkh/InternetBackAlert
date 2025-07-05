using System.Text.Json.Serialization;

namespace InternetBackAlert.Source.Data;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SettingsData))]
internal partial class SourceGenerationContext : JsonSerializerContext
{

}

internal readonly record struct SettingsData(string AlertAudioPath, float AlertVolume, bool IsAlertAudioEnabled, bool IsAlertPopupEnabled);