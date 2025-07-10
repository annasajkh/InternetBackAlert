using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using InternetBackAlert.Source.Data;
using InternetBackAlert.Source.Utils;
using Projektanker.Icons.Avalonia;

namespace InternetBackAlert.Source.UIs.Containers;

internal class MainContainer : ComponentBase
{
    internal bool IsAlertAudioEnabled { get; private set; } = true;
    internal bool IsAlertPopupEnabled { get; private set; } = true;
    internal TextBlock? IsConnectedToTheInternetTextBlock { get; private set; }
    internal Slider? VolumeSlider { get; private set; }
    internal bool isLoadingSettings { get; private set; } = true;
    internal bool IsSettingsExistAndLoaded { get; set; }
    internal TextBox? AlertTextBox { get; private set; }

    async void AudioAlertBrowserButtonPressedAsync(RoutedEventArgs routedEventArgs)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is null)
        {
            throw new Exception("Cannot open file");
        }

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Pick audio file",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("Audio files") { Patterns = ["*.mp3"] }]
        });

        if (files.Count >= 1)
        {
            string? localPath = files[0].TryGetLocalPath();

            if (AlertTextBox is not null)
            {
                AlertTextBox.Text = localPath;
            }
        }
    }


    void AlertAudioToggleOnIsCheckedChanged(RoutedEventArgs routedEventArgs)
    {
        ToggleSwitch? toggleSwitch = (ToggleSwitch?)routedEventArgs.Source;

        if (toggleSwitch is ToggleSwitch toggleSwitchTemp && toggleSwitchTemp.IsChecked is bool toggleSwitchTempChecked)
        {
            IsAlertAudioEnabled = toggleSwitchTempChecked;
        }
    }


    void AlertPopupToggleOnIsCheckedChanged(RoutedEventArgs routedEventArgs)
    {
        ToggleSwitch? toggleSwitch = (ToggleSwitch?)routedEventArgs.Source;

        if (toggleSwitch is ToggleSwitch toggleSwitchTemp && toggleSwitchTemp.IsChecked is bool toggleSwitchTempChecked)
        {
            IsAlertPopupEnabled = toggleSwitchTempChecked;
        }
    }

    protected override object Build()
    {
        Control ui = new Grid()
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .VerticalAlignment(VerticalAlignment.Stretch)
                    .Rows("*, 270 *")
                    .Cols("*, 480, *")
                    .Children(
                        new Border()
                        .Row(1)
                        .Col(1)
                        .BorderBrush(Color.FromRgb(46, 47, 51).ToBrush())
                        .Background(Color.FromRgb(35, 36, 41).ToBrush())
                        .BorderThickness(1)
                        .CornerRadius(8)
                        .Child(
                            new StackPanel()
                            .Margin(32, 0)
                            .Spacing(8)
                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Children(
                                new TextBlock()
                                .Ref(out TextBlock IsConnectedToTheInternetTextBlockTemp)
                                .HorizontalAlignment(HorizontalAlignment.Center)
                                .VerticalAlignment(VerticalAlignment.Center)
                                .Padding(0, 0, 0, 18)
                                .TextWrapping(TextWrapping.Wrap)
                                .FontSize(16)
                                .Foreground(Brushes.White),

                                new Grid()
                                {
                                    ColumnSpacing = 4
                                }
                                .Rows("32")
                                .Cols("Auto, *, 32")
                                .Children(
                                    new TextBlock()
                                    .TextWrapping(TextWrapping.Wrap)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .FontSize(14)
                                    .Foreground(Brushes.White)
                                    .Text("Alert audio path:")
                                    .Row(0)
                                    .Col(0),

                                    new TextBox()
                                    .Text("")
                                    .Ref(out TextBox alertTextBoxTemp)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Row(0)
                                    .Col(1),

                                    new Button()
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Row(0)
                                    .Col(2)
                                    .Content(
                                        new Image()
                                        .Width(16)
                                        .Height(16)
                                        .Source(
                                            new IconImage()
                                            {
                                                Value = "fa-solid fa-ellipsis",
                                                Brush = Brushes.White
                                            }
                                        )
                                    )
                                    .OnClick(AudioAlertBrowserButtonPressedAsync)
                                ),

                                new Grid()
                                {
                                    ColumnSpacing = 4
                                }
                                .Rows("32")
                                .Cols("Auto, *")
                                .Children(
                                    new TextBlock()
                                    .TextWrapping(TextWrapping.Wrap)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .FontSize(14)
                                    .Row(0)
                                    .Col(0)
                                    .Foreground(Brushes.White)
                                    .Text("Volume:"),

                                    new Slider()
                                        .VerticalAlignment(VerticalAlignment.Center)
                                        .Row(0)
                                        .Col(1)
                                        .Value(100)
                                        .Minimum(0)
                                        .Maximum(1)
                                        .Ref(out Slider volumeSliderTemp)
                                ),

                                new Grid()
                                {
                                    ColumnSpacing = 4
                                }
                                .Rows("32")
                                .Cols("*, Auto")
                                .Children(
                                    new TextBlock()
                                    .Row(0)
                                    .Col(0)
                                    .TextWrapping(TextWrapping.Wrap)
                                    .FontSize(14)
                                    .Foreground(Brushes.White)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Text("Enable alert audio"),

                                    new ToggleSwitch()
                                    .IsChecked(true)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .OnIsCheckedChanged(AlertAudioToggleOnIsCheckedChanged)
                                    .Ref(out ToggleSwitch alertAudioToggleSwitchTemp)
                                    .Row(0)
                                    .Col(1)
                                ),

                                new Grid()
                                {
                                    ColumnSpacing = 4
                                }
                                .Rows("32")
                                .Cols("*, Auto")
                                .Children(
                                    new TextBlock()
                                    .Row(0)
                                    .Col(0)
                                    .TextWrapping(TextWrapping.Wrap)
                                    .FontSize(14)
                                    .Foreground(Brushes.White)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Text("Enable alert popup"),

                                    new ToggleSwitch()
                                    .IsChecked(true)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .OnIsCheckedChanged(AlertPopupToggleOnIsCheckedChanged)
                                    .Ref(out ToggleSwitch alertPopupToggleSwitchTemp)
                                    .Row(0)
                                    .Col(1)
                                )
                            )
                        )
                    );

        AlertTextBox = alertTextBoxTemp;
        IsConnectedToTheInternetTextBlock = IsConnectedToTheInternetTextBlockTemp;
        VolumeSlider = volumeSliderTemp;


        if (isLoadingSettings)
        {
            SettingsData? settingsData = Settings.Load();

            if (settingsData is not null)
            {
                AlertTextBox.Text = settingsData.Value.AlertAudioPath;

                volumeSliderTemp.Value = settingsData.Value.AlertVolume;

                alertAudioToggleSwitchTemp.IsChecked = settingsData.Value.IsAlertAudioEnabled;
                IsAlertAudioEnabled = settingsData.Value.IsAlertAudioEnabled;

                alertPopupToggleSwitchTemp.IsChecked = settingsData.Value.IsAlertPopupEnabled;
                IsAlertPopupEnabled = settingsData.Value.IsAlertPopupEnabled;

                IsSettingsExistAndLoaded = true;
            }


            isLoadingSettings = false;
        }

        return ui;
    }
}
