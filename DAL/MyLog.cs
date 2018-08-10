using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
namespace FSMIS.DAL
{
    public class Mylog
    {
        private static string path = AppDomain.CurrentDomain.BaseDirectory + "Logs";
        private static Mylog _log;
        private Mylog()
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static Mylog Instance
        {
            get
            {
                if (_log == null)
                {
                    _log = new Mylog();
                }
                return _log;
            }

        }
        public void WriteLog(string className, string content)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");//获取当前系统时间
            string filename = path + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";//用日期对日志文件命名

            //创建或打开日志文件，向日志文件末尾追加记录
            StreamWriter mySw = File.AppendText(filename);

            //向日志文件写入内容
            string write_content = time + "  " + className + ": " + content;
            mySw.WriteLine(write_content);

            //关闭日志文件
            mySw.Close();
        }

    }
    
}