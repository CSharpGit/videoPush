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

namespace 视频推流
{
    /// <summary>
    /// CameraManage.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class CameraManage : Window
    {
        Helper helper = new Helper();

        Timer timer = new Timer(1000);//设置时间间隔

        List<Camera> CameraList = new List<Camera>();

        public CameraManage()
        {
            InitializeComponent();
            InitialCameraList();
            timer.Elapsed += new ElapsedEventHandler(CheckProcess);
            timer.AutoReset = true; //每到指定时间Elapsed事件是触发一次（false），还是一直触发（true）
        }

        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            Button bt = e.OriginalSource as Button;
            int[] prosessId = GetCurrentProcessIdes();
            string operation = bt.Name;
            if (operation=="start")
            {
                if (prosessId.Length>0)
                {
                    if (Array.IndexOf(prosessId, Convert.ToInt32(bt.CommandParameter)) == -1)//bt传来的进程Id不存在当前进程中
                    {
                        MessageBox.Show(bt.CommandParameter.ToString());
                    }
                    else//bt传来的进程Id存在当前进程中
                    {
                        MessageBox.Show("该摄像头正在推流中！");
                    }
                }
                else
                {
                    for (int i = 0; i < CameraList.Count; i++)
                    {
                        if (bt.CommandParameter.ToString()== CameraList[i].ProcessId.ToString())
                        {
                            using (Process process = new Process())
                            {
                                FileInfo file = new FileInfo(StaticInfo.batFileName[i].ToString());
                                if (file.Directory != null)
                                {
                                    process.StartInfo.WorkingDirectory = file.Directory.FullName;
                                }
                                process.StartInfo.FileName = StaticInfo.batFileName[i].ToString();
                                process.StartInfo.RedirectStandardOutput = true;
                                process.StartInfo.RedirectStandardError = true;
                                process.StartInfo.UseShellExecute = false;
                                process.StartInfo.CreateNoWindow = true;
                                process.Start();
                                CameraList[i].ProcessId = process.Id.ToString();//记录启用的进程id
                                process.WaitForExit();
                                process.Close();
                            }
                        }
                    }
                    timer.Enabled = true; //是否触发Elapsed事件
                    timer.Start();
                }
                //MessageBox.Show(bt.CommandParameter.ToString());
            }
            else if (operation == "end")
            {
                if (Array.IndexOf(prosessId, Convert.ToInt32(bt.CommandParameter)) == -1)//记录的进程Id不存在当前进程中
                {
                    MessageBox.Show(bt.CommandParameter.ToString());
                }
                else//记录的进程Id不存在当前进程中
                {
                    Helper.KillProcessAndChildren(Convert.ToInt32(bt.CommandParameter));//结束bt传来的进程
                    foreach (var cl in CameraList)
                    {
                        if (cl.ProcessId== bt.CommandParameter.ToString())
                        {
                            cl.ProcessId = "002";
                        }
                    }
                }
            }
        }

        //初始化摄像头
        private void InitialCameraList()
        {
            for (int i = 0; i < StaticInfo.cameraName.Count; i++)
            {
                CameraList.Add(new Camera {
                    ProcessId="001",
                    Status= "未启动",
                    StatusColor = "#F7D358",
                    CameraName =StaticInfo.cameraName[i].ToString(),
                });
            }
            this.ListBoxCameras.ItemsSource = CameraList;
        }

        private void StartAll_Click(object sender, RoutedEventArgs e)
        {
            string[] batPath = new string[StaticInfo.batFileName.Count];
            for (int i = 0; i < batPath.Length; i++)
            {
                batPath[i] = StaticInfo.batFileName[i].ToString();//获取项目文件目录
            }
            helper.ExcuteBatFile(batPath);
            for (int i = 0; i < CameraList.Count; i++)
            {
                Camera cm = CameraList[i];
                cm.ProcessId = StaticInfo.processId[i].ToString();
            }
            timer.Enabled = true; //是否触发Elapsed事件
            timer.Start();
        }

        private void EndAll_Click(object sender, RoutedEventArgs e)
        {
            //timer.Stop();
            int[] prosessId = GetCurrentProcessIdes();
            if (prosessId.Length > 0)
            {
                foreach (var cl in CameraList)
                {
                    if (Array.IndexOf(prosessId, Convert.ToInt32(cl.ProcessId)) == -1)//记录的进程Id不存在当前进程中
                    {
                        
                    }
                    else//记录的进程Id存在当前进程中
                    {
                        Helper.KillProcessAndChildren(Convert.ToInt32(cl.ProcessId));
                        cl.ProcessId = "002";
                    }
                }
            }
            else
            {
                MessageBox.Show("当前没有任何摄像头在推流！");
            }
            StaticInfo.processId.Clear();
        }

        public void CheckProcess(object sender, ElapsedEventArgs e)
        {
            int[] prosessId = GetCurrentProcessIdes();
            for (int i = 0; i < CameraList.Count; i++)
            {
                if (Array.IndexOf(prosessId, Convert.ToInt32(CameraList[i].ProcessId))==-1)//记录的进程Id不存在当前进程中
                {
                    Camera cm = CameraList[i];
                    cm.Status = "已关闭";
                    cm.StatusColor = "#FFFF0006";
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
    }
}
