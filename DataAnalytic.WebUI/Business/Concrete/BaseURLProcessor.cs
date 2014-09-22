using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAnalytic.WebUI.Business.Abstract;
using System.Net;
using System.IO;
using System;
using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using DataAnalytic.WebUI.App_Start;
using DataAnalytic.WebUI.Utility.Logging;

namespace DataAnalytic.WebUI.Business.Concrete
{
    public class BaseURLProcessor : IURLProcessor
    {
        private const String BASE_FILE_PATH = "BaseFilePath";
        private static String BaseFilePath = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/").AppSettings.Settings[BASE_FILE_PATH].Value;
        private LogWriter logWriter = LoggingUtility.LogWriter;

        public virtual void Process(string url)
        {
            string filepath = BuildFilePath(url);
            DownloadFile(url, filepath, true);
        }

        protected void DownloadFile(string url, string path, bool isAbsoluteFilePath = false)
        {
            DataAnalytic.WebUI.Utility.File.FileUtility.DownloadFile(url, path, isAbsoluteFilePath);
            //string filepath;
            
            //if (isAbsoluteFilePath) {
            //    filepath = path;
            //} else {
            //    filepath = String.Concat(BaseFilePath, path);
            //}

            //if (!string.IsNullOrEmpty(filepath) && !File.Exists(filepath))
            //{
            //    try
            //    {
            //        WebClient client = new WebClient();

            //        Stream reader = client.OpenRead(url);

            //        Byte[] buffer = new Byte[256];

            //        Stream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write);

            //        int count = reader.Read(buffer, 0, 256);
            //        while (count > 0)
            //        {
            //            fs.Write(buffer, 0, count);
            //            count = reader.Read(buffer, 0, 256);
            //        }

            //        reader.Close();

            //        fs.Flush();
            //        fs.Close();
            //    }
            //    catch (Exception ex)
            //    {
            //        throw ex;
            //    }
            //}
        }

        public virtual string BuildFilePath(string url)
        {
            Uri uri;
            string filepath = "";

            if (Uri.IsWellFormedUriString(url, System.UriKind.RelativeOrAbsolute))
            {
                uri = new Uri(url);
                
                string filename = uri.GetComponents(System.UriComponents.Path, System.UriFormat.UriEscaped);

                if (string.IsNullOrEmpty(filename))
                {
                    filename = "index.html";
                }
                else
                {
                    filename = filename.Substring(filename.LastIndexOf('/')+1);
                    if (filename.IndexOf('.') < 0)
                    {
                        filename = string.Concat(filename, ".html");
                    }
                }

                               
                string host = uri.GetComponents(System.UriComponents.Host, System.UriFormat.UriEscaped);
                string parentFolder = String.Concat(BaseFilePath, host);
                string childFolder = String.Concat(parentFolder,"\\",DateTime.Now.ToString("yyyy-MMMM-dd"));

                if (!Directory.Exists(parentFolder))
                {
                    Directory.CreateDirectory(parentFolder);
                }

                if (!Directory.Exists(childFolder))
                {
                    Directory.CreateDirectory(childFolder);
                }

                //check if the file container exists
                filepath = String.Concat(childFolder,"\\",filename);
            }

            return filepath;
        }

        public virtual string ToString()
        {
            return "Greating from BaseFile Processor";
        }
    }
}