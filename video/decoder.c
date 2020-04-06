#include <stdbool.h>
#include <libavformat/avformat.h>
#include <libswscale/swscale.h>


#define TEXTURE_PIX_FMT AV_PIX_FMT_RGB24

#define MOV_FILE "foo.mov"
//#define MOV_FILE "DJI_0001.MOV"

typedef struct Video
{
    AVFormatContext *fmt_ctx;
    int stream_index;
    AVStream *st;
    AVCodec *dec;
    AVCodecContext *dec_ctx;
    struct SwsContext *sws_ctx;
    AVFrame *frame;
    int linesize[1];
}
Video;


void
video_close(Video *video)
{
    if (video->frame != NULL)
    {
        av_frame_free(&(video->frame));
    }
    /* becomes NOP when sws_ctx pointer is NULL */
    sws_freeContext(video->sws_ctx);
    if (video->dec_ctx != NULL)
    {
        avcodec_free_context(&(video->dec_ctx));
    }
    if (video->fmt_ctx != NULL)
    {
        avformat_close_input(&(video->fmt_ctx));
    }

    free(video);
}

/**
 * fetch stream meta data
 *
 * width      - frame's width in pixels
 * height     - frame's height in pixels
 * start_time - video stream's start time, in stream's time base units
 * duration   - video stream's duration, in stream's time base units
 */
void
video_stream_info(Video *video, int *width, int *height,
                  int64_t *start_time, int64_t *duration)
{
    *width = video->dec_ctx->width;
    *height = video->dec_ctx->height;
    *start_time = video->st->start_time;
    *duration = video->st->duration;
}

int
video_seek_frame(Video *video, int64_t frame_pts)
{
    int res;

    res = av_seek_frame(video->fmt_ctx, video->stream_index, frame_pts, AVSEEK_FLAG_ANY);
    if (res < 0)
    {
        fprintf(stderr, "Error in seeking\n");
        return res;
    };
    avcodec_flush_buffers(video->dec_ctx);

    return 0;
}

int
video_decode_frame(Video *video, uint8_t **pixels, int64_t *pts)
{
    int ret;
    while (true)
    {
        AVPacket pkt = { 0 };

        ret = av_read_frame(video->fmt_ctx, &pkt);
        if (ret == AVERROR_EOF)
        {
            /* end of the file, send EOF(?) packet to decoder */
            avcodec_send_packet(video->dec_ctx, NULL);
        }
        else if (ret != 0)
        {
            printf("av_read_frame error: %d %s\n", ret, av_err2str(ret));
            return 1;
        }
        else
        {
            /* got a new package */

            if (pkt.stream_index != video->stream_index)
            {
                /* skip non-video stream packets */
                printf("not a video pkt, skipping\n");
                av_packet_unref(&pkt);
                continue;
            }

            ret = avcodec_send_packet(video->dec_ctx, &pkt);
            if (ret != 0)
            {
                printf("error send packt\n");
                return 1;
            }
        }

        ret = avcodec_receive_frame(video->dec_ctx, video->frame);
        if (ret == 0)
        {
            *pts = video->frame->pts;

            char *keyframe = video->frame->key_frame ? "yes" : "no";
            printf("frame pts %ld keyframe %s\n", video->frame->pts, keyframe);

            sws_scale(video->sws_ctx,
                      (const uint8_t *const*)video->frame->data,
                      video->frame->linesize,
                      0,
                      video->frame->height,
                      (uint8_t *const*)pixels,
                      video->linesize);
            av_packet_unref(&pkt);
            return 0;
        }
        else if (ret == AVERROR(EAGAIN))
        {
            printf("NEED more packets\n");
        }
        else if (ret == AVERROR_EOF)
        {
            printf("END OF THE MOVIE?\n");
            return -1;
        }

        av_packet_unref(&pkt);
    }
}

int
video_open(Video **video, char *filename)
{
    Video* vid = *video = calloc(1, sizeof(*vid));

    int ret;

    ret = avformat_open_input(&(vid->fmt_ctx), filename, NULL, NULL);
    if (ret < 0)
    {
        fprintf(stderr, "cannot open %s\n", filename);
        goto err_out;
    }

    /* retrieve stream information */
    if (avformat_find_stream_info(vid->fmt_ctx, NULL) < 0)
    {
        fprintf(stderr, "Could not find stream information\n");
        goto err_out;
    }

    vid->stream_index = av_find_best_stream(vid->fmt_ctx, AVMEDIA_TYPE_VIDEO, -1, -1, NULL, 0);
    if (vid->stream_index < 0)
    {
        fprintf(stderr, "Could not find %s stream in input file '%s'\n",
                av_get_media_type_string(AVMEDIA_TYPE_VIDEO), MOV_FILE);
        goto err_out;
    }
    printf("video stream %d\n", vid->stream_index);

    vid->st = vid->fmt_ctx->streams[vid->stream_index];

    /* find decoder for the stream */
    vid->dec = avcodec_find_decoder(vid->st->codecpar->codec_id);
    if (!(vid->dec))
    {
        fprintf(stderr, "Failed to find %s codec\n",
                av_get_media_type_string(AVMEDIA_TYPE_VIDEO));
        goto err_out;
    }

    /* allocate a codec context for the decoder */
    vid->dec_ctx = avcodec_alloc_context3(vid->dec);
    if (vid->dec_ctx == NULL)
    {
        fprintf(stderr, "Failed to allocate the %s codec context\n",
                av_get_media_type_string(AVMEDIA_TYPE_VIDEO));

        goto err_out;
    }

    /* Copy codec parameters from input stream to output codec context */
    ret = avcodec_parameters_to_context(vid->dec_ctx, vid->st->codecpar);
    if (ret < 0)
    {
        fprintf(stderr, "Failed to copy %s codec parameters to decoder context\n",
                av_get_media_type_string(AVMEDIA_TYPE_VIDEO));
        goto err_out;
    }

    AVDictionary *opts = NULL;
    ret = avcodec_open2(vid->dec_ctx, vid->dec, &opts);
    if (ret < 0)
    {
        fprintf(stderr, "Failed to open %s codec\n",
                av_get_media_type_string(AVMEDIA_TYPE_VIDEO));
        return 1;
    }

    /* set-up context for converting to video frames to textures */
    vid->sws_ctx = sws_getContext(vid->dec_ctx->width,
                                  vid->dec_ctx->height,
                                  vid->dec_ctx->pix_fmt,
                                  vid->dec_ctx->width,       // do 1:1 conversion
                                  vid->dec_ctx->height,
                                  TEXTURE_PIX_FMT,
                                  0, NULL, NULL, NULL);
    if (vid->sws_ctx == NULL)
    {
        fprintf(stderr, "error creating SwsContext\n");
        goto err_out;
    }

    vid->frame = av_frame_alloc();
    if (vid->frame == NULL)
    {
        fprintf(stderr, "Could not allocate frame\n");
        goto err_out;
    }

    vid->linesize[0] = vid->dec_ctx->width * 3;

    return 0;

err_out:
    video_close(vid);
    return 1;
}

static void
rgb24_save(char *fname, uint8_t *pixels, int len)
{
    FILE *f;

    f = fopen(fname, "w");
    fwrite(pixels, len, 1, f);
    fclose(f);
}

int
main(int argc, char **argv)
{
    Video *v;
    int r;

    r = video_open(&v, MOV_FILE);
    if (r != 0)
    {
        fprintf(stderr, "error opening video\n");
        return 1;
    }

    int w, h;
    int64_t start_time, duration;

    video_stream_info(v, &w, &h, &start_time, &duration);
    printf("%s %d %d pixels\n", MOV_FILE, w, h);
    printf("start: %ld duration: %ld\n", start_time, duration);

    uint8_t *pixels[1];
    pixels[0] = malloc(v->dec_ctx->width * v->dec_ctx->height * 3);

    char fname[64];

    r = 0;
    for (int i = 0; r == 0; i += 1)
    {
        snprintf(fname, 64, "frame-%d", i);
        printf("fname '%s'\n", fname);

        int64_t pts;
        r = video_decode_frame(v, pixels, &pts);

        //rgb24_save(fname, pixels[0], v->dec_ctx->width * v->dec_ctx->height * 3);
    }

    video_close(v);
    printf("closed\n");
}
