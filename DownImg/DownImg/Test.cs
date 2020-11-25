using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DownImg
{
    class Test
    {
        // 任务队列
        static Queue<string> _tasks = new Queue<string>();

        // 为保证线程安全，使用一个锁来保护_task的访问
        readonly static object _locker = new object();

        // 通过 _wh 给工作线程发信号
        static EventWaitHandle _wh = new AutoResetEvent(false);

        static Thread _worker;

        static void MainTwo(string[] args)
        {
            // 需要获取天气情况的城市对应代码
            var cityIds = new List<int> { 101280601, 101010100, 101020100, 101110101, 101040100 };

            // 任务开始，启动工作线程
            _worker = new Thread(Work);
            _worker.Start();

            // 生产者将数据插入队里中，并给工作线程发信号
            foreach (var cityId in cityIds)
                EnqueueTask(FetchData(cityId));

            // 任务结束
            Dispose();
        }

        /// <summary>执行工作</summary>
        static void Work()
        {
            while (true)
            {
                string work = null;
                lock (_locker)
                {
                    if (_tasks.Count > 0)
                    {
                        work = _tasks.Dequeue(); // 有任务时，出列任务

                        if (work == null)  // 退出机制：当遇见一个null任务时，代表任务结束
                            return;
                    }
                }

                if (work != null)
                    SaveData(work);  // 任务不为null时，处理并保存数据
                else
                    _wh.WaitOne();   // 没有任务了，等待信号
            }
        }

        /// <summary>插入任务</summary>
        static void EnqueueTask(string task)
        {
            lock (_locker)
                _tasks.Enqueue(task);  // 向队列中插入任务 

            _wh.Set();  // 给工作线程发信号
        }

        /// <summary>结束释放</summary>
        static void Dispose()
        {
            EnqueueTask(null);      // 插入一个Null任务，通知工作线程退出
            _worker.Join();         // 等待工作线程完成
            _wh.Close();            // 释放资源
        }

        /// <summary>获取数据</summary>
        static string FetchData(int cityId)
        {
            var wc = new WebClient { Encoding = Encoding.UTF8 };
            var url = string.Format("http://www.weather.com.cn/adat/sk/{0}.html", cityId);

            return wc.DownloadString(url);
        }

        /// <summary>处理保存</summary>
        static void SaveData(string data)
        {
            //var weatherInfo = (JsonConvert.DeserializeObject(data, typeof(Dictionary<string, Weatherinfo>)) as Dictionary<string, Weatherinfo>)["weatherinfo"];

            //Console.WriteLine("[{0}]：{1} 气温（{2}） 风向（{3}） 风力（{4}）", weatherInfo.Time, weatherInfo.City, weatherInfo.Temp, weatherInfo.Wd, weatherInfo.Ws);

            Thread.Sleep(200);  // 模拟数据保存
        }
    }

    public class Weatherinfo
    {
        public string City { get; set; }
        public string Temp { get; set; }
        public string Time { get; set; }
        public string Wd { get; set; }
        public string Ws { get; set; }
    }
}
