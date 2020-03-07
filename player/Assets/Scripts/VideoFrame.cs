using UnityEngine;
using UnityEngine.UI;

public class VideoFrame : MonoBehaviour
{
    const int WIDTH = 3840;
    const int HEIGHT = 2160;

    RawImage image;
    Texture2D tex;
    Video video;

    long CurrFrame = 0;

    void Awake()
    {
        //video = new Video("/home/boris/area51/uffplay/video/foo.mov");
        video = new Video("/home/boris/area51/uffplay/video/DJI_0001.MOV");

        tex = new Texture2D(video.Width, video.Height, TextureFormat.RGB24, false);
        image = gameObject.GetComponent<RawImage>();
        image.texture = tex;
    }

    void Update()
    {
        if (video == null)
        {
            /* playback finished */
            return;
        }

        if (CurrFrame >= video.NumFrames)
        {
            Log.Msg("end of video");
            video.Close();
            video = null;

            return;
        }

        var data = tex.GetRawTextureData<byte>();
        video.GetFrame(data);
        tex.Apply();

        CurrFrame += 1;
    }
}
