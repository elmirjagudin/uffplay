using System;
using System.Runtime.InteropServices;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


public class Video
{
    [DllImport("video")]
    static extern int video_open(ref IntPtr decoder, string filename);

    [DllImport("video")]
    static unsafe extern int video_decode_frame(IntPtr decoder, void *pixels);

    IntPtr decoder;

    public Video(string filename)
    {
        var ret = video_open(ref decoder, filename);
        if (ret != 0)
        {
            throw new Exception("video_open() error " + ret);
        }
    }

    public void GetFrame(NativeArray<byte> pixels)
    {
        unsafe
        {
            void *ptr = NativeArrayUnsafeUtility.GetUnsafePtr(pixels);
            var ret = video_decode_frame(decoder, &ptr);
            if (ret != 0)
            {
                throw new Exception("video_decode_frame() error " + ret);
            }
        }
    }
}
