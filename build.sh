#!/bin/sh


PLUGINS_DIR="$(pwd)/player/Assets/Plugins"
BUILD_DIR="$(pwd)/build"
FFMPEG_INST_DIR="$BUILD_DIR/ffmpeg"

# build ffmpeg
mkdir -p "$FFMPEG_INST_DIR"
cd ffmpeg
./configure --prefix=$FFMPEG_INST_DIR --disable-optimizations --disable-stripping --enable-pic --enable-gpl --enable-libx264
make install
cd -

# build video wrapper lib
cd video
make libvideo.so FFMPEG_INST_DIR="$FFMPEG_INST_DIR"
mkdir -p "$PLUGINS_DIR"
cp libvideo.so "$PLUGINS_DIR"


