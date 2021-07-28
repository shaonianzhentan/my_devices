using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDevices
{
    public class CmdHelper
    {
        //执行命令
        public static void Exec(string str)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";//调用cmd.exe程序
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;//重定向标准输入
                    process.StartInfo.RedirectStandardOutput = true;//重定向标准输出
                    process.StartInfo.RedirectStandardError = true;//重定向标准出错
                    process.StartInfo.CreateNoWindow = true;//不显示黑窗口
                    process.Start();//开始调用执行
                    process.StandardInput.WriteLine(str + "&exit");//标准输入str + "&exit"，相等于在cmd黑窗口输入str + "&exit"
                    process.StandardInput.AutoFlush = true;//刷新缓冲流，执行缓冲区的命令，相当于输入命令之后回车执行
                    process.WaitForExit();//等待退出
                    process.Close();//关闭进程
                }
            }
            catch
            {
            }
        }

        //执行关机操作
        public static void Shutdown()
        {
            Exec("shutdown -s -f -t 10");
        }

        //执行重启操作
        public static void Restart()
        {
            Exec("shutdown -r -f -t 10");
        }

        public static void CancelShutdown()
        {
            Exec("shutdown -a");
        }
    }
}
