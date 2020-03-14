using System;
using System.Runtime.InteropServices;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


public class Video
{
    [DllImport("video")]
    static extern int video_open(ref IntPtr decoder, string filename);

    [DllImport("video")]
    static unsafe extern int video_decode_frame(IntPtr decoder, void *pixels, out long frame_pts);

    [DllImport("video")]
    static extern void video_stream_info(IntPtr decoder, out int width, out int height, out long num_frames);

    [DllImport("video")]
    static extern void video_close(IntPtr decoder);

    public int Width { get { return _Width; }}
    public int Height { get { return _Height; }}
    public long NumFrames { get { return _NumFrames; }}

    IntPtr decoder;

    int _Width;
    int _Height;
    long _NumFrames;

    public Video(string filename)
    {
        var ret = video_open(ref decoder, filename);
        if (ret != 0)
        {
            throw new Exception("video_open() error " + ret);
        }

        video_stream_info(decoder, out _Width, out _Height, out _NumFrames);
    }

    public void GetFrame(NativeArray<byte> pixels)
    {
        long frame_pts;
        unsafe
        {
            void *ptr = NativeArrayUnsafeUtility.GetUnsafePtr(pixels);
            var ret = video_decode_frame(decoder, &ptr, out frame_pts);
            Log.Msg("frame pts {0}", frame_pts);
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
