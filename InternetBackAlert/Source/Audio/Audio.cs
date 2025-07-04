namespace InternetBackAlert.Source.Audio;

public enum AudioType
{
    Mp3,
    Ogg,
    Wav
}

public class Audio
{
    public nint Handle { get; private set; }
    public int Volume { get; private set; }
    public bool Loop { get; private set; }

    public Audio(nint handle, int volume, bool loop)
    {
        Handle = handle;
        Volume = volume;
        Loop = loop;
    }
}
