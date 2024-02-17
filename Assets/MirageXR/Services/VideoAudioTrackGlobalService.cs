using i5.Toolkit.Core.ServiceCore;
using UnityEngine;

public class VideoAudioTrackGlobalService : IService
{
    public bool UseAudioTrack { get; private set; }

    public void Initialize(IServiceManager owner)
    {
    }

    public void Cleanup()
    {
    }

    public void EnableAudioTrack()
    {
        UseAudioTrack = true;
    }

    public void DisableAudioTrack()
    {
        UseAudioTrack = false;
    }
}
