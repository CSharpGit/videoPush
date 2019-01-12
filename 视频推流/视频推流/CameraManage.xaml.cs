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
using System.Runtime.InteropServices;

namespace 视频推流
{
    /// <summary>
    /// CameraManage.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class CameraManage : Window
    {
        System.Timers.Timer CheckProcessTimer = new System.Timers.Timer(1000);//定时检查进程

        System.Timers.Timer CheckCameraEnableStatusTimer = new System.Timers.Timer(5000);//定时检查摄像头启用状态

        List<Camera> CameraList = new List<Camera>();//摄像头列表

        List<IntPtr> IntPtrs = new List<IntPtr>();//窗口句柄列表

        List<int> IsEnable = new List<int>();//摄像头启用状态

        public CameraManage()
        {
            InitializeComponent();
            foreach (var pId in GetCurrentProcessIdes())
            {
                Helper.KillProcessAndChildren(pId);
            }
            InitialCameraList();//初始化摄像头
            for (int i = 0; i < CameraList.Count; i++)
            {
                IntPtrs.Add(IntPtr.Zero);
            }
            CheckCameraEnableStatusTimer.Elapsed += new ElapsedEventHandler(CheckCamera_EnableStatus);//给定时器添加触发事件
            CheckCameraEnableStatusTimer.AutoReset = true; //每到指定时间Elapsed事件是触发一次（false），还是一直触发（true
            CheckCameraEnableStatusTimer.Enabled = true; //是否触发Elapsed事件
            CheckCameraEnableStatusTimer.Start();//检查摄像头状态定时器启动

            CheckProcessTimer.Elapsed += new ElapsedEventHandler(CheckProcess);//给定时器添加触发事件
            CheckProcessTimer.AutoReset = true; //每到指定时间Elapsed事件是触发一次（false），还是一直触发（true）
        }

        //初始化摄像头
        private void InitialCameraList()
        {
            if (Helper.InitMysql())
            {
                string sql = "select name,ip,port,http,enable from cxjz_ipcamera";
                DataSet result = Helper.Query(sql);
                foreach (DataRow dr in result.Tables[0].Rows)
                {
                    StaticInfo.cameraName.Add(dr["name"].ToString());
                    StaticInfo.ipAdress.Add(dr["ip"].ToString());
                    StaticInfo.port.Add(dr["port"].ToString());
                    StaticInfo.http.Add(dr["http"].ToString());
                    IsEnable.Add(Convert.ToInt32(dr["enable"]));
                }
                //StaticInfo.cameraName.Add("11");
                //StaticInfo.ipAdress.Add("192.168.1.11");
                //StaticInfo.port.Add("554");
                //StaticInfo.http.Add("rtmp://219.141.127.213:1935/hls/asf");

                //StaticInfo.cameraName.Add("12");
                //StaticInfo.ipAdress.Add("192.168.1.12");
                //StaticInfo.port.Add("554");
                //StaticInfo.http.Add("rtmp://219.141.127.213:1935/hls/cxjz");

                //StaticInfo.cameraName.Add("13");
                //StaticInfo.ipAdress.Add("192.168.1.13");
                //StaticInfo.port.Add("554");
                //StaticInfo.http.Add("rtmp://219.141.127.213:1935/hls/fghh");

                //StaticInfo.cameraName.Add("14");
                //StaticInfo.ipAdress.Add("192.168.1.14");
                //StaticInfo.port.Add("554");
                //StaticInfo.http.Add("rtmp://219.141.127.213:1935/hls/jhk");

                //StaticInfo.cameraName.Add("15");
                //StaticInfo.ipAdress.Add("192.168.1.15");
                //StaticInfo.port.Add("554");
                //StaticInfo.http.Add("rtmp://219.141.127.213:1935/hls/dfc");


                if (StaticInfo.cameraName.Count > 0)
                {
                    string head = "ffmpeg -i rtsp://admin:@";
                    string center = "/h264/ch1/main/av_stream -vcodec copy -acodec copy -f flv -bufsize 20 ";
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
                //StaticInfo.cameraName.Count
                CameraList.Add(new Camera
                {
                    ProcessId = "00" + i,
                    Status = "未启动",
                    StatusColor = "#F7D358",
                    CameraName = StaticInfo.cameraName[i].ToString(),
                    //CameraName="摄像头名称图一日体育体育挂号费"
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
            HideAllCmdWindonws();//启动完所有推流线程，先隐藏所有窗口
            CheckProcessTimer.Enabled = true; //是否触发Elapsed事件
            CheckProcessTimer.Start();//检查进程Id定时器启动

            this.startAll.Foreground=Brushes.Red;
            this.endAll.Foreground = Brushes.Black;
        }

        private void EndAll_Click(object sender, RoutedEventArgs e)//全部关闭按钮单击事件
        {
            int[] prosessId = GetCurrentProcessIdes();
            if (prosessId.Length>0)
            {
                for (int i = 0; i < CameraList.Count; i++)
                {
                    if (CameraList[i].Status == "推流中")
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
            this.endAll.Foreground = Brushes.Red;
            this.startAll.Foreground = Brushes.Black;
        }

        private void AutoReStartOpen_Click(object sender, RoutedEventArgs e)
        {
            CheckProcessTimer.Stop();
            CheckProcessTimer.Enabled = false;
            MessageBox.Show("自动重启功能开启完成！");
            CheckProcessTimer.Elapsed += new ElapsedEventHandler(CheckProcess_AutoRestart);//给定时器添加触发事件
            CheckProcessTimer.Start();
            CheckProcessTimer.Enabled = true;
            this.autoReStartOpen.Foreground = Brushes.Red;
            this.autoReStartClose.Foreground = Brushes.Black;
        }

        private void AutoReStartClose_Click(object sender, RoutedEventArgs e)
        {
            CheckProcessTimer.Stop();
            CheckProcessTimer.Enabled = false;
            CheckProcessTimer.Elapsed -= new ElapsedEventHandler(CheckProcess_AutoRestart);
            MessageBox.Show("自动重启功能关闭完成！");
            CheckProcessTimer.Start();
            CheckProcessTimer.Enabled = true;
            this.autoReStartClose.Foreground = Brushes.Red;
            this.autoReStartOpen.Foreground = Brushes.Black;
        }

        private void ShowAllCmdWindows_Click(object sender, RoutedEventArgs e)
        {
            ShowAllCmdWindonws();
            this.showAllCmdWindows.Foreground = Brushes.Red;
            this.hideAllCmdWindows.Foreground = Brushes.Black;
        }

        private void HideAllCmdWindows_Click(object sender, RoutedEventArgs e)
        {
            HideAllCmdWindonws();
            this.hideAllCmdWindows.Foreground = Brushes.Red;
            this.showAllCmdWindows.Foreground = Brushes.Black;
        }

        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            Button bt = e.OriginalSource as Button;
            int[] prosessId = GetCurrentProcessIdes();
            string operation = bt.Name;
            string pId = bt.CommandParameter.ToString();//bt传来的摄像头编号或进程Id
            if (operation=="start")
            {
                if (pId.Substring(0, 2) == "00")//取到的是编号
                {
                    int index = Convert.ToInt32(pId);//转换pId为编号
                    Task oneBatTask = Task.Factory.StartNew(delegate
                    {
                        ExcuteOneBatFile(index);
                    });
                    Thread.Sleep(1000);
                }
                else//取到的是进程ID
                {
                    //MessageBox.Show("该摄像头正在推流中！");
                    int index = CameraList.FindIndex(c => c.ProcessId == pId);//获取pId在CameraList中的位置
                    if (IntPtrs[index]==IntPtr.Zero)//判断intPtrs中对应位置的句柄是否为Zero
                    {
                        IntPtr intPtr = Process.GetProcessById(Convert.ToInt32(pId)).MainWindowHandle;//获取对应位置进程id的窗口句柄
                        IntPtrs[index] = intPtr;//将窗口句柄保存
                        ShowWindow(intPtr,0);//隐藏窗口
                    }
                    else
                    {
                        ShowWindow(IntPtrs[index],1);//显示对应窗体句柄的窗口
                        IntPtrs[index] = IntPtr.Zero;//将intPtrs对应位置的intPtr重置
                    }
                }
                CheckProcessTimer.Enabled = true; //是否触发Elapsed事件
                CheckProcessTimer.Start();
            }
            else if (operation == "end")
            {
                if (pId.Substring(0, 2) == "00")//取到的是编号
                {
                    MessageBox.Show("该摄像头未启动！");
                }
                else//取到的是进程ID
                {
                    Helper.KillProcessAndChildren(Convert.ToInt32(pId));//结束bt传来的进程
                }
            }
        }

        public void CheckProcess(object sender, ElapsedEventArgs e)
        {
            int[] prosessId = GetCurrentProcessIdes();
            for (int i = 0; i < CameraList.Count; i++)
            {
                Camera cm = CameraList[i];
                if (Array.IndexOf(prosessId, Convert.ToInt32(CameraList[i].ProcessId))==-1)//记录的进程Id不存在当前进程中
                {
                    if (cm.Status == "推流中")
                    {
                        cm.Status = "已关闭";
                        cm.StatusColor = "#FFFF0006";
                        cm.ProcessId = "00"+i;
                    }
                }
                else
                {
                    cm.Status = "推流中";
                    cm.StatusColor = "#31B404";
                }
            }
        }

        public void CheckProcess_AutoRestart(object sender, ElapsedEventArgs e)
        {
            string[] cmStatus = CameraList.Select(c=>c.Status).ToArray();
            for (int i = 0; i < cmStatus.Length; i++)
            {
                if (cmStatus[i]== "已关闭")
                {
                    if (IsEnable[i]==1)
                    {
                        if (Array.IndexOf(GetCurrentProcessIdes(), Convert.ToInt32(CameraList[i].ProcessId)) == -1)//记录的进程Id不存在当前进程中
                        {
                            ExcuteOneBatFile(i);
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
        }

        public void CheckCamera_EnableStatus(object sender,ElapsedEventArgs e)
        {
            string sql = "select enable from cxjz_ipcamera";
            try
            {
                if (Helper.InitMysql())
                {
                    IsEnable.Clear();
                    DataSet result = Helper.Query(sql);
                    foreach (DataRow dr in result.Tables[0].Rows)
                    {
                        IsEnable.Add(Convert.ToInt32(dr["enable"]));
                    }
                    for (int i = 0; i < IsEnable.Count; i++)
                    {
                        if (Convert.ToInt32(IsEnable[i]) == 0)
                        {
                            string pId = CameraList[i].ProcessId;
                            if (pId.Substring(0, 2) != "00")
                            {
                                Helper.KillProcessAndChildren(Convert.ToInt32(pId));
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("数据库连接失败，软件即将关闭");
                    Application.Current.Shutdown();
                }
            }
            catch (Exception)
            {

                throw;
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
                //FileInfo file = new FileInfo(StaticInfo.batFileName[index].ToString());
                //process.StartInfo.WorkingDirectory = file.Directory.FullName;
                process.StartInfo.FileName = StaticInfo.batFileName[index].ToString();
                //process.StartInfo.RedirectStandardOutput = false;
                //process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = false;
                process.Start();
                CameraList[index].ProcessId = process.Id.ToString();//记录启用的进程id
                //intPtrs[index] = process.MainWindowHandle;
                //Console.WriteLine("___________________________"+process.MainWindowHandle);
                process.WaitForExit();
                process.Close();
            }
        }

        public IntPtr GetIntPtr(int pId)
        {
            IntPtr intPtr = Process.GetProcessById(pId).MainWindowHandle;
            return intPtr;
        } 

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var pid in GetCurrentProcessIdes())
            {
                Helper.KillProcessAndChildren(pid);
            }
            Application.Current.Shutdown();
        }

        public void HideAllCmdWindonws()
        {
            string[] currentPid = CameraList.Select(c => c.ProcessId).ToArray();
            for (int i = 0; i < currentPid.Length; i++)
            {
                if (currentPid[i].Substring(0,2) != "00")
                {
                    if (GetIntPtr(Convert.ToInt32(currentPid[i])) !=IntPtr.Zero)
                    {
                        IntPtrs[i] = GetIntPtr(Convert.ToInt32(currentPid[i]));
                        ShowWindow(GetIntPtr(Convert.ToInt32(currentPid[i])), 0);
                    }
                }
            }
        }

        public void ShowAllCmdWindonws()
        {
            for (int i = 0; i < IntPtrs.Count; i++)
            {
                if (IntPtrs[i] != IntPtr.Zero)
                {
                    ShowWindow(IntPtrs[i],1);
                    IntPtrs[i] = IntPtr.Zero;
                }
            }
        }

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
    }
}
