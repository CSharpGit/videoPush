using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace 视频推流
{
    public class Camera: INotifyPropertyChanged
    {
        public string _ProcessId;//进程id
        private string _Status;//进程状态
        private string _StatusColor;//状态提示颜色

        public string ProcessId
        {
            get { return _ProcessId; }
            set
            {
                if (_ProcessId != value)
                {
                    _ProcessId = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("ProcessId"));
                }
            }
        }

        public string Status
        {
            get { return _Status; }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }

        public string StatusColor
        {
            get { return _StatusColor; }
            set
            {
                if (_StatusColor != value)
                {
                    _StatusColor = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusColor"));
                }
            }
        }

        public string CameraName { get; set; }//摄像头名称

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
