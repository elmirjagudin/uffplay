using UnityEngine;
using UnityEngine.UI;


public class FrameSlider : MonoBehaviour
{
    public Slider Slider;
    public VideoFrame Video;

    /*
     * wraps UnityEngine.UI.Slider.value property so that we
     * can store current frame as uint (rather then float)
     */
    uint _CurrentFramePTS;
    public uint CurrentFramePTS
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
        VideoFrame.VideoLoadedEvent += Init;
    }

    public void Init(uint FirstFrame, uint LastFrame)
    {
        Slider.minValue = FirstFrame;
        Slider.maxValue = LastFrame;

        VideoFrame.FrameChangedEvent += HandleFrameChanged;
    }

    public void ValueChanged()
    {
        var FramePTS = (uint)Slider.value;
        if (FramePTS != CurrentFramePTS)
        {
            /* seek frames only of slider changed by the user */
            Video.SeekFrame(FramePTS);
        }
    }

    void HandleFrameChanged(uint FrameNumber)
    {
        if (FrameNumber != CurrentFramePTS)
        {
            CurrentFramePTS = FrameNumber;
        }
    }
}
