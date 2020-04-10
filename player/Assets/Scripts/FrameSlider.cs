using UnityEngine;
using UnityEngine.UI;


public class FrameSlider : MonoBehaviour
{
    public Slider Slider;
    public Video Video;

    /*
     * wraps UnityEngine.UI.Slider.value property so that we
     * can store current frame as uint (rather then float)
     */
    long _CurrentFramePTS;
    public long CurrentFramePTS
    {
        get { return _CurrentFramePTS; }
        set
        {
            _CurrentFramePTS = value;
            Slider.value = value;
        }
    }

    void Awake()
    {
        Video.VideoLoadedEvent += Init;
    }

    public void Init(long StartTime, long Duration)
    {
        Slider.minValue = StartTime;
        Slider.maxValue = StartTime + Duration;

        Video.FrameChangedEvent += HandleFrameChanged;
    }

    public void ValueChanged()
    {
        var FramePTS = (long)Slider.value;
        if (FramePTS != CurrentFramePTS)
        {
            /* seek frames only of slider changed by the user */
            Video.Seek(FramePTS);
        }
    }

    void HandleFrameChanged(long FramePTS)
    {
        if (FramePTS != CurrentFramePTS)
        {
            CurrentFramePTS = FramePTS;
        }
    }
}
