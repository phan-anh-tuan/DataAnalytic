using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataAnalytic.WebUI.Utility
{
    public class AppConfiguration
    {
        private const string PAGE_SIZE = "PageSize";
        public static int PageSize = Int32.Parse(System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/").AppSettings.Settings[PAGE_SIZE].Value);

        private const String BASE_FILE_PATH = "BaseFilePath";
        public static String BaseFilePath = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/").AppSettings.Settings[BASE_FILE_PATH].Value;

        private const String GOOGLE_CLIENTID = "GoogleAPICredential_ClientId";
        public static String GoogleClientId = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/").AppSettings.Settings[GOOGLE_CLIENTID].Value;

        private const String GOOGLE_CLIENTSECRET = "GoogleAPICredential_ClientSecret";
        public static String GoogleClientSecret = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/").AppSettings.Settings[GOOGLE_CLIENTSECRET].Value;

        private const String GOOGLE_APIKEY = "GoogleAPICredential_ApiKey";
        public static String GoogleApiKey = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/").AppSettings.Settings[GOOGLE_APIKEY].Value;

        private const String GOOGLE_APPNAME = "GoogleAPICredential_ApplicationName";
        public static String GoogleAppName = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/").AppSettings.Settings[GOOGLE_APPNAME].Value;

        private const string SMTP_SERVER = "SMTPServer";
        public static string SmtpServer= System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/").AppSettings.Settings[SMTP_SERVER].Value;

        private const string SMTP_SERVICE_PORT = "SMTPServicePort";
        public static string SmtpServicePort = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/").AppSettings.Settings[SMTP_SERVICE_PORT].Value;

        private const string YOUTUBE_VIDEO_DOWNLOAD_NOTIFICAtION_LIST = "YoutubeVideoDownload_NotificationRecipients";
        public static string YouTubeVideoDownloadNotificationList = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/").AppSettings.Settings[YOUTUBE_VIDEO_DOWNLOAD_NOTIFICAtION_LIST].Value;

        private const string FTP_FOLDER = "FTPFolder";
        public static string FtpFolder = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/").AppSettings.Settings[FTP_FOLDER].Value;
    }
}