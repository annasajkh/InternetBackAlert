using InternetBackAlert.Source.Bindings.SDL2;

namespace InternetBackAlert.Source.Audio;

/// <summary>
/// Something that play music
/// </summary>
public class MusicPlayer
{
    public Music? Music { get; private set; }

    private bool paused;

    /// <summary>
    /// Set or get the paused stus of the music
    /// </summary>
    public bool Paused
    {
        get
        {
            return paused;
        }

        set
        {
            paused = value;

            if (paused)
            {
                SDL_mixer.Mix_PauseMusic();
            }
            else
            {
                SDL_mixer.Mix_ResumeMusic();
            }
        }
    }

    /// <summary>
    /// The position of the music while it's playing
    /// </summary>
    public double Position
    {
        get
        {
            if (Music is null)
            {
                throw new Exception("There is no music source");
            }

            return SDL_mixer.Mix_GetMusicPosition(Music.Handle);

        }

        set
        {
            SDL_mixer.Mix_SetMusicPosition(value);
        }
    }

    /// <summary>
    /// Check if the music player is playing
    /// Paused music is treated as playing, even though it is not currently making forward progress in mixing.
    /// </summary>
    public bool Playing
    {
        get
        {
            return SDL_mixer.Mix_PlayingMusic() > 0;
        }
    }

    /// <summary>
    /// This event will fire when the music is finish playing
    /// Note it won't work if looping is enable on the music
    /// </summary>
    public event Action? OnFinished;
    private SDL_mixer.MusicFinishedDelegate musicFinishedDelegate;

    public MusicPlayer()
    {
        Paused = false;

        musicFinishedDelegate = FinishedCallback;
        SDL_mixer.Mix_HookMusicFinished(musicFinishedDelegate);
    }

    private void FinishedCallback()
    {
        OnFinished?.Invoke();
    }

    /// <summary>
    /// Set the music to be played
    /// </summary>
    public void SetSource(Music music)
    {
        Music = music;
    }

    public void SetVolume(int volume)
    {
        SDL_mixer.Mix_VolumeMusic(volume);
    }

    /// <summary>
    /// Play the music
    /// </summary>
    public void Play()
    {
        if (Music is Music music)
        {
            int volumeError = SDL_mixer.Mix_VolumeMusic(music.Volume);
            if (volumeError == 0)
            {
                throw new Exception("Cannot set volume");
            }

            SDL_mixer.Mix_PlayMusic(music.Handle, music.Loop ? -1 : 0);
        }
    }

    /// <summary>
    /// Halt the music
    /// </summary>
    public int Halt()
    {
        return SDL_mixer.Mix_HaltMusic();
    }

    public void Rewind()
    {
        SDL_mixer.Mix_RewindMusic();
    }

    public void SetCMD(string command)
    {
        SDL_mixer.Mix_SetMusicCMD(command);
    }

    public SDL_mixer.Mix_Fading Fading()
    {
        return SDL_mixer.Mix_FadingMusic();
    }
}
