using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class VideoFrame : MonoBehaviour
{
    const int WIDTH = 3840;
    const int HEIGHT = 2160;

    RawImage image;
    Texture2D tex;

    void Awake()
    {
        tex = new Texture2D(WIDTH, HEIGHT, TextureFormat.RGB24, false);
        image = gameObject.GetComponent<RawImage>();
        image.texture = tex;

        tex.LoadRawTextureData(File.ReadAllBytes("/home/elmjag/area51/uffplay/video/foo"));
        tex.Apply();
    }
}
