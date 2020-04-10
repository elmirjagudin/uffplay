using UnityEngine;
using UnityEngine.UI;

public class Video : MonoBehaviour
{
    public delegate void VideoLoaded(long StartTime, long Duration);
    public static event VideoLoaded VideoLoadedEvent;

    public delegate void VideoPlaying();
    public static event VideoPlaying VideoPlayingEvent;

    public delegate void VideoPaused();
    public static event VideoPlaying VideoPausedEvent;

    public delegate void FrameChanged(long FramePTS);
    public static event FrameChanged FrameChangedEvent;


    VideoDecoder video;
    RawImage image;
    Texture2D tex;

    enum State
    {
        Uninitialized,
        Playing,
        Paused,
        Finished,
    }

    State VideoState = State.Uninitialized;

    public void Open(string FileName)
    {
        Log.Msg("Open video {0}", FileName);
        video = new VideoDecoder(FileName);

        tex = new Texture2D(video.Width, video.Height, TextureFormat.RGB24, false);
        image = gameObject.GetComponent<RawImage>();
        image.texture = tex;

        VideoLoadedEvent?.Invoke(video.StartTime, video.Duration);

        Play();
    }

    public void Play()
    {
        VideoState = State.Playing;
        VideoPlayingEvent?.Invoke();
    }

    public void Pause()
    {
        VideoState = State.Paused;
        VideoPausedEvent?.Invoke();
    }

    public void Seek(long FramePTS)
    {
        video.Seek(FramePTS);
    }

    void LoadNextFrame()
    {
        try
        {
            long frame_pts;
            var data = tex.GetRawTextureData<byte>();
            video.GetFrame(data, out frame_pts);
            tex.Apply();
            FrameChangedEvent?.Invoke(frame_pts);
            Log.Msg("showing frame with pts {0}", frame_pts);
        }
        catch (EndOfVideo)
        {
            VideoState = State.Finished;
            Log.Msg("End of Video");
        }
    }

    void Update()
    {
        switch (VideoState)
        {
            case State.Uninitialized:
                /* nop */
                break;
            case State.Playing:
                LoadNextFrame();
                break;
            default:
                break;
        }
    }
}
