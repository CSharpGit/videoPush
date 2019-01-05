using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace 视频推流
{
    public class Camera
    {
        public string ProcessId { get; set; }//进程id

        public string Status { get; set; }//进程状态

        public string StatusColor { get; set; }//提示颜色
        
        public string CameraName { get; set; }//摄像头名称

        public BitmapImage CameraImg { get; set; }//摄像头图片
    }
}
