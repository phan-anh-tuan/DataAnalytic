using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using DataAnalytic.WebUI.Utility;

namespace DataAnalytic.WebUI.Utility.File
{
    public class FileUtility
    {
        //public static String BaseFilePath = AppConfiguration.BaseFilePath;

        public static String BaseFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/LocalStorage"); //System.Web.HttpContext.Current.Server.MapPath("LocalStorage"); 
        
        public static string CreateSubFolders(params string[] folders) 
        {
            string parentFolder = BaseFilePath;
            try
            {
                foreach (string folder in folders)
                {
                    string childFolder = Path.Combine(parentFolder, folder);
                    if (!Directory.Exists(childFolder))
                    {
                        Directory.CreateDirectory(childFolder);
                    }
                    parentFolder = childFolder;
                }
                return parentFolder;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static void DownloadFile(string url, string path, bool isAbsoluteFilePath = false)
        {
            string filepath;

            if (isAbsoluteFilePath)
            {
                filepath = path;
            }
            else
            {
                filepath = String.Concat(BaseFilePath, path);
            }

            if (!string.IsNullOrEmpty(filepath) && !System.IO.File.Exists(filepath))
            {
                try
                {
                    WebClient client = new WebClient();

                    Stream reader = client.OpenRead(url);

                    Byte[] buffer = new Byte[256];

                    Stream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write);

                    int count = reader.Read(buffer, 0, 256);
                    while (count > 0)
                    {
                        fs.Write(buffer, 0, count);
                        count = reader.Read(buffer, 0, 256);
                    }

                    reader.Close();

                    fs.Flush();
                    fs.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static string FileNameEncode(string fileName)
        {
            string newFileName = fileName;
            foreach (var character in Path.GetInvalidFileNameChars())
            {
                newFileName = newFileName.Replace(character, ' ');
            }
            return newFileName;
        }
    }
}