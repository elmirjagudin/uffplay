using UnityEngine;
using UnityEngine.UI;

public class PlayPause : MonoBehaviour
{
    public Video Video;
    public Sprite Play;
    public Sprite Pause;

    void Awake()
    {
        Video.VideoPlayingEvent += HadleVideoPlaying;
        Video.VideoPausedEvent += HadleVideoPaused;
    }

    Image CurrentImage()
    {
        return GetComponent<Image>();
    }

    bool IsPaused()
    {
        return CurrentImage().sprite == Play;
    }

    public void Toggle()
    {
        if (IsPaused())
        {
            Video.Play();
        }
        else
        {
            Video.Pause();
        }
    }

    void SetSprite(Sprite sprite)
    {
        CurrentImage().sprite = sprite;
    }

    void HadleVideoPlaying()
    {
        SetSprite(Pause);
    }

    void HadleVideoPaused()
    {
        SetSprite(Play);
    }
}
