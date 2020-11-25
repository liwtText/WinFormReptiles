using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "App.config", Watch = true)]
namespace DownImg
{
    public class LoggerHelper
    {
        public static log4net.ILog _defaultLoggerinfo = log4net.LogManager.GetLogger("loginfo");
        public static log4net.ILog _defaultLogger = log4net.LogManager.GetLogger("logerror");
        /// <summary>
        /// 输出日志到Log4Net
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        public void WriteInfoLog(string msg)
        {
            _defaultLoggerinfo.Info(msg);
        }

        public static void WriteErrorLog(string msg)
        {
            _defaultLogger.Error(msg);
        }
    }
}
