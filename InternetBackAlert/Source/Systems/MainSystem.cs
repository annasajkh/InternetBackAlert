using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using InternetBackAlert.Source.Audio;
using InternetBackAlert.Source.Data;
using InternetBackAlert.Source.UIs.Containers;
using InternetBackAlert.Source.Utils;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;
using System.Timers;
using Timer = System.Timers.Timer;

namespace InternetBackAlert.Source.Systems;

enum ConnectionHistory
{
    Connected,
    Disconnected
}


internal class MainSystem : IDisposable
{
    Thread playAudioThread;
    CancellationTokenSource cancellationTokenSource = new();

    Timer checkConnectionTimer = new(interval: 500);
    Timer DisconnectCheckingTimer = new(interval: 5000);
    Timer autoSaveTimer = new(interval: 500);

    bool isDisposed;
    volatile bool isConnected = Helper.IsConnectedToTheInternet();
    bool possiblyDisconnected;
    bool isConfirmedDisconnected;

    List<ConnectionHistory> connectionHistories = new();
    List<ConnectionHistory> connectionPossiblyDisconnectedHistories = new();
    List<Music> audioSources = new();

    readonly object connectionHistoriesLock = new object();

    SettingsData currentSettingsData;
    SettingsData oldSettingsData;


    MainContainer mainContainer;

    public MainSystem(MainContainer mainContainer)
    {
        this.mainContainer = mainContainer;

        playAudioThread = new Thread(playAlertAudio);
        playAudioThread.Start();

        checkConnectionTimer.Elapsed += (object? source, ElapsedEventArgs elapsedEventArgs) =>
        {
            isConnected = Helper.IsConnectedToTheInternet();

            if (possiblyDisconnected && !isConnected)
            {
                connectionPossiblyDisconnectedHistories.Add(ConnectionHistory.Disconnected);
            }

            lock (connectionHistoriesLock)
            {
                connectionHistories.Add(isConnected ? ConnectionHistory.Connected : ConnectionHistory.Disconnected);

                if (connectionHistories.Count > 2)
                {
                    connectionHistories.RemoveAt(0);
                }

#if DEBUG
                if (connectionHistories.Count == 2)
                {
                    Console.WriteLine($"[{connectionHistories[0]}, {connectionHistories[1]}]");
                }
#endif
            }

            if (mainContainer.IsConnectedToTheInternetTextBlock is not null)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (!possiblyDisconnected)
                    {
                        mainContainer.IsConnectedToTheInternetTextBlock.Text = $"You are {(isConnected ? "connected" : "disconnected")} from the internet";
                    }
                });
            }
        };

        DisconnectCheckingTimer.Elapsed += (object? source, ElapsedEventArgs elapsedEventArgs) =>
        {
            if (connectionPossiblyDisconnectedHistories.Count >= 9)
            {
                isConfirmedDisconnected = true;
            }

            connectionPossiblyDisconnectedHistories.Clear();

            lock (connectionHistoriesLock)
            {
                connectionHistories.Clear();
            }

            possiblyDisconnected = false;
        };

        autoSaveTimer.Elapsed += (object? source, ElapsedEventArgs elapsedEventArgs) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (mainContainer is not null && mainContainer.AlertTextBox is not null && mainContainer.AlertTextBox.Text is not null && mainContainer.VolumeSlider is not null && !mainContainer.isLoadingSettings)
                {
                    currentSettingsData = new SettingsData()
                    {
                        AlertAudioPath = mainContainer.AlertTextBox.Text,
                        AlertVolume = (float)mainContainer.VolumeSlider.Value,
                        IsAlertEnabled = mainContainer.IsAlertEnabled
                    };

                    if (currentSettingsData != oldSettingsData)
                    {
                        Settings.Save(currentSettingsData);

                        oldSettingsData = currentSettingsData;
                    }
                }
            });
        };

        DisconnectCheckingTimer.AutoReset = false;

        checkConnectionTimer.AutoReset = true;
        checkConnectionTimer.Start();

        autoSaveTimer.AutoReset = true;
        autoSaveTimer.Start();

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (mainContainer is not null && mainContainer.VolumeSlider is not null)
            {
                mainContainer.VolumeSlider.ValueChanged += (object? sender, RangeBaseValueChangedEventArgs rangeBaseValueChangedEventArgs) =>
                {
                    Global.MusicPlayer?.SetVolume((int)(mainContainer.VolumeSlider.Value * 100));
                };
            }
        });
    }

    void playAlertAudio()
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            lock (connectionHistoriesLock)
            {
                if (connectionHistories.Count == 2 && connectionHistories[0] == ConnectionHistory.Disconnected && connectionHistories[1] == ConnectionHistory.Disconnected && !possiblyDisconnected && !isConfirmedDisconnected)
                {
                    DisconnectCheckingTimer.Start();

                    if (mainContainer.IsConnectedToTheInternetTextBlock is not null)
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            mainContainer.IsConnectedToTheInternetTextBlock.Text = $"You are possibly disconnected from the internet";
                        });
                    }

                    possiblyDisconnected = true;
                }
            }

            if (isConfirmedDisconnected)
            {
                lock (connectionHistoriesLock)
                {
                    if (connectionHistories.Count == 2 && connectionHistories[0] == ConnectionHistory.Disconnected && connectionHistories[1] == ConnectionHistory.Connected)
                    {
                        if (mainContainer.IsAlertEnabled)
                        {
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                if (mainContainer.AlertTextBox is not null && mainContainer.AlertTextBox.Text == "")
                                {
                                    return;
                                }
                            });

                            try
                            {
                                Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    if (mainContainer is not null && mainContainer.VolumeSlider is not null && mainContainer.AlertTextBox is not null && mainContainer.AlertTextBox.Text is not null)
                                    {
                                        Music music = Music.Load(mainContainer.AlertTextBox.Text, AudioType.Mp3, volume: (int)(mainContainer.VolumeSlider.Value * 100));

                                        if (music is not null && Global.MusicPlayer is not null)
                                        {
                                            Global.MusicPlayer.SetSource(music);
                                            Global.MusicPlayer.OnFinished += () =>
                                            {
                                                music.Dispose();
                                                audioSources.Remove(music);
                                            };

                                            audioSources.Add(music);

                                            Global.MusicPlayer.Play();
                                        }
                                    }

                                    if (Global.lifetime is not null && Global.lifetime.MainWindow is not null)
                                    {
                                        Global.lifetime.MainWindow.Activate();
                                    }
                                });
                            }
                            catch (FileNotFoundException)
                            {
                                Dispatcher.UIThread.InvokeAsync(async () =>
                                {
                                    IMsBox<ButtonResult> box = MessageBoxManager.GetMessageBoxStandard("Error", "File Not found", ButtonEnum.Ok, Icon.Error);
                                    await box.ShowAsync();
                                });
                            }
                            catch (Exception exception)
                            {
                                Dispatcher.UIThread.InvokeAsync(async () =>
                                {
                                    IMsBox<ButtonResult> box = MessageBoxManager.GetMessageBoxStandard("Error", exception.ToString(), ButtonEnum.Ok, Icon.Error);
                                    await box.ShowAsync();
                                });
                            }
                        }

                        isConfirmedDisconnected = false;
                    }
                }
            }

            Thread.Sleep(100);
        }
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;

        cancellationTokenSource.Cancel();
    }
}
