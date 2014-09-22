using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using System.Text;
using System.Net;
using Quartz;
using Quartz.Job;
using Quartz.Impl;
using HtmlAgilityPack;
using DataAnalytic.Domain.Abstract;
using DataAnalytic.Domain.Concrete;
using DataAnalytic.Domain.Entities;
using DataAnalytic.WebUI.Utility.Logging;
using DataAnalytic.WebUI.Utility.File;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace DataAnalytic.WebUI.Business.Concrete
{
    public class ASXDownloader : IJob
    {
        private LogWriter logWriter = LoggingUtility.LogWriter;
        private IObjectRepository<Security> repository = new EFSecurityRepository();
        #region Job Execution
        public void Execute(IJobExecutionContext context)
        {

            try
            {
                logWriter.Write(string.Format("start executing job {0} at {1}", context.JobDetail.Key, DateTime.Now.ToLongTimeString()));
                if (context.MergedJobDataMap.Keys.Contains("UrlPattern"))
                {
                    /*********************************************
                     * download data from the URL
                     * 
                     * *******************************************/

                    string folder = FileUtility.CreateSubFolders(new string[] { "ASX", DateTime.Now.ToString("yyyy-MM-dd") });
                    string urlPattern = context.MergedJobDataMap.GetString("UrlPattern");
                    IList<Security> securities = repository.GetDataSet.ToList<Security>();
                    int counter = 0;
                    StringBuilder codes = new StringBuilder();
                    foreach (var securityCode in securities)
                    {
                        counter++;
                        if (codes.Length > 0) codes.Append("+");
                        codes.Append(securityCode.Code);
                        if (counter % 10 == 0)
                        {
                            String url = string.Format(urlPattern, codes);
                            FileUtility.DownloadFile(url, System.IO.Path.Combine(folder, string.Concat(counter, ".html")), true);
                            codes = new StringBuilder();
                        }
                    }

                    //consolidate data into a unique file
                    ConsolidateStockTradingData();
                    
                    //upload the file to Server
                    UploadFile();
                }
                else
                {
                    /**************************************************
                     * write data to database
                     * ***********************************************/

                    System.IO.DirectoryInfo dir = new DirectoryInfo(Path.Combine(FileUtility.BaseFilePath, "ASX", DateTime.Now.ToString("yyyy-MM-dd")));
                    System.IO.FileInfo[] files = dir.GetFiles("*.htm");
                    HtmlDocument doc = new HtmlDocument();
                    DataAnalytic.Domain.Concrete.EFSecurityDailyTransactionRepository repo = new DataAnalytic.Domain.Concrete.EFSecurityDailyTransactionRepository();
                    foreach (var file in files)
                    {
                        doc.Load(file.OpenRead());
                        HtmlNode table = doc.DocumentNode.SelectSingleNode("//table[@class=\"datatable\"]");
                        if (table != null)
                        {
                            HtmlNodeCollection trs = table.SelectNodes("tr");
                            for (var i = 1; i < trs.Count; i++) //omit the first row
                            {
                                decimal value = 0;

                                HtmlNodeCollection tds = trs[i].SelectNodes("td");
                                SecurityDailyTransaction transaction = new SecurityDailyTransaction
                                {
                                    TransactionDate = DateTime.Now,
                                    SecurityCode = tds[0].InnerText.Trim(),
                                    LastPrice = decimal.TryParse(tds[1].InnerText.Trim(), out value) ? value : 0,
                                    ChangedPrice = decimal.TryParse(tds[2].InnerText.Trim(), out value) ? value : 0,
                                    ChangedPercentage = decimal.TryParse(tds[3].InnerText.Trim(), out value) ? value : 0,
                                    BidPrice = decimal.TryParse(tds[4].InnerText.Trim(), out value) ? value : 0,
                                    OfferPrice = decimal.TryParse(tds[5].InnerText.Trim(), out value) ? value : 0,
                                    OpenPrice = decimal.TryParse(tds[6].InnerText.Trim(), out value) ? value : 0,
                                    HighPrice = decimal.TryParse(tds[7].InnerText.Trim(), out value) ? value : 0,
                                    LowPrice = decimal.TryParse(tds[8].InnerText.Trim(), out value) ? value : 0,
                                    Volume = decimal.TryParse(tds[9].InnerText.Trim(), out value) ? value : 0,
                                    ModifiedDate = DateTime.Now,
                                    StatusCode = "",
                                    ModifiedByID = 0 //admin
                                };

                                //logWriter.Write(string.Format("Code {0}", transaction.SecurityCode));
                                //logWriter.Write(string.Format("Last {0}; $+/- {1}; %Chg {2}; Bid {3}; Offer {4}; Open {5}; High {6}; Low {7}; Volume {8}", transaction.LastPrice, transaction.ChangedPrice, transaction.ChangedPercentage, transaction.BidPrice, transaction.OfferPrice, transaction.OpenPrice, transaction.HighPrice, transaction.LowPrice, transaction.Volume));
                                repo.Append(transaction);
                            }
                        }
                    }
                    repo.SaveChange();
                }
                logWriter.Write(string.Format("Finish executing job {0} at {1}", context.JobDetail.Key, DateTime.Now.ToLongTimeString()));
            }
            catch (Exception ex)
            {
                logWriter.Write(string.Format("Error executing job {0} at {1}, error {2}", context.JobDetail.Key, DateTime.Now.ToLongTimeString(), ex.Message));
            }

        }
        #endregion

        #region private functions

        private void CreateTableHeader(HtmlNode table)
        {
            HtmlNode tr = HtmlNode.CreateNode("<tr>");
            table.AppendChild(tr);

            HtmlNode th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Code");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Last");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("$ +/-");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("% Chg");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Bid");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Offer");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Open");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("High");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Low");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Volume");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Options");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Warrants & Structured Products");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("CFDs");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Chart");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Status");

            th = HtmlNode.CreateNode("<th>");
            tr.AppendChild(th);
            th.InnerHtml = HtmlDocument.HtmlEncode("Announcements");
        }

        private void CreateTableDataRow(HtmlNode table, params string[] values)
        {
            HtmlNode tr = HtmlNode.CreateNode("<tr>");
            table.AppendChild(tr);

            HtmlNode td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[0]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[1]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[2]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[3]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[4]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[5]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[6]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[7]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[8]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[9]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[10]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[11]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[12]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[13]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[14]);

            td = HtmlNode.CreateNode("<td>");
            tr.AppendChild(td);
            td.InnerHtml = HtmlDocument.HtmlEncode(values[15]);
        }

        private void ConsolidateStockTradingData()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(FileUtility.BaseFilePath, "ASX", DateTime.Now.ToString("yyyy-MM-dd")));
            FileInfo[] files = dir.GetFiles("*.html");
            HtmlDocument document = new HtmlDocument();

            // create html document
            var html = HtmlNode.CreateNode("<html><head></head><body></body></html>");
            document.DocumentNode.AppendChild(html);

            // select the <body>
            HtmlNode body = document.DocumentNode.SelectSingleNode("/html/body");

            HtmlNode tableNode = HtmlNode.CreateNode("<table class=\"datatable\">");
            body.AppendChild(tableNode);

            CreateTableHeader(tableNode);

            HtmlDocument doc = new HtmlDocument();
            foreach (var file in files)
            {
                FileStream filestream = file.OpenRead();
                doc.Load(filestream);
                HtmlNode table = doc.DocumentNode.SelectSingleNode("//table[@class=\"datatable\"]");
                if (table != null)
                {
                    HtmlNodeCollection trs = table.SelectNodes("tr");
                    for (var i = 1; i < trs.Count; i++) //omit the first row
                    {
                        decimal value = 0;
                        List<string> values = new List<string>();

                        HtmlNode anchor = trs[i].SelectNodes("th//a").First();

                        HtmlNodeCollection tds = trs[i].SelectNodes("td");

                        values.Add(anchor.InnerText.Trim()); //Code
                        values.Add(decimal.TryParse(tds[0].InnerText.Trim(), out value) ? value.ToString() : "0"); //Last
                        values.Add(decimal.TryParse(tds[1].InnerText.Trim(), out value) ? value.ToString() : "0"); //$ +/-
                        values.Add((tds[2].InnerText.Trim().IndexOf('%') < 0) ? "0" : tds[2].InnerText.Trim().Substring(0, tds[2].InnerText.Trim().IndexOf('%'))); //% Chg
                        values.Add(decimal.TryParse(tds[3].InnerText.Trim(), out value) ? value.ToString() : "0"); //Bid
                        values.Add(decimal.TryParse(tds[4].InnerText.Trim(), out value) ? value.ToString() : "0"); //Offer
                        values.Add(decimal.TryParse(tds[5].InnerText.Trim(), out value) ? value.ToString() : "0"); //Open
                        values.Add(decimal.TryParse(tds[6].InnerText.Trim(), out value) ? value.ToString() : "0"); // High
                        values.Add(decimal.TryParse(tds[7].InnerText.Trim(), out value) ? value.ToString() : "0"); // Low
                        values.Add(decimal.TryParse(tds[8].InnerText.Trim(), out value) ? value.ToString() : "0"); // Volume
                        values.Add(" "); // Options
                        values.Add(" "); // Warrants & Structured Products
                        values.Add(" "); // CFDs
                        values.Add(" "); // Chart
                        values.Add(" "); // Status
                        values.Add(" "); // Announcements

                        CreateTableDataRow(tableNode, values.ToArray<string>());
                        //logWriter.Write(string.Format("Code {0}", transaction.SecurityCode));
                        //logWriter.Write(string.Format("Last {0}; $+/- {1}; %Chg {2}; Bid {3}; Offer {4}; Open {5}; High {6}; Low {7}; Volume {8}", transaction.LastPrice, transaction.ChangedPrice, transaction.ChangedPercentage, transaction.BidPrice, transaction.OfferPrice, transaction.OpenPrice, transaction.HighPrice, transaction.LowPrice, transaction.Volume));

                    }
                }
                filestream.Close();
                file.Delete();
            }
            Stream fs = new FileStream(Path.Combine(dir.FullName, string.Format("ASX-{0}.htm", DateTime.Today.ToString("yyyy-MM-dd"))), FileMode.Create, FileAccess.Write);
            document.Save(fs);
            fs.Close();
        }

        private void UploadFile()
        {
            FtpWebRequest request;
            FtpWebResponse response;

            try
            {
                request = (FtpWebRequest)WebRequest.Create(string.Format(DataAnalytic.WebUI.Utility.AppConfiguration.FtpFolder, DateTime.Today.ToString("yyyy-MM-dd")));
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                response = (FtpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                logWriter.Write(string.Format("create directory error {0}", ex.Message));
            }

            /**************************
             * upload file
             * ***********************/
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(FileUtility.BaseFilePath, "ASX", DateTime.Now.ToString("yyyy-MM-dd")));
            FileInfo[] files = dir.GetFiles("*.htm");
            foreach (var file in files)
            {
                request = (FtpWebRequest)WebRequest.Create(string.Format(string.Concat(DataAnalytic.WebUI.Utility.AppConfiguration.FtpFolder,"/{1}"), DateTime.Today.ToString("yyyy-MM-dd"), file.Name));
                request.Method = WebRequestMethods.Ftp.UploadFile;
                // request.EnableSsl = true; //not supported
                request.ContentLength = file.Length;
                byte[] buffer = new byte[4097];
                int bytes_read = 0;
                FileStream fs = file.OpenRead();
                Stream rs = request.GetRequestStream();
                do
                {
                    bytes_read = fs.Read(buffer, 0, 4097);
                    rs.Write(buffer, 0, bytes_read);
                } while (bytes_read > 0);
                fs.Close();
                rs.Close();
                response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
        }
        #endregion
    }
}