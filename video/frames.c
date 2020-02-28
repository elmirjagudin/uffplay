#include <libavformat/avformat.h>

#define MOV_FILE "foo.mov"

static void 
pgm_save(unsigned char *buf, int wrap, int xsize, int ysize, char *filename)
{
    FILE *f;
    int i;

    f = fopen(filename,"w");
    fprintf(f, "P5\n%d %d\n%d\n", xsize, ysize, 255);
    for (i = 0; i < ysize; i++)
    {
        fwrite(buf + i * wrap, 1, xsize, f);
    }
    fclose(f);
}


int 
main(int argc, char **argv)
{
    int ret;
    AVFormatContext *fmt_ctx = NULL; //avformat_alloc_context();

    ret = avformat_open_input(&fmt_ctx, MOV_FILE, NULL, NULL);
    if (ret < 0)
    {
        fprintf(stderr, "cannot open %s\n", MOV_FILE);
        return 1;
    }

    /* retrieve stream information */
    if (avformat_find_stream_info(fmt_ctx, NULL) < 0) {
        fprintf(stderr, "Could not find stream information\n");
        return 1;
    }

    ret = av_find_best_stream(fmt_ctx, AVMEDIA_TYPE_VIDEO, -1, -1, NULL, 0);
    if (ret < 0) 
    {
        fprintf(stderr, "Could not find %s stream in input file '%s'\n",
                av_get_media_type_string(AVMEDIA_TYPE_VIDEO), MOV_FILE);
        return 1;
    }
    printf("video stream %d\n", ret);

    int stream_index = ret;
    AVStream *st = fmt_ctx->streams[stream_index];

    /* find decoder for the stream */
    AVCodec *dec = avcodec_find_decoder(st->codecpar->codec_id);
    if (!dec)
    {
        fprintf(stderr, "Failed to find %s codec\n",
                    av_get_media_type_string(AVMEDIA_TYPE_VIDEO));
        return 1;
    }

    /* allocate a codec context for the decoder */
    AVCodecContext *dec_ctx = avcodec_alloc_context3(dec);
    if (dec_ctx == NULL)
    {
        fprintf(stderr, "Failed to allocate the %s codec context\n",
                av_get_media_type_string(AVMEDIA_TYPE_VIDEO));

        return 1;
    }

    /* Copy codec parameters from input stream to output codec context */
    if ((ret = avcodec_parameters_to_context(dec_ctx, st->codecpar)) < 0)
    {
        fprintf(stderr, "Failed to copy %s codec parameters to decoder context\n",
                av_get_media_type_string(AVMEDIA_TYPE_VIDEO));
        return 1;
    }

    AVDictionary *opts = NULL;

    if ((ret = avcodec_open2(dec_ctx, dec, &opts)) < 0)
    {
        fprintf(stderr, "Failed to open %s codec\n",
                av_get_media_type_string(AVMEDIA_TYPE_VIDEO));
        return 1;
    }


    AVFrame *frame = av_frame_alloc();
    if (!frame) 
    {
        fprintf(stderr, "Could not allocate frame\n");
        return 1;
    }

    int i;
    int got_frame;

    for (i = 0; i < 32*10 ; i++)
    {
        AVPacket pkt = { 0 };

        ret = av_read_frame(fmt_ctx, &pkt);
        printf("i %d ret %d stream_index %d\n", i, ret, pkt.stream_index);
        if (pkt.stream_index != stream_index)
        {
            printf("not a video pkt, skipping");
            av_packet_unref(&pkt);
            continue;    
        }

        ret = avcodec_send_packet(dec_ctx, &pkt);
        if (ret != 0)
        {
            printf("error send packt");
            return 1;
        }

        ret = avcodec_receive_frame(dec_ctx, frame);
        if (ret == 0)
        {
            printf("got frame\n");
            /* the picture is allocated by the decoder. no need to
               free it */
            char buf[1024];
            snprintf(buf, sizeof(buf), "%s-%d", "filename", dec_ctx->frame_number);
            pgm_save(frame->data[0], frame->linesize[0],
                     frame->width, frame->height, buf);
        }
        else if (ret == AVERROR(EAGAIN))
        {
            printf("NEED more packets\n");
        }

        av_packet_unref(&pkt);
    }

    avformat_close_input(&fmt_ctx);

    return 0;
}