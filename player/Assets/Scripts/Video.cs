using System;
using System.Runtime.InteropServices;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


public class EndOfVideo : Exception {}

public class Video
{
    [DllImport("video")]
    static extern int video_open(ref IntPtr decoder, string filename);

    [DllImport("video")]
    static extern int video_seek_frame(IntPtr decoder, long frame_pts);

    [DllImport("video")]
    static unsafe extern int video_decode_frame(IntPtr decoder, void *pixels, out long frame_pts);

    [DllImport("video")]
    static extern void video_stream_info(IntPtr decoder,
                                         out int width, out int height,
                                         out long start_time, out long duration);

    [DllImport("video")]
    static extern void video_close(IntPtr decoder);

    public int Width { get { return _Width; }}
    public int Height { get { return _Height; }}
    public long StartTime { get { return _StartTime; }}
    public long Duration { get { return _Duration; }}

    IntPtr decoder;

    int _Width;
    int _Height;
    long _StartTime;
    long _Duration;

    public Video(string filename)
    {
        var ret = video_open(ref decoder, filename);
        if (ret != 0)
        {
            throw new Exception("video_open() error " + ret);
        }

        video_stream_info(decoder,
                          out _Width, out _Height,
                          out _StartTime, out _Duration);
    }

    public void Seek(long FramePTS)
    {
        var ret = video_seek_frame(decoder, FramePTS);
        if (ret != 0)
        {
            throw new Exception("video_seek_frame() error " + ret);
        }
    }

    public void GetFrame(NativeArray<byte> pixels, out long pts)
    {
        unsafe
        {
            void *ptr = NativeArrayUnsafeUtility.GetUnsafePtr(pixels);
            var ret = video_decode_frame(decoder, &ptr, out pts);
            if (ret == -1)
            {
                throw new EndOfVideo();
            }

            if (ret != 0)
            {
                throw new Exception("video_decode_frame() error " + ret);
            }
        }
    }

    public void Close()
    {
        video_close(decoder);
    }
}
