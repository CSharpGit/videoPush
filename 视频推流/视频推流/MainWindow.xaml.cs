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
        }

        Helper helper = new Helper();

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (helper.initMysql())
            {
                string sql = "select ip,port,http from cxjz_ipcamera";
                DataSet result=helper.Query(sql);
                foreach (DataRow dr in result.Tables[0].Rows)
                {
                    StaticInfo.ipAdress.Add(dr["ip"].ToString());
                    StaticInfo.port.Add(dr["port"].ToString());
                    StaticInfo.http.Add(dr["http"].ToString());
                }

                if (StaticInfo.ipAdress.Count>0)
                {
                    string head = "ffmpeg -i rtsp://admin:@";
                    string center = "/h264/ch1/main/av_stream -vcodec copy -acodec copy -f flv -bufsize 20 ";
                    string[] cmdCommand = new string[StaticInfo.ipAdress.Count];
                    for (int i = 0; i < StaticInfo.ipAdress.Count; i++)
                    {
                        cmdCommand[i] = head+ StaticInfo.ipAdress[i]+":"+ StaticInfo.port[i]+center+ StaticInfo.http[i];
                        Console.WriteLine(cmdCommand[i]);
                    }
                    
                    helper.ExcuteCmdCommand(cmdCommand);
                }
                //string[] cmdCommand = new string[5];
                //for (int i = 0; i < cmdCommand.Length; i++)
                //{
                //    cmdCommand[i] = "ping www.baidu.com -t";
                //}
                //helper.ExcuteCmdCommand(cmdCommand);
            }
            else
            {
                MessageBox.Show("服务器连接失败，即将关闭软件？!", "提示", MessageBoxButton.YesNo);
                if (DialogResult.Value)
                {
                    this.Close();
                }
            }
            

            //if (helper.CreateBatFile(cmdCommand))
            //{
            //    Console.WriteLine("成功创建{0}个bat文件", cmdCommand.Length);
            //}
            //else
            //{
            //    Console.WriteLine("创建bat文件失败！");
            //}
            //string[] batPath = new string[cmdCommand.Length];
            //for (int i = 0; i < batPath.Length; i++)
            //{
            //    int num = i + 1;
            //    batPath[i] = Directory.GetCurrentDirectory() + "\\allBat\\" + num + ".bat";//获取项目文件目录
            //}
            //helper.ExcuteBatFile(batPath);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var pid in StaticInfo.processId)
            {
                helper.KillProcessAndChildren(Convert.ToInt32(pid));
            }
        }
    }
}
