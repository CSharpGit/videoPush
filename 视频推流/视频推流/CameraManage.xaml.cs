using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Data;
using System.Linq;
using System.IO;
using System.Timers;
using System.Diagnostics;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Threading;

namespace 视频推流
{
    /// <summary>
    /// CameraManage.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class CameraManage : Window
    {
        System.Timers.Timer timer = new System.Timers.Timer(1000);//设置时间间隔

        List<Camera> CameraList = new List<Camera>();

        public CameraManage()
        {
            InitializeComponent();
            foreach (var pId in GetCurrentProcessIdes())
            {
                Helper.KillProcessAndChildren(pId);
            }
            InitialCameraList();//初始化摄像头
            timer.Elapsed += new ElapsedEventHandler(CheckProcess);//给定时器添加触发事件
            timer.AutoReset = true; //每到指定时间Elapsed事件是触发一次（false），还是一直触发（true）
        }

        //初始化摄像头
        private void InitialCameraList()
        {
            if (Helper.InitMysql())
            {
                string sql = "select name,ip,port,http from cxjz_ipcamera";
                DataSet result = Helper.Query(sql);
                foreach (DataRow dr in result.Tables[0].Rows)
                {
                    StaticInfo.cameraName.Add(dr["name"].ToString());
                    StaticInfo.ipAdress.Add(dr["ip"].ToString());
                    StaticInfo.port.Add(dr["port"].ToString());
                    StaticInfo.http.Add(dr["http"].ToString());
                }

                if (StaticInfo.cameraName.Count > 0)
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

                    if (Helper.CreateBatFile(cmdCommand))
                    {
                        Console.WriteLine("成功创建{0}个bat文件", cmdCommand.Length);
                    }
                    else
                    {
                        Console.WriteLine("创建bat文件失败！");
                    }
                }
            }
            else
            {
                MessageBox.Show("服务器连接失败，即将关闭软件!");
                Application.Current.Shutdown();
            }

            for (int i = 0; i < StaticInfo.cameraName.Count; i++)
            {
                CameraList.Add(new Camera
                {
                    ProcessId = "00" + i,
                    Status = "未启动",
                    StatusColor = "#F7D358",
                    CameraName = StaticInfo.cameraName[i].ToString(),
                });
            }
            this.ListBoxCameras.ItemsSource = CameraList;
        }

        private void StartAll_Click(object sender, RoutedEventArgs e)//全部启动按钮单击事件
        {
            for (int i = 0; i < CameraList.Count; i++)
            {
                if (CameraList[i].Status=="未启动"|| CameraList[i].Status=="已关闭")
                {
                    Task oneBatTask = Task.Factory.StartNew(delegate {
                        ExcuteOneBatFile(i);
                    });
                    Thread.Sleep(1000);
                }
            }
            timer.Enabled = true; //是否触发Elapsed事件
            timer.Start();
        }

        private void EndAll_Click(object sender, RoutedEventArgs e)//全部关闭按钮单击事件
        {
            int[] prosessId = GetCurrentProcessIdes();
            if (prosessId.Length > 0)
            {
                for (int i = 0; i < CameraList.Count; i++)
                {
                    if (Array.IndexOf(prosessId, Convert.ToInt32(CameraList[i].ProcessId)) == -1)//记录的进程Id不存在当前进程中
                    {
                        //MessageBox.Show("记录的进程Id不存在当前进程中!");
                    }
                    else//记录的进程Id存在当前进程中
                    {
                        Helper.KillProcessAndChildren(Convert.ToInt32(CameraList[i].ProcessId));
                        CameraList[i].ProcessId = "00" + i;
                    }
                }
            }
            else
            {
                MessageBox.Show("当前没有任何摄像头在推流！");
            }
            //timer.Stop();
        }

        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            Button bt = e.OriginalSource as Button;
            int[] prosessId = GetCurrentProcessIdes();
            string operation = bt.Name;
            string pId = bt.CommandParameter.ToString();//bt传来的摄像头编号或进程Id
            if (operation=="start")
            {
                if (prosessId.Length > 0)
                {
                    if (Array.IndexOf(prosessId, Convert.ToInt32(bt.CommandParameter)) == -1)//bt传来的进程Id不存在当前进程中
                    {
                        if (pId.Substring(0, 2) == "00")
                        {
                            int index = Convert.ToInt32(pId);
                            Task oneBatTask = Task.Factory.StartNew(delegate {
                                ExcuteOneBatFile(index);
                            });
                            Thread.Sleep(1000);
                        }
                    }
                    else//bt传来的进程Id存在当前进程中
                    {
                        MessageBox.Show("该摄像头正在推流中！");
                    }
                }
                else
                {
                    if (pId.Substring(0, 2) == "00")
                    {
                        int index =Convert.ToInt32(pId);
                        Task oneBatTask= Task.Factory.StartNew(delegate {
                            ExcuteOneBatFile(index);
                        });
                        Thread.Sleep(1000);
                    }
                    timer.Enabled = true; //是否触发Elapsed事件
                    timer.Start();
                }
            }
            else if (operation == "end")
            {
                if (Array.IndexOf(prosessId, Convert.ToInt32(bt.CommandParameter)) == -1)//记录的进程Id不存在当前进程中
                {
                    MessageBox.Show("该摄像头未启动！");
                }
                else//记录的进程Id不存在当前进程中
                {
                    Helper.KillProcessAndChildren(Convert.ToInt32(bt.CommandParameter));//结束bt传来的进程
                    int index = CameraList.FindIndex(item => item.ProcessId == (bt.CommandParameter.ToString()));
                    CameraList[index].ProcessId = "00" + index;
                }
            }
        }

        public void CheckProcess(object sender, ElapsedEventArgs e)
        {
            int[] prosessId = GetCurrentProcessIdes();
            for (int i = 0; i < CameraList.Count; i++)
            {
                if (Array.IndexOf(prosessId, Convert.ToInt32(CameraList[i].ProcessId))==-1)//记录的进程Id不存在当前进程中
                {
                    Camera cm = CameraList[i];
                    if (cm.Status == "推流中")
                    {
                        cm.Status = "已关闭";
                        cm.StatusColor = "#FFFF0006";
                    }
                }
                else
                {
                    Camera cm = CameraList[i];
                    cm.Status = "推流中";
                    cm.StatusColor = "#31B404";
                }
            }
        }

        public int[] GetCurrentProcessIdes()//获取当前正在运行的CMD进程，将Id存在数组中
        {
            Process[] process = Process.GetProcessesByName("cmd");
            int[] processId = new int[process.Length];
            for (int i = 0; i < processId.Length; i++)
            {
                processId[i] = process[i].Id;
            }
            return processId;
        }

        public void ExcuteOneBatFile(int index)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = StaticInfo.batFileName[index].ToString();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                CameraList[index].ProcessId = process.Id.ToString();//记录启用的进程id
                process.WaitForExit();
                process.Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var pid in GetCurrentProcessIdes())
            {
                Helper.KillProcessAndChildren(pid);
            }
            Application.Current.Shutdown();
        }
    }
}
