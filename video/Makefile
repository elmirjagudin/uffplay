CFLAGS := $(shell PKG_CONFIG_PATH=$(FFMPEG_INST_DIR)/lib/pkgconfig pkg-config --cflags libavcodec)
LDFLAGS := $(shell PKG_CONFIG_PATH=$(FFMPEG_INST_DIR)/lib/pkgconfig pkg-config --libs libavcodec libavformat libavdevice libswresample libswscale)

libvideo.so: decoder.c
	gcc -shared -fPIC $(CFLAGS) decoder.c -Wl,-Bsymbolic $(LDFLAGS) -o libvideo.so

decoder: decoder.c
	gcc -g $(CFLAGS) -DDECODER_WITH_MAIN_FUNC decoder.c $(LDFLAGS) -o decoder

clean:
	rm -rf libvideo.so decoder
