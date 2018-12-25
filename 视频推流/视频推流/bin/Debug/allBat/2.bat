ffmpeg -i rtsp://admin:@192.168.1.10:554/h264/ch1/main/av_stream -vcodec copy -acodec copy -f flv -bufsize 20 rtmp://129.28.68.220:1935/cxjz/tst
