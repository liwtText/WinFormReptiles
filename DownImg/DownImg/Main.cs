using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownImg
{
    public class Main
    {
        public static Queue<string> queue = new Queue<string>();
        public static void GetImg(string url,int page,string imgurl,string urlHade)
        {
            for (int i = 0; i < page; i++)
            {
                HtmlAgilityPack.HtmlDocument htmlcode = CrawlerHelper.GetHtmlSourceCode(url);
                var picturelist = CrawlerHelper.GetPictureUrlsTwo(htmlcode);
                foreach (var item in picturelist)
                {
                    queue.Enqueue(imgurl + item);
                }
                url = urlHade + CrawlerHelper.GetNextPageUrl(htmlcode);
            }
            string imageCollectionDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            if (!Directory.Exists(imageCollectionDirectory))
            {
                Directory.CreateDirectory(imageCollectionDirectory);
            }
            foreach (string str in queue)
            {
                CrawlerHelper.logger.WriteInfoLog("图片路径: |" + str);
                CrawlerHelper.DownLoadImage(str, imageCollectionDirectory);
            }

            //CrawlerHelper.logger.WriteInfoLog("图片路径: |" + queue.Count());
            //linklist = linklist.Distinct().ToList();
            //Parallel.For(0, page, i =>
            //            {
            //                linkList.AddRange(CrawlerHelper.GetLinkUrls(CrawlerHelper.GetHtmlSourceCode(linklist[i])));
            //            });


        }
    }
}
