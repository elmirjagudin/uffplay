using UnityEngine;
using UnityEngine.UI;

public class VideoFrame : MonoBehaviour
{
    const int WIDTH = 3840;
    const int HEIGHT = 2160;

    RawImage image;
    Texture2D tex;
    Video video;

    int ctr = 0;

    void Awake()
    {
        tex = new Texture2D(WIDTH, HEIGHT, TextureFormat.RGB24, false);
        image = gameObject.GetComponent<RawImage>();
        image.texture = tex;

        video = new Video("/home/boris/area51/uffplay/video/DJI_0001.MOV");
    }

    void Update()
    {
        if (ctr > 128)
        {
            return;
        }

        ctr += 1;

        var data = tex.GetRawTextureData<byte>();
        video.GetFrame(data);
        tex.Apply();
    }
}
