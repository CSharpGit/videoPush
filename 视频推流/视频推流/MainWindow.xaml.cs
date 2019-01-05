using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 视频推流
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Process[] processes = Process.GetProcessesByName("CMD");
            if (processes.Length>0)
            {
                foreach (var pro in processes)
                {
                    Helper.KillProcessAndChildren(pro.Id);
                }
            }
        }

        Helper helper = new Helper();

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (helper.InitMysql())
            {
                string sql = "select name,icon,ip,port,http from cxjz_ipcamera";
                DataSet result=helper.Query(sql);
                foreach (DataRow dr in result.Tables[0].Rows)
                {
                    StaticInfo.cameraName.Add(dr["name"].ToString());
                    StaticInfo.cameraImg.Add("http://219.141.127.213:8081/" + dr["icon"].ToString());
                    StaticInfo.ipAdress.Add(dr["ip"].ToString());
                    StaticInfo.port.Add(dr["port"].ToString());
                    StaticInfo.http.Add(dr["http"].ToString());
                }

                if (StaticInfo.cameraName.Count>0)
                {
                    //string head = "ffmpeg -i rtsp://admin:@";
                    //string center = "/h264/ch1/main/av_stream -vcodec copy -acodec copy -f flv -bufsize 20 ";
                    string[] cmdCommand = new string[StaticInfo.cameraName.Count];
                    for (int i = 0; i < StaticInfo.cameraName.Count; i++)
                    {
                        //cmdCommand[i] = head+ StaticInfo.ipAdress[i]+":"+ StaticInfo.port[i]+center+ StaticInfo.http[i];
                        cmdCommand[i] = "ping www.baidu.com -t";
                        Console.WriteLine(cmdCommand[i]);
                    }
                    //helper.ExcuteCmdCommand(cmdCommand);

                    if (helper.CreateBatFile(cmdCommand))
                    {
                        Console.WriteLine("成功创建{0}个bat文件", cmdCommand.Length);
                        CameraManage cameraManage = new CameraManage();
                        cameraManage.Show();
                        this.Close();
                    }
                    else
                    {
                        Console.WriteLine("创建bat文件失败！");
                    }
                    //string[] batPath = new string[cmdCommand.Length];
                    //for (int i = 0; i < batPath.Length; i++)
                    //{
                    //    batPath[i] = Directory.GetCurrentDirectory() + "\\allBat\\" + StaticInfo.cameraName[i] + ".bat";//获取项目文件目录
                    //}
                    //helper.ExcuteBatFile(batPath);
                }
            }
            else
            {
                MessageBox.Show("服务器连接失败，即将关闭软件？!", "提示", MessageBoxButton.YesNo);
                if (DialogResult.Value)
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
}
