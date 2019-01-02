using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 视频推流
{
    public class Camera
    {
        public string ProcessId { get; set; }//进程id

        public int Status { get; set; }//进程状态
        
        public string CameraName { get; set; }//摄像头名称
    }
}
