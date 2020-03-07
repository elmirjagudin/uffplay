#!/bin/sh

FFMPEG_INST_DIR="$(pwd)/../build/ffmpeg"
PLUGINS_DIR="$(pwd)/../player/Assets/Plugins"

make decoder FFMPEG_INST_DIR="$FFMPEG_INST_DIR"
make libvideo.so FFMPEG_INST_DIR="$FFMPEG_INST_DIR"

mkdir -p "$PLUGINS_DIR"
cp libvideo.so "$PLUGINS_DIR"
