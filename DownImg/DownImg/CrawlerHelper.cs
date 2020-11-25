using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;

namespace DownImg
{
    public class CrawlerHelper
    {
        private static HttpClient _imageDownloadClient;
        public static LoggerHelper logger = new LoggerHelper();
        /// <summary>
        /// 下载图片
        /// </summary>
        public static void DownLoadImage(string url, string path)
        {
            if (string.IsNullOrWhiteSpace(url)) { return; }
            _imageDownloadClient = HttpClientFactory.CreateClient(Thread.CurrentThread.ManagedThreadId);
            var responseTask = _imageDownloadClient.GetByteArrayAsync(url);
            responseTask.Wait();
            var filename = url.Substring(url.LastIndexOf('/') + 1);
            using (var responseStream = new MemoryStream(responseTask.Result))
            {
                using (var writeStream = new FileStream(Path.Combine(path, filename), FileMode.Create))
                {
                    var bufferArr = new byte[2048];
                    var readSize = responseStream.Read(bufferArr, 0, bufferArr.Length);
                    while (readSize > 0)
                    {
                        writeStream.Write(bufferArr, 0, readSize);
                        readSize = responseStream.Read(bufferArr, 0, bufferArr.Length);
                    }
                    writeStream.Close();
                    responseStream.Close();
                }
            }
        }

        /// <summary>
        /// 获取网页源代码
        /// </summary>
        public static HtmlDocument GetHtmlSourceCode(string url, ProxyTuple proxy = null)
        {
            if (string.IsNullOrWhiteSpace(url)) { return null; }
            HtmlDocument document = null;
            var webClient = new HtmlWeb
            {
                UserAgent = "User-Agent:Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36"
            };
            try
            {
                document = proxy == null ? webClient.Load(url) : webClient.Load(url, proxy.Host, proxy.Port, proxy.UserId, proxy.Password);
            }
            catch
            {
                return null;
            }
            return document;
        }

        /// <summary>
        /// 取出源代码中的图片信息
        /// </summary>
        public static List<string> GetPictureUrls(HtmlDocument document)
        {
            var retlist = new List<string>();
            string str = "";
            string imgPattern = @"var z_img=\'(.+?)\'";
            if (Regex.IsMatch(document.Text, imgPattern))
            {
                MatchCollection matchcol = Regex.Matches(document.Text, imgPattern);
                foreach (Match match in matchcol)
                {
                    str=match.Groups[1].ToString();
                }
            }
            Regex pattern = new Regex(@"[\[\]""]");
            string abc  = pattern.Replace(str, "");
            string[] strList = abc.Split(',');
            if (strList.Count()>0)
            {
                foreach (string imgUrl in strList)
                {
                    retlist.Add(imgUrl);
                }
            }
            return retlist.Distinct().ToList();
        }
        /// <summary>
        /// 取出源代码中的图片信息
        /// </summary>
        public static List<string> GetPictureUrlsTwo(HtmlDocument document)
        {
            Console.WriteLine("Extract images from html code.");
            var retlist = new List<string>();
            var imgList = document.DocumentNode.SelectNodes("//img[@src]");
            if (imgList != null)
            {
                foreach (HtmlNode imgUrl in imgList)
                {
                    HtmlAttribute att = imgUrl.Attributes["src"];
                    if (!string.IsNullOrWhiteSpace(att.Value))
                    {
                        retlist.Add(att.Value);
                    }
                }
            }
            return retlist.Distinct().ToList();
        }
        /// <summary>
        /// 取出源代码中的链接地址
        /// </summary>
        public static List<string> GetLinkUrls(HtmlDocument document)
        {
            Console.WriteLine("Extract link urls from html code.");
            var retlist = new List<string>();
            var hrefList = document.DocumentNode.SelectNodes("//a[@href]");
            if (hrefList != null)
            {
                foreach (var href in hrefList)
                {
                    var att = href.Attributes["href"];
                    if (!string.IsNullOrWhiteSpace(att.Value) && !att.Value.StartsWith("#") && !att.Value.StartsWith("javascript"))
                    {
                        retlist.Add(att.Value);
                    }
                }
            }
            return retlist;
        }
        public static string GetUrl(HtmlDocument document)
        {
            Console.WriteLine("Extract link urls from html code.");
            var retlist = new List<string>();
            var hrefList = document.DocumentNode.SelectNodes("//a[@href]");
            if (hrefList != null)
            {
                return hrefList.LastOrDefault().Attributes["href"].Value;
            }
            return "";
        }
        /// <summary>
        /// 取出源代码中下一页的链接地址
        /// </summary>
        public static string GetNextPageUrl(HtmlDocument document)
        {
            var retlist = new List<string>();
            var hrefList = document.DocumentNode.SelectNodes("//a[@href]");
            if (hrefList != null)
            {
                foreach (var href in hrefList)
                {
                    string text = href.InnerText;
                    var att = href.Attributes["href"];
                    if (text.Contains("下一章"))
                    {
                        //logger.WriteInfoLog("下一章:"+ att.Value);
                        return att.Value;
                    }
                }
            }
            return "";
        }

    }

    internal class HttpClientFactory
    {
        private static ConcurrentDictionary<int, HttpClient> dicHttpClients => new ConcurrentDictionary<int, HttpClient>();

        /// <summary>
        /// 创建HttpClient
        /// </summary>
        public static HttpClient CreateClient(int threadId)
        {
            if (!dicHttpClients.TryGetValue(threadId, out HttpClient httpClient) || httpClient == null)
            {
                httpClient = new HttpClient
                {
                    Timeout = new TimeSpan(0, 0, 3)
                };
                httpClient.DefaultRequestHeaders.Add("refer", "https://www.baidu.com/");
                httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.143 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("accept", "*/*");
                dicHttpClients.TryAdd(threadId, httpClient);
            }
            return httpClient;
        }
    }

    public class ProxyPool
    {
        private static ConcurrentBag<ProxyTuple> bagProxys => new ConcurrentBag<ProxyTuple>();
        private static Random random => new Random();

        /// <summary>
        /// 添加代理
        /// </summary>
        public static void AddInto(ProxyTuple newProxy)
        {
            ProxyTuple oldProxy;
            newProxy.Useful = true;
            if ((oldProxy = bagProxys.FirstOrDefault(item => item.Host == newProxy.Host)) != null)
            {
                oldProxy.Port = newProxy.Port;
                oldProxy.UserId = newProxy.UserId;
                oldProxy.Password = newProxy.Password;
            }
            else
            {
                newProxy.Index = bagProxys.Count;
                bagProxys.Add(newProxy);
            }
        }

        /// <summary>
        /// 获取指定代理
        /// </summary>
        public static ProxyTuple GetProxy(int index)
        {
            return bagProxys.FirstOrDefault(item => item.Index == index && item.Useful);
        }

        /// <summary>
        /// 获取随机代理
        /// </summary>
        public static ProxyTuple GetRandomProxy()
        {
            ProxyTuple oldProxy;
            do
            {
                int[] enableList = bagProxys.Where(item => item.Useful).Select(item => item.Index).ToArray();
                oldProxy = GetProxy(enableList[random.Next(enableList.Length)]);
            } while (oldProxy == null);
            return oldProxy;
        }

        /// <summary>
        /// 排除代理
        /// </summary>
        public static void ExcludeProxy(int index)
        {
            ProxyTuple oldProxy;
            if ((oldProxy = GetProxy(index)) == null) { return; }
            oldProxy.Useful = false;
        }
    }

    public class ProxyTuple
    {
        public ProxyTuple(string host, int port, string userid = "", string password = "")
        {
            Host = host;
            Port = port;
            UserId = userid;
            Password = password;
        }

        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 有用的
        /// </summary>
        public bool Useful { get; set; }
    }
}