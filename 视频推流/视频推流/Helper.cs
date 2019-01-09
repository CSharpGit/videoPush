using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 视频推流
{
    public class Helper
    {
        public static readonly string connstr = ConfigurationManager.ConnectionStrings["connstr"].ConnectionString.ToString();//连接字符串

        /**
         * 传入参数：写入bat文件的命令数组
         * 功能：根据传入的数组大小，创建传入n(数组长度)个bat文件
         */
        public static bool CreateBatFile(string[] command)
        {
            string path = Directory.GetCurrentDirectory();//获取项目文件目录
            string folderName = path + "\\allBat";//需要创建的文件夹名称
            try
            {
                if (!Directory.Exists(folderName))//如果文件夹不存在
                {
                    Directory.CreateDirectory(folderName);//创建该文件夹
                }
                string[] batFileName = new string[command.Length];
                for (int i = 0; i < command.Length; i++)
                {
                    batFileName[i] = folderName + "\\" + StaticInfo.cameraName[i] + ".bat";//设置第i个bat文件的文件名
                    FileStream fs = File.Create(batFileName[i]);//创建第i个bat文件
                    StaticInfo.batFileName.Add(batFileName[i]);//记录bat文件名
                    fs.Close();
                    StreamWriter sw = new StreamWriter(batFileName[i]);
                    sw.WriteLine(command[i]);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception)
            {

                //throw;
                return false;
            }
            return true;
        }
        
        /**
         * 传入参数：父进程id
         * 功能：根据父进程id，杀死与之相关的进程树
         */
        public static void KillProcessAndChildren(int pId)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pId);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pId);
                Console.WriteLine("进程{0}已被强行关闭！",pId);
                proc.Kill();
                proc.WaitForExit();
                proc.Close();
            }
            catch (Exception)
            {
                //
            }
        }

        /**
         * 传入参数：
         * 功能：检测MySQL的连接情况
         * 返回值：true，连接成功；false，连接失败
         */
        public static bool InitMysql()
        {
            using(MySqlConnection connection = new MySqlConnection(connstr))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (Exception)
                {
                    //throw;
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        /**
         * 传入参数：（string）SQL语句
         * 功能：执行传入的语句，返回DataSet
         * 返回值：DataSet数据集
         */
        public static DataSet Query(string SQLString)
        {
            using (MySqlConnection connection = new MySqlConnection(connstr))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    MySqlDataAdapter command = new MySqlDataAdapter(SQLString, connection);
                    command.Fill(ds);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                return ds;
            }
        }
    }
}
