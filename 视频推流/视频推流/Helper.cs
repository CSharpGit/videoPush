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
        public static readonly string connstr = ConfigurationManager.ConnectionStrings["connstr"].ConnectionString.ToString();

        public bool CreateBatFile(string[] command)//批量创建bat文件
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
                    int num = i + 1;
                    batFileName[i] = folderName + "\\" + num + ".bat";//设置第i个bat文件的文件名
                    FileStream fs = File.Create(batFileName[i]);//创建第i个bat文件
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
        
        public void ExcuteBatFile(string[] batPath)//执行bat文件
        {
            using (Process process = new Process())
            {
                foreach (var p in batPath)
                {
                    FileInfo file = new FileInfo(p);
                    if (file.Directory != null)
                    {
                        process.StartInfo.WorkingDirectory = file.Directory.FullName;
                    }
                }
                Task[] taskArray = new Task[batPath.Length];
                for (int i = 0; i < taskArray.Length; i++)
                {
                    taskArray[i] = Task.Factory.StartNew(() => {
                        process.StartInfo.FileName = batPath[i];
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.Start();
                        StaticInfo.processId.Add(process.Id);//记录启用的进程id
                        process.WaitForExit();
                        process.Close();
                    });
                    Thread.Sleep(1000);
                }
            }
        }

        public void ExcuteCmdCommand(string[] cmdCommand)//执行cmd命令
        {
            using (Process process = new Process())
            {
                Task[] taskArray = new Task[cmdCommand.Length];
                for (int i = 0; i < taskArray.Length; i++)
                {
                    taskArray[i] = Task.Factory.StartNew(() =>{
                        process.StartInfo.FileName = "cmd.exe";//调用程序名称
                        process.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
                        process.StartInfo.RedirectStandardInput = true;// 接受来自调用程序的输入信息
                        process.StartInfo.RedirectStandardOutput = true;//重定向输出信息
                        process.StartInfo.RedirectStandardError = true;//重定向错误信息
                        process.StartInfo.CreateNoWindow = true;//不打开程序窗口
                        process.Start();//启动
                        StaticInfo.processId.Add(process.Id);//记录启用的进程id
                        process.StandardInput.WriteLine(cmdCommand[i] + "&exit");//写入命令
                        process.StandardInput.AutoFlush = true;//自动执行
                        process.WaitForExit();
                        process.Close();
                    });
                    Thread.Sleep(1000);
                }
            }
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

        public bool initMysql()
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

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString)
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
