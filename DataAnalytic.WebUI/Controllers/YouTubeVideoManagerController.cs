using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.ComponentModel;

using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Upload;
using Google.Apis.Services;
using DataAnalytic.WebUI.Infrastructure;
using DataAnalytic.WebUI.Utility.Logging;
using DataAnalytic.WebUI.Utility;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using DataAnalytic.Domain.Abstract;
using DataAnalytic.WebUI.Utility.File;
using YoutubeExtractor;

namespace DataAnalytic.WebUI.Controllers
{
    public class YouTubeVideoManagerController : Controller
    {

        private LogWriter logWriter = LoggingUtility.LogWriter;
        private IObjectRepository<DataAnalytic.Domain.Entities.Video> repository;

        public YouTubeVideoManagerController(IObjectRepository<DataAnalytic.Domain.Entities.Video> videoRepository)
        {
            repository = videoRepository;
        }

        public async Task<ActionResult> IndexAsync(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);

            if (result.Credential != null)
            {
                var youtubeService = GetYouTubeService(result);

                /****************************************************
                 * Get list of video uploaded
                 * 
                 * **************************************************/
                //var channelsListRequest = youtubeService.Channels.List("contentDetails");
                //channelsListRequest.Mine = true;

                //// Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
                //var channelsListResponse = await channelsListRequest.ExecuteAsync();

                //foreach (var channel in channelsListResponse.Items)
                //{
                //    // From the API response, extract the playlist ID that identifies the list
                //    // of videos uploaded to the authenticated user's channel.
                //    var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;

                //    //Console.WriteLine("Videos in list {0}", uploadsListId);

                //    var nextPageToken = "";
                //    while (nextPageToken != null)
                //    {
                //        var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                //        playlistItemsListRequest.PlaylistId = uploadsListId;
                //        playlistItemsListRequest.MaxResults = 50;
                //        playlistItemsListRequest.PageToken = nextPageToken;

                //        // Retrieve the list of videos uploaded to the authenticated user's channel.
                //        var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                //        foreach (var playlistItem in playlistItemsListResponse.Items)
                //        {
                //            // Print information about each video.
                //            //Console.WriteLine("{0} ({1})", playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId);
                //        }

                //        nextPageToken = playlistItemsListResponse.NextPageToken;
                //    }

                //}


                return View();

            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }

        }

        public async Task<ActionResult> GetVideos(string channelId, string playListId)
        {
            List<string> videoList = new List<string>();

            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(System.Threading.CancellationToken.None);

            if (result.Credential != null)
            {
                var youtubeService = GetYouTubeService(result);

                if (!string.IsNullOrEmpty(playListId))
                {
                    videoList = await GetVideosByPlaylist(playListId, youtubeService);
                }
                else
                {
                    videoList = await GetVideosByChannel(channelId, youtubeService);
                }
                return View(videoList);
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }

        }

        public async Task<string> UploadVideo(int id)
        {
            DataAnalytic.Domain.Entities.Video video = repository.GetDataSet.FirstOrDefault(v => v.ID == id);
            if (video != null)
            {
                var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(System.Threading.CancellationToken.None);

                if (result.Credential != null)
                {
                    System.Uri uri = new System.Uri(video.URL);
                    string filePath = System.IO.Path.Combine(DataAnalytic.WebUI.Utility.File.FileUtility.BaseFilePath, uri.Host, video.TvChannel, System.Web.HttpUtility.UrlDecode(uri.PathAndQuery.Substring(uri.PathAndQuery.LastIndexOf("/") + 1)));
                    if (System.IO.File.Exists(filePath))
                    {
                        var youtubeService = GetYouTubeService(result);
                        string videoId = await UploadVid(video.Title, video.Description, video.Tags == null ? null : video.Tags.Split(','), video.Category, filePath, youtubeService);
                        return "File was uploaded successfully";
                    }
                }
                else
                {
                    return "This request required authentication";
                }
            }

            return "The video does not exist";

        }

        public async Task<ActionResult> Search()
        {
            //var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(System.Threading.CancellationToken.None);

            //if (result.Credential != null)
            //{
            return View();
            //}
            //else
            //{
            //    return new RedirectResult(result.RedirectUri);
            //}
        }

        [HttpPost]
        public async Task<PartialViewResult> Search(string keywords)
        {
            //var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(System.Threading.CancellationToken.None);
            //if (result.Credential != null)
            //{
            var youtubeService = GetYouTubeService();

            Dictionary<string, string> videos = new Dictionary<string, string>();
            var nextPageToken = "";
            while (nextPageToken != null && !string.IsNullOrEmpty(keywords))
            {

                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Q = keywords;
                searchListRequest.Type = "video";
                searchListRequest.MaxResults = 50;
                searchListRequest.PageToken = nextPageToken;

                // Call the search.list method to retrieve results matching the specified query term.
                var searchListResponse = await searchListRequest.ExecuteAsync();




                //List<string> videos = new List<string>();
                //List<string> channels = new List<string>();
                //List<string> playlists = new List<string>();

                // Add each result to the appropriate list, and then display the lists of
                // matching videos, channels, and playlists.
                foreach (var searchResult in searchListResponse.Items)
                {
                    switch (searchResult.Id.Kind)
                    {
                        case "youtube#video":
                            var key = string.Format("//www.youtube.com/embed/{0};https://www.youtube.com/watch?v={0};{1}", searchResult.Id.VideoId, searchResult.Snippet.Thumbnails.Default.Url);
                            if (!videos.ContainsKey(key))
                            {
                                videos.Add(key, string.Format("{0};{1}", searchResult.Snippet.Title, searchResult.Snippet.ChannelTitle));
                            }
                            break;

                        //    case "youtube#channel":
                        //        channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                        //        break;

                        //    case "youtube#playlist":
                        //        playlists.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
                        //        break;
                    }
                }
                //nextPageToken = searchListResponse.NextPageToken;
                nextPageToken = null; // temporarily stop searching
            }
            return PartialView(videos);
            //}
            //else
            //{
            //    // How to notify user errors happened? 
            //    return null;
            //}

        }

        private async Task<List<string>> GetVideosByChannel(string channelId, YouTubeService youtubeService)
        {
            List<string> videoList = new List<string>();


            /****************************************************
             * Get list of video uploaded
             * 
             * **************************************************/
            var channelsListRequest = youtubeService.Channels.List("contentDetails");
            if (!string.IsNullOrEmpty(channelId))
            {
                channelsListRequest.Id = channelId;
            }

            channelsListRequest.Mine = true;

            // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
            var channelsListResponse = await channelsListRequest.ExecuteAsync();

            foreach (var channel in channelsListResponse.Items)
            {
                // From the API response, extract the playlist ID that identifies the list
                // of videos uploaded to the authenticated user's channel.
                var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;

                var nextPageToken = "";
                while (nextPageToken != null)
                {
                    var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                    playlistItemsListRequest.PlaylistId = uploadsListId;
                    playlistItemsListRequest.MaxResults = 50;
                    playlistItemsListRequest.PageToken = nextPageToken;

                    // Retrieve the list of videos uploaded to the authenticated user's channel.
                    var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                    foreach (var playlistItem in playlistItemsListResponse.Items)
                    {
                        videoList.Add(playlistItem.Snippet.ResourceId.VideoId);
                    }

                    nextPageToken = playlistItemsListResponse.NextPageToken;
                }

            }

            return videoList;
        }

        private async Task<List<string>> GetVideosByPlaylist(string playlistId, YouTubeService youtubeService)
        {
            List<string> videoList = new List<string>();

            /****************************************************
             * Get list of video uploaded
             * 
             * **************************************************/
            var nextPageToken = "";
            while (nextPageToken != null)
            {
                var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                playlistItemsListRequest.PlaylistId = playlistId;
                playlistItemsListRequest.MaxResults = 50;
                playlistItemsListRequest.PageToken = nextPageToken;

                // Retrieve the list of videos uploaded to the authenticated user's channel.
                var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                foreach (var playlistItem in playlistItemsListResponse.Items)
                {
                    videoList.Add(playlistItem.Snippet.ResourceId.VideoId);
                }

                nextPageToken = playlistItemsListResponse.NextPageToken;
            }

            return videoList;
        }

        private YouTubeService GetYouTubeService(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult result)
        {
            return new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = result.Credential,
                /***************************************
                 * For Production Environment
                 * *************************************/
                //ApplicationName = "ForMyKids"
                /***************************************
                 * For Development
                 * *************************************/
                ApplicationName = AppConfiguration.GoogleAppName
            });
        }

        private YouTubeService GetYouTubeService()
        {
            return new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = AppConfiguration.GoogleApiKey,
                ApplicationName = AppConfiguration.GoogleAppName
            });
        }

        private async Task<string> UploadVid(string title, string description, string[] tags, string category, string filePath, YouTubeService youtubeService)
        {
            /**********************************************************************
               * 
               * upload a video to youtube channel
               * ********************************************************************/
            var video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = title; // "Banana Detectives";
            video.Snippet.Description = description; // "Banana Detectives";
            video.Snippet.Tags = tags; //new string[] { "BANANAS", "BANANAS PYJAMAS" };
            video.Snippet.CategoryId = category; // "24" => Entertainment, see https://developers.google.com/youtube/v3/docs/videoCategories/list
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = "public"; // or "private" or "public"
            //var filePath = @"C:\\Temp\\mpegmedia.abc.net.au\\BANANAS-IN-PYJAMAS\\ABC4KIDS%20Banana%20Detectives.mp4"; 

            const int KB = 0x400;
            var minimumChunkSize = 256 * KB;

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;
                videosInsertRequest.ChunkSize = minimumChunkSize * 4;

                await videosInsertRequest.UploadAsync();
            }

            /**********************************************************
             * Delete the video in local storage to save the disk space
             * ********************************************************/
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (System.Exception) { }


            return video.Id; // return the id of the video ?
        }
        #region DownloadYoutubeVideo
        public async Task<bool> DownloadYoutubeVideo(string link)
        {

            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            /*
             * Try to get the first .mp4 video with 480p resolution otherwise 480p
             */
            VideoInfo video = videoInfos.FirstOrDefault(info => info.VideoType == VideoType.Mp4 && info.Resolution == 480);

            if (video == null)
            {
                video = videoInfos.First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);
            }


            if (!Directory.Exists(Path.Combine(FileUtility.BaseFilePath, "www.youtube.com", "Youtube")))
            {
                Directory.CreateDirectory(Path.Combine(FileUtility.BaseFilePath, "www.youtube.com", "Youtube"));
            }

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */

            var videoDownloader = new VideoDownloader(video, Path.Combine(FileUtility.BaseFilePath, "www.youtube.com", "Youtube", FileUtility.FileNameEncode(video.Title) + video.VideoExtension));

            /*
           * Execute the video downloader.
           * For GUI applications note, that this method runs synchronously.
           */
            videoDownloader.Execute();

            /*************************************************
             * create a corresponding record in database
             ***********************************************/


            var youtubeService = GetYouTubeService();

            /****************************************************
             * Get video detail
             * 
             * **************************************************/
            var videosListRequest = youtubeService.Videos.List("snippet");
            videosListRequest.Id = link.Substring(link.IndexOf("v=") + 2);
            if (!string.IsNullOrEmpty(videosListRequest.Id))
            {
                // Retrieve the snippet part of the video resource.
                var videosListResponse = videosListRequest.Execute();
                DataAnalytic.Domain.Entities.Video videoEntity = new DataAnalytic.Domain.Entities.Video
                {
                    Title = videosListResponse.Items[0].Snippet.Title,
                    Description = videosListResponse.Items[0].Snippet.Description,
                    Category = videosListResponse.Items[0].Snippet.CategoryId,
                    //Tags = videosListResponse.Items[0].Snippet.Tags //this is only visible to video uploader at the moment :(
                    TvChannel = "YouTube",
                    IsAvailable = true,
                    UpdatedDate = System.DateTime.Now,
                    URL = string.Concat("http://www.youtube.com/", FileUtility.FileNameEncode(video.Title) + video.VideoExtension)
                };
                repository.AddObject(videoEntity);
            }

            //await SendMessage(AppConfiguration.YouTubeVideoDownloadNotificationList.Split(';'), "Your movie download completed", string.Format("Your movie {0} has been downloaded successfully. Please visit video manager to review it", FileUtility.FileNameEncode(video.Title) + video.VideoExtension));

            SendMail("Anh Tuan; My Quyen", AppConfiguration.YouTubeVideoDownloadNotificationList, "Your movie download completed", string.Format("Your movie {0} has been downloaded successfully. Please visit video manager to review it", FileUtility.FileNameEncode(video.Title) + video.VideoExtension),true);
    
            return true;
        }
        #endregion

        private async Task SendMessage(string[] tos, string subject, string body)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress("phan.anh.tuan@gmail.com");
            foreach (var to in tos)
            {
                message.To.Add(to);
            }
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            message.Priority = MailPriority.High;
            SmtpClient smtp = new SmtpClient(AppConfiguration.SmtpServer);
            smtp.Port = int.Parse(AppConfiguration.SmtpServicePort);
            smtp.Credentials = new NetworkCredential("phan.anh.tuan@gmail.com", "unisoft");
            smtp.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(message);
        }
        #region SendAsyncCancel
        /// <summary>
        /// this code used to SmtpClient.SendAsyncCancel Method
        /// </summary>
        // static bool mailSent = false;
        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                logWriter.Write("Email Send canceled.");
            }
            if (e.Error != null)
            {
                logWriter.Write("Email Send failed.");
            }
            else
            {
                logWriter.Write("Email Sent successully.");
            }
        }
        #endregion
        private void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    logWriter.Write(string.Format("{0} bytes sent.", progress.BytesSent));
                    break;

                case UploadStatus.Failed:
                    logWriter.Write(string.Format("An error prevented the upload from completing.\n{0}", progress.Exception));
                    break;
            }
        }

        private void videosInsertRequest_ResponseReceived(Video video)
        {
            string tags = null;
            if (video.Snippet.Tags != null)
            {
                tags = string.Join(",", video.Snippet.Tags);
            }

            DataAnalytic.Domain.Entities.Video entity = repository.GetDataSet.FirstOrDefault(v => v.Title == video.Snippet.Title && v.Tags == tags);
            if (entity != null)
            {
                entity.YouTubeId = video.Id;
                repository.SaveObject(entity);
            }

            //SendMessage(AppConfiguration.YouTubeVideoDownloadNotificationList.Split(';'), "Your movie has been uploaded successfully", string.Format("Your movie {0} has been uploaded successfully. Please visit youtube channel to review it", video.Snippet.Title));
            SendMail("Anh Tuan; My Quyen", AppConfiguration.YouTubeVideoDownloadNotificationList, "Your movie has been uploaded successfully", string.Format("Your movie {0} has been uploaded successfully. Please visit youtube channel to review it", video.Snippet.Title), true);
            logWriter.Write(string.Format("Video id '{0}' was successfully uploaded.", video.Id));
        }
        #region SendEmail in Somee
        private void SendMail(string sToName, string sToEmail, string sHeader, string sMessage, bool fSSL)
        {
            try
            {
                string sHost = AppConfiguration.SmtpServer;
                int nPort = int.Parse(AppConfiguration.SmtpServicePort);
                string sUserName = "phan.anh.tuan@gmail.com";
                string sPassword = "unisoft";
                string sFromName = "Phan Anh Tuan";
                string sFromEmail = "phan.anh.tuan@gmail.com";
                if (sToName.Length == 0)
                    sToName = sToEmail;
                if (sFromName.Length == 0)
                    sFromName = sFromEmail;

                System.Web.Mail.MailMessage Mail = new System.Web.Mail.MailMessage();
                Mail.Fields["http://schemas.microsoft.com/cdo/configuration/smtpserver"] = sHost;
                Mail.Fields["http://schemas.microsoft.com/cdo/configuration/sendusing"] = 2;

                Mail.Fields["http://schemas.microsoft.com/cdo/configuration/smtpserverport"] = nPort.ToString();

                if (fSSL)
                    Mail.Fields["http://schemas.microsoft.com/cdo/configuration/smtpusessl"] = "true";


                Mail.Fields["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"] = 1;
                Mail.Fields["http://schemas.microsoft.com/cdo/configuration/sendusername"] = sUserName;
                Mail.Fields["http://schemas.microsoft.com/cdo/configuration/sendpassword"] = sPassword;


                Mail.To = sToEmail;
                Mail.From = sFromEmail;
                Mail.Subject = sHeader;
                Mail.Body = sMessage;
                Mail.BodyFormat = System.Web.Mail.MailFormat.Html;

                System.Web.Mail.SmtpMail.SmtpServer = sHost;
                System.Web.Mail.SmtpMail.Send(Mail);
            }
            catch (System.Exception ex)
            {
                logWriter.Write(string.Format("Send Mail error {0}",ex.Message));
            }
        }
        #endregion 
    }
}
