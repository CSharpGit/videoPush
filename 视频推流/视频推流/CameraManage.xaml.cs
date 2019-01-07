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
            MessageBox.Show(e.Source.ToString());
            //for (int i = 0; i < CameraList.Count; i++)
            //{
            //    Camera cm = CameraList[i];
            //    cm.Status = "已挂死";
            //    cm.StatusColor = "#FFFF0006";
            //}
            //MessageBox.Show((e.OriginalSource as FrameworkElement).Name);
        }

        //初始化摄像头
        private void InitialCameraList()
        {
            for (int i = 0; i < StaticInfo.cameraName.Count; i++)
            {
                CameraList.Add(new Camera {
                    ProcessId="0",
                    Status= "未启动",
                    StatusColor = "#F7D358",
                    CameraName =StaticInfo.cameraName[i].ToString(),
                });
            }
            this.ListBoxCameras.ItemsSource = CameraList;
        }

        private void StartAll_Click(object sender, RoutedEventArgs e)
        {
            string[] batPath = new string[StaticInfo.cameraName.Count];
            for (int i = 0; i < batPath.Length; i++)
            {
                batPath[i] = Directory.GetCurrentDirectory() + "\\allBat\\" + StaticInfo.cameraName[i] + ".bat";//获取项目文件目录
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

        public void CheckProcess(object sender, ElapsedEventArgs e)
        {
            Process[] process = Process.GetProcessesByName("cmd");
            int[] processId = new int[process.Length];
            for (int i = 0; i < processId.Length; i++)
            {
                processId[i] = process[i].Id;
            }
            for (int i = 0; i < StaticInfo.processId.Count; i++)
            {
                if (Array.IndexOf(processId, Convert.ToInt32(CameraList[i].ProcessId))==-1)//记录的进程Id不存在当前进程中
                {
                    Camera cm = CameraList[i];
                    cm.Status = "已挂死";
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

        private void EndAll_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }
    }
}
