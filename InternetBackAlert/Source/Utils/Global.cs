using Avalonia.Controls.ApplicationLifetimes;
using InternetBackAlert.Source.Audio;

namespace InternetBackAlert.Source.Utils;

internal static class Global
{
    internal static ClassicDesktopStyleApplicationLifetime? lifetime;
    internal static MusicPlayer? MusicPlayer { get; private set; } = new();
}
