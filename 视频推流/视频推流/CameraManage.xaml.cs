using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Controls;

namespace 视频推流
{
    /// <summary>
    /// CameraManage.xaml 的交互逻辑
    /// </summary>
    public partial class CameraManage : Window
    {
        public CameraManage()
        {
            InitializeComponent();
            InitialCameraList();
        }

        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show((e.OriginalSource as FrameworkElement).Name);
        }

        //初始化摄像头
        private void InitialCameraList()
        {
            List<Camera> CameraList = new List<Camera>()
            {
                new Camera() {ProcessId ="10000",CameraName="摄像头1",Status=1},
                new Camera() {ProcessId ="10001",CameraName="摄像头2",Status=2},
                new Camera() {ProcessId ="10002",CameraName="摄像头3",Status=3},
                new Camera() {ProcessId ="10003",CameraName="摄像头1",Status=1},
                new Camera() {ProcessId ="10004",CameraName="摄像头2",Status=2},
                new Camera() {ProcessId ="10005",CameraName="摄像头3",Status=3},
                new Camera() {ProcessId ="10006",CameraName="摄像头1",Status=1},
                new Camera() {ProcessId ="10007",CameraName="摄像头2",Status=2},
                new Camera() {ProcessId ="10008",CameraName="摄像头3",Status=3},
                new Camera() {ProcessId ="10009",CameraName="摄像头1",Status=1},
                new Camera() {ProcessId ="10010",CameraName="摄像头2",Status=2},
                new Camera() {ProcessId ="10011",CameraName="摄像头3",Status=3},
                new Camera() {ProcessId ="10012",CameraName="摄像头1",Status=1},
                new Camera() {ProcessId ="10013",CameraName="摄像头2",Status=2},
                new Camera() {ProcessId ="10014",CameraName="摄像头3",Status=3},
            };
            //填充数据源
            this.ListBoxCars.ItemsSource = CameraList;
        }
    }

    public class NameToLogoPathConverter : IValueConverter
    {
        //正向转换
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string uriStr = string.Format(@"/Resources/img/{0}.jpg", (string)value);
            return new BitmapImage(new Uri(uriStr, UriKind.Relative));
        }

        //未被用到
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
