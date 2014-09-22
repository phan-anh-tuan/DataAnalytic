using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using DataAnalytic.WebUI.Business.Abstract;
using DataAnalytic.WebUI.Business.Concrete;
using HtmlAgilityPack;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using DataAnalytic.WebUI.App_Start;
using DataAnalytic.WebUI.Utility.Logging;
using DataAnalytic.Domain.Entities;
using DataAnalytic.Domain.Abstract;
using System.Text.RegularExpressions;


namespace DataAnalytic.WebUI.Business.Concrete
{
    public class Abc4KidProcessor : BaseURLProcessor
    {
        private LogWriter logWriter = LoggingUtility.LogWriter;
        private IObjectRepository<Video> repository = UnityConfig.GetConfiguredContainer().Resolve<IObjectRepository<Video>>();

        private Dictionary<string, string> GetChannelList(string url)
        {
            /**************************************************
             * sample Dictionary item
             * "JUSTINE-CLARKE-SONGS-TO-MAKE-YOU-SMILE", "/abcforkids/video/show.htm?show=JUSTINE-CLARKE-SONGS-TO-MAKE-YOU-SMILE&videoId=4057931"
             * 
             * ************************************************/
            Dictionary<string, string> channels = new Dictionary<string, string>();

            WebClient client = new WebClient();
            HtmlDocument doc = new HtmlDocument();
            doc.Load(client.OpenRead(url));
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];
                /*************************************************************************
                 * sample URL /abcforkids/video/show.htm?show=PEPPA-PIG&videoId=3146854
                 * ***********************************************************************/
                if (att.Value.StartsWith("/abcforkids/video/show.htm"))
                {
                    string sPattern = @"show=[\w|\-]*";
                    foreach (Match match in Regex.Matches(att.Value, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        //logWriter.Write(string.Format("tv channel {0}", match.Value));
                        string channelName = match.Value.Substring(5);
                        if (!channels.ContainsKey(channelName))
                        {
                            channels.Add(channelName, att.Value);
                        }
                    }
                }
            }

            return channels;
        }

        private List<string> GetVideoList(string channelUrl)
        {
            /*************************************************************************
            * sample List item ?show=ARTHUR&amp;videoId=3975147
            * ***********************************************************************/
            List<string> videos = new List<string>();

            WebClient client = new WebClient();
            HtmlDocument doc = new HtmlDocument();
            doc.Load(client.OpenRead(channelUrl));
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];
                /*************************************************************************
                 * sample URL ?show=ARTHUR&amp;videoId=3975147
                 * ***********************************************************************/
                if (att.Value.StartsWith("?show="))
                {
                    //logWriter.Write(string.Format("tv video {0}", att.Value));
                    videos.Add(att.Value);
                }
            }

            return videos;
        }

        private string GetVideoUrl(string url)
        {
            string videoUrl = "";
            WebClient client = new WebClient();
            HtmlDocument doc = new HtmlDocument();
            
            doc.Load(client.OpenRead(url));
            foreach (HtmlNode scriptNodes in doc.DocumentNode.SelectNodes("//script"))
            {

                string s = scriptNodes.InnerText;
                //string sPattern = @"var\s*videoClip\s*=\s*\{(\w\W)*\}";
                string sPattern = @"http:[\w\W]*.mp4";
                foreach (Match match in Regex.Matches(s, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    //logWriter.Write(string.Format("Video URL = {0}", match.Value));
                    videoUrl = match.Value;
                }
            }

            //if (string.IsNullOrWhiteSpace(videoUrl))
            //{
            //    DataAnalytic.WebUI.Utility.File.FileUtility.DownloadFile(url, "c:\\temp\\file.htm", true);
            //}
            return videoUrl;
        }

        public override void Process(string url)
        {

            /*****************************************************************************
             * step1: Get tv channel
             * step2: For each tv channel get the video list then put them into database.
             *****************************************************************************/
            try
            {

                /**************************************************
                    * sample Dictionary item
                    * "JUSTINE-CLARKE-SONGS-TO-MAKE-YOU-SMILE", "/abcforkids/video/show.htm?show=JUSTINE-CLARKE-SONGS-TO-MAKE-YOU-SMILE&videoId=4057931"
                    * 
                    * ************************************************/
                Dictionary<string, string> channels = GetChannelList(url);
                foreach (KeyValuePair<string, string> kvp in channels)
                {
                    /*************************************************************************
                       * sample Video List item ?show=ARTHUR&amp;videoId=3975147
                       * ***********************************************************************/
                    Uri uri = new Uri(url);
                    string channelUrl = string.Concat("http://", uri.Host, kvp.Value);
                    List<string> videos = GetVideoList(channelUrl);
                    foreach (string svideo in videos)
                    {
                        //build the url like http://www.abc.net.au/abcforkids/video/show.htm?show=ARTHUR&amp;videoId=3975147
                        
                        
                        string videoUrl = GetVideoUrl(string.Concat("http://", uri.Host, new Uri(channelUrl).AbsolutePath, svideo));
                        
                        if (!string.IsNullOrWhiteSpace(videoUrl))
                        {
                            Uri videoUri = new Uri(videoUrl);
                            string videoName = videoUri.PathAndQuery.Substring(videoUri.PathAndQuery.LastIndexOf("/") + 1);
                            videoName = System.Web.HttpUtility.UrlDecode(videoName.Substring(0, videoName.IndexOf("."))).Replace("_", " ").Replace("-", " ");
                            
                            // persist the video to database
                            Video video = new Video
                            {
                                TvChannel = kvp.Key,
                                URL = videoUrl,
                                Title = videoName,
                                Description = videoName,
                                Tags =  string.Join(",",videoName.Split(' ')) + "," + videoName,
                                Category = "24", // Entertainment
                                IsAvailable = false,
                                UpdatedDate = DateTime.Now
                            };
                            repository.AddObject(video);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logWriter.Write(string.Format("Error happened in Abc4KidProcessor.Process: ", ex.Message));
                throw ex;
            }
        }

        public override string BuildFilePath(string url)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Greating from Abc4Kids Processor";
        }
    }
}