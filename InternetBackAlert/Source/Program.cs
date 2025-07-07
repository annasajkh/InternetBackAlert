using Avalonia;
using Avalonia.Controls;
using InternetBackAlert.Source.Bindings.SDL2;
using InternetBackAlert.Source.Systems;
using InternetBackAlert.Source.UIs.Containers;
using InternetBackAlert.Source.Utils;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Semi.Avalonia;

namespace InternetBackAlert.Source;

static internal class Program
{
    static void Main()
    {
        if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) < 0)
        {
            string errorMessage = SDL.SDL_GetError();
            Console.WriteLine($"Cannot initialize SDL Audio: {errorMessage}");
        }
        else
        {
            SDL_mixer.Mix_OpenAudio(frequency: 44100, format: SDL.AUDIO_S16SYS, channels: 2, chunksize: 2048);
        }


        Global.lifetime = new()
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose
        };

        IconProvider.Current
            .Register<FontAwesomeIconProvider>();

        AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .AfterSetup(appBuilder => appBuilder.Instance?.Styles.Add(new SemiTheme()))
            .SetupWithLifetime(Global.lifetime);

        MainContainer mainContainer = new();


        Global.lifetime.MainWindow = new Window()
        {
            Title = "Internet Back Alert",
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Width = 960,
            Height = 540,
            Content = mainContainer,
        };

        Global.lifetime.MainWindow.Icon = new WindowIcon(Path.Combine("Assets", "Icons", "icon.ico"));

#if DEBUG
        Global.lifetime.MainWindow.AttachDevTools();
#endif

        MainSystem mainSystem = new(mainContainer);

        Global.lifetime.Start();

        mainSystem.Dispose();
    }
}
