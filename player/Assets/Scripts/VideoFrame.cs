using UnityEngine;
using UnityEngine.UI;

public class VideoFrame : MonoBehaviour
{
    public delegate void VideoLoaded(uint FirstFrame, uint LastFrame);
    public static event VideoLoaded VideoLoadedEvent;

    public delegate void FrameChanged(uint FrameNumer);
    public static event FrameChanged FrameChangedEvent;

    const int WIDTH = 3840;
    const int HEIGHT = 2160;

    RawImage image;
    Texture2D tex;
    VideoDecoder video;

    void Awake()
    {
        //video = new Video("/home/boris/area51/uffplay/video/foo.mov");
        video = new VideoDecoder("/home/boris/area51/old_uffplay/video/DJI_0001.MOV");

        tex = new Texture2D(video.Width, video.Height, TextureFormat.RGB24, false);
        image = gameObject.GetComponent<RawImage>();
        image.texture = tex;
    }

    void Start()
    {
        Log.Msg("start {0} duration {1}", video.StartTime, video.Duration);
        VideoLoadedEvent?.Invoke((uint)video.StartTime,
                                 (uint)video.StartTime + (uint)video.Duration);
    }

    public void SeekFrame(long FramePTS)
    {
        video.Seek(FramePTS);
    }

    void Update()
    {
        if (video == null)
        {
            /* playback finished */
            return;
        }

        try
        {
            long frame_pts;
            var data = tex.GetRawTextureData<byte>();
            video.GetFrame(data, out frame_pts);
            tex.Apply();
            FrameChangedEvent?.Invoke((uint)frame_pts);
            Log.Msg("showing frame with pts {0}", frame_pts);
        }
        catch (EndOfVideo)
        {
            video = null;
            Log.Msg("End of Video");
        }
    }
}
