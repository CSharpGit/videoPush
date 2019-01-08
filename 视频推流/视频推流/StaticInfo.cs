using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 视频推流
{
    public static class StaticInfo
    {
        public static ArrayList ipAdress = new ArrayList();//推流主机ip

        public static ArrayList port = new ArrayList();//推流主机端口

        public static ArrayList http = new ArrayList();//服务器rtmp流

        public static ArrayList cameraName = new ArrayList();//摄像头名称

        public static ArrayList processId = new ArrayList();//进程ID
        
        public static ArrayList batFileName = new ArrayList();//bat文件名
    }
}
