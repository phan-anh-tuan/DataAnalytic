using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using Quartz;
using Quartz.Job;
using Quartz.Impl;
using DataAnalytic.WebUI.Utility.Logging;
using DataAnalytic.WebUI.Utility.File;
using DataAnalytic.Domain.Entities;
using DataAnalytic.Domain.Concrete;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using HtmlAgilityPack;

namespace DataAnalytic.WebUI.Business.Concrete
{
    public class REAuctionResultDownloader : IJob
    {
        private LogWriter logWriter = LoggingUtility.LogWriter;

        #region private functions
        private void Paging(DateTime today)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(FileUtility.BaseFilePath, "homepriceguide", today.ToString("yyyy-MM-dd")));
            FileInfo[] files = dir.GetFiles("*.pdf");

            foreach (var file in files)
            {
                if (!Directory.Exists(Path.Combine(file.DirectoryName, "paging"))) Directory.CreateDirectory(Path.Combine(file.DirectoryName, "paging"));

                PdfReader reader = new PdfReader(file.FullName);
                Document document = null;
                PdfCopy pdfCopyProvider = null;
                PdfImportedPage importedPage = null;

                if (reader.NumberOfPages > 3)
                {

                    for (int i = 0; i < reader.NumberOfPages; i++)
                    {
                        if (i % 3 == 0)
                        {
                            // Capture the correct size and orientation for the page:
                            document = new Document(reader.GetPageSizeWithRotation(i + 1));
                            string outputPdfPath = Path.Combine(file.DirectoryName, "paging", string.Concat(file.Name.Substring(0, file.Name.IndexOf(".pdf")), "_", (i + 1), ".pdf"));
                            //string.Concat(file.FullName.Substring(0, file.FullName.IndexOf(".pdf")), "_", (i + 1), ".pdf");
                            // Initialize an instance of the PdfCopyClass with the source 
                            // document and an output file stream:
                            pdfCopyProvider = new PdfCopy(document, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));
                            document.Open();
                        }
                        // Extract the desired page number:
                        importedPage = pdfCopyProvider.GetImportedPage(reader, i + 1);
                        pdfCopyProvider.AddPage(importedPage);
                        if (i % 3 == 2 || i == reader.NumberOfPages - 1)
                        {
                            document.Close();
                        }
                    }
                }
                else
                {
                    file.CopyTo(Path.Combine(file.DirectoryName, "paging", file.Name));
                }
                reader.Close();
            }
        }

        private void Transforming(DateTime today)
        {
            System.IO.DirectoryInfo dir = new DirectoryInfo(Path.Combine(FileUtility.BaseFilePath, "homepriceguide", today.ToString("yyyy-MM-dd"), "paging"));
            System.IO.FileInfo[] files = dir.GetFiles("*.pdf");

            foreach (var file in files)
            {
                string pathToPdf = file.FullName;
                string pathToHtml = Path.ChangeExtension(pathToPdf, ".htm");

                // Convert PDF file to HTML file
                SautinSoft.PdfFocus f = new SautinSoft.PdfFocus();
                // Let's force the component to store images inside HTML document
                // using base-64 encoding
                //f.HtmlOptions.IncludeImageInHtml = true;
                f.HtmlOptions.Title = "Simple text";

                // This property is necessary only for registered version
                //f.Serial = "XXXXXXXXXXX";

                f.OpenPdf(pathToPdf);

                if (f.PageCount > 0)
                {
                    int result = f.ToHtml(pathToHtml);

                    //Show HTML document in browser
                    //if (result == 0)
                    //{
                    //    System.Diagnostics.Process.Start(pathToHtml);
                    //}
                }
                f.ClosePdf();
                file.Delete();
            }
        }

        private void PersistingHtmlFiles(DateTime today)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(FileUtility.BaseFilePath, "homepriceguide", today.ToString("yyyy-MM-dd"), "paging"));
            FileInfo[] files = dir.GetFiles("*.html");
            HtmlDocument doc = new HtmlDocument();
            AuctionResult auctionResult = null;
            DataAnalytic.Domain.Concrete.EFAuctionResultRepository auctionResultRepository = new DataAnalytic.Domain.Concrete.EFAuctionResultRepository();
            foreach (var file in files)
            {
                logWriter.Write(string.Format("processing file {0}", file.FullName));
                FileStream fs = file.OpenRead();
                doc.Load(fs);
                HtmlNodeCollection trs = doc.DocumentNode.SelectNodes("//table/tr");
                for (int i = 1; i < trs.Count; i++)
                {

                    HtmlNode tr = trs[i];
                    HtmlNodeCollection childnodes = tr.ChildNodes;
                    List<HtmlNode> tds = childnodes.Where<HtmlNode>(p => p.Name == "td").ToList<HtmlNode>();
                    int noOfBedrooms = -1;
                    string type = "house";

                    if (tds.Count() > 5)
                    {
                        if (!string.IsNullOrWhiteSpace(System.Web.HttpUtility.HtmlDecode(tds[0].InnerText.Trim())))
                        {
                            string[] types = tds[2].InnerText.Trim().Split(' ');
                            foreach (var val in types)
                            {

                                if (noOfBedrooms == -1 && !int.TryParse(val, out noOfBedrooms))
                                {
                                    noOfBedrooms = -1;
                                }
                                else
                                {
                                    switch (val)
                                    {
                                        case "t":
                                            type = "town house";
                                            break;
                                        case "u":
                                            type = "unit";
                                            break;
                                        case "h":
                                            type = "house";
                                            break;
                                    }
                                }
                            }

                            decimal value;
                            string text = tds[3].InnerText.Trim().StartsWith("$") ? tds[3].InnerText.Trim().Substring(1) : tds[3].InnerText.Trim();
                            if (!decimal.TryParse(text, out value)) value = -1;


                            auctionResult = new AuctionResult
                            {
                                City = "Sydney",
                                Suburb = System.Web.HttpUtility.HtmlDecode(tds[0].InnerText.Trim()),
                                TransactionDate = today,
                                UpdatedDate = today,
                                Address = System.Web.HttpUtility.HtmlDecode(tds[1].InnerText.Trim()),
                                NoOfBedroom = noOfBedrooms,
                                Type = type,
                                Price = value,
                                Result = System.Web.HttpUtility.HtmlDecode(tds[4].InnerText.Trim()),
                                Agent = System.Web.HttpUtility.HtmlDecode(tds[5].InnerText.Trim())
                            };
                            logWriter.Write(string.Format("property at address {0}", auctionResult.Address));
                            auctionResultRepository.Append(auctionResult);
                        }
                    } //end if tds.count > 0
                } // end for int i = 1
                auctionResultRepository.SaveChange();
                fs.Close();
                file.Delete();
            } // end foreach
        }

        private string getPropertyType(string val)
        {
            switch (val)
            {
                case "t":
                    return "town house";
                case "u":
                    return "unit";
                case "h":
                    return "house";
                case "studio":
                    return "studio";
                default:
                    return "";
            }
        }
        private void PersistingHtmFiles(DateTime today)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(FileUtility.BaseFilePath, "homepriceguide", today.ToString("yyyy-MM-dd"), "paging"));
            //DirectoryInfo dir = new DirectoryInfo(Path.Combine(FileUtility.BaseFilePath, "homepriceguide", "2014-08-28", "paging"));
            FileInfo[] files = dir.GetFiles("*.htm");

            HtmlDocument doc = new HtmlDocument();
            AuctionResult auctionResult = null;
            DataAnalytic.Domain.Concrete.EFAuctionResultRepository auctionResultRepository = new DataAnalytic.Domain.Concrete.EFAuctionResultRepository();
            
            #region foreach file
            foreach (var file in files)
            {
                int counter = -1;
                logWriter.Write(string.Format("processing file {0}", file.FullName));
                string city = (file.Name.IndexOf("_") > 0) ? file.Name.Substring(0, file.Name.IndexOf("_")) : file.Name.Substring(0, file.Name.IndexOf(".htm"));
                FileStream fs = file.OpenRead();
                doc.Load(fs);
                HtmlNodeCollection spans = doc.DocumentNode.SelectNodes("//div/span[@class=\"cls_5\"]");
                Match match = Regex.Match(file.Name, "_\\d+");
                if (match.Success && int.Parse(match.Value.Substring(1)) > 1)
                {
                    spans = doc.DocumentNode.SelectNodes("//div/span[@class=\"cls_4\"]");
                }
                int previousElement_yCoordinator = 0;
                #region foreach span
                foreach (var span in spans)
                {
                    if (span.NextSibling != null)
                    {
                        //bypass the first element
                    }
                    else
                    {
                        //validitem_count++;
                        //logWriter.Write(string.Format("processing span {0}",span.InnerText.Trim()));
                        HtmlNode div = span.ParentNode;
                        if (div != null && div.Attributes.Contains("style"))
                        {
                            string[] css = div.Attributes["style"].Value.Split(';');
                            foreach (var style in css)
                            {
                                if (style.StartsWith("left"))
                                {
                                    string leftValue = style.Split(':')[1];
                                    int yCoordinator = int.Parse(leftValue.Substring(0, leftValue.Length - 2));
                                    counter++;
                                    if (yCoordinator == previousElement_yCoordinator)
                                    {
                                        //update Agent property of the previous entity
                                        counter--;
                                        if (counter % 6 == 5 && auctionResult != null)
                                        {
                                            auctionResult.Agent += " " + span.InnerText.Trim();
                                        }
                                    }
                                    else
                                    {
                                        switch (counter % 6)
                                        {
                                            case 0: // suburb
                                                auctionResult = new AuctionResult { City = city, Suburb = span.InnerText.Trim(), TransactionDate = today, UpdatedDate = today };
                                                break;
                                            case 1: // address
                                                if (auctionResult != null)
                                                {
                                                    auctionResult.Address = span.InnerText.Trim();
                                                    logWriter.Write(string.Format("processing property at {0}", auctionResult.Address));
                                                    if (span.Descendants("span").Count<HtmlNode>() > 0)
                                                    {
                                                        counter++;
                                                        List<HtmlNode> descendents = span.Descendants().ToList<HtmlNode>();
                                                        
                                                        foreach (var node in descendents)
                                                        {
                                                            if (node.Name.ToUpper().Equals("SPAN"))
                                                            {
                                                                auctionResult.NoOfBedroom = int.Parse(node.InnerText.Trim());
                                                                string[] propertyType = descendents[descendents.Count - 1].InnerText.Trim().Split(' ');
                                                                auctionResult.Type = getPropertyType(propertyType[propertyType.Length - 1]);
                                                            }
                                                        }
                                                    }
                                                }
                                                break;
                                            case 2: //type
                                                if (auctionResult != null)
                                                {
                                                    string[] types = span.InnerText.Trim().Split(' ');
                                                    //if (types.Length > 2)
                                                    //{
                                                    //    switch (types[2])
                                                    //    {
                                                    //        case "t":
                                                    //            auctionResult.Type = "town house";
                                                    //            break;
                                                    //        case "u":
                                                    //            auctionResult.Type = "unit";
                                                    //            break;
                                                    //        case "h":
                                                    //            auctionResult.Type = "house";
                                                    //            break;
                                                    //    }
                                                    //    auctionResult.NoOfBedroom = int.Parse(types[0]);
                                                    //}
                                                    //else
                                                    //{
                                                    foreach (var val in types)
                                                    {
                                                        int noOfBedrooms;
                                                        if (int.TryParse(val, out noOfBedrooms))
                                                        {
                                                            auctionResult.NoOfBedroom = noOfBedrooms;
                                                        }
                                                        else
                                                        {

                                                            auctionResult.Type = getPropertyType(val);
                                                          
                                                        }
                                                    }
                                                    if (auctionResult.Type == null)
                                                    {
                                                        auctionResult.Type = "house";
                                                    }
                                                    //}
                                                }
                                                break;
                                            case 3: //price
                                                if (auctionResult != null)
                                                {
                                                    decimal value;
                                                    string text = span.InnerText.Trim().StartsWith("$") ? span.InnerText.Trim().Substring(1) : span.InnerText.Trim();
                                                    if (decimal.TryParse(text, out value)) auctionResult.Price = value;
                                                }
                                                break;
                                            case 4: //result
                                                if (auctionResult != null)
                                                {
                                                    auctionResult.Result = span.InnerText.Trim();
                                                }
                                                break;
                                            case 5: //agent
                                                if (auctionResult != null)
                                                {
                                                    auctionResult.Agent = span.InnerText.Trim();
                                                    //logWriter.Write(auctionResult);
                                                    auctionResultRepository.Append(auctionResult);
                                                    //auctionResultRepository.AddOrUpdate(auctionResult);
                                                }
                                                break;
                                        }
                                    }
                                    previousElement_yCoordinator = yCoordinator;
                                }
                            }
                        }
                    }   
                }
                #endregion foreach span
                auctionResultRepository.SaveChange();
                fs.Close();
                file.Delete();
            }
            #endregion foreach file
        }
        #endregion
        public void Execute(IJobExecutionContext context)
        {

            try
            {
                logWriter.Write(string.Format("start executing job {0} at {1}", context.JobDetail.Key, DateTime.Now.ToLongTimeString()));


                if (context.MergedJobDataMap.Keys.Contains("UrlPattern") && context.MergedJobDataMap.Keys.Contains("cities"))
                {
                    DateTime day = DateTime.Today.AddDays(-1 * ((int)DateTime.Today.DayOfWeek + 1));
                    string folder = FileUtility.CreateSubFolders(new string[] { "HomePriceGuide", day.ToString("yyyy-MM-dd") });
                    string urlPattern = context.MergedJobDataMap.GetString("UrlPattern");
                    string[] cities = context.MergedJobDataMap.GetString("cities").Split(',');
                    foreach (var city in cities)
                    {
                        String url = string.Format(urlPattern, string.Concat(city, ".pdf"));
                        FileUtility.DownloadFile(url, Path.Combine(folder, string.Concat(city, ".pdf")), true);
                    }

                    Paging(day);

                    Transforming(day);
                }
                else
                {
                    PersistingHtmFiles(DateTime.Today);
                }

                logWriter.Write(string.Format("Finish executing job {0} at {1}", context.JobDetail.Key, DateTime.Now.ToLongTimeString()));
            }
            catch (Exception ex)
            {
                logWriter.Write(string.Format("Error executing job {0} at {1}, error {2}", context.JobDetail.Key, DateTime.Now.ToLongTimeString(), ex.Message));
            }

        }

        public void Persisting(DateTime today)
        {
            string containerFolder = today.ToString("yyyy-MM-dd");
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(DataAnalytic.WebUI.Utility.File.FileUtility.BaseFilePath, "HomePriceGuide", containerFolder, "paging"));
            FileInfo[] files = dir.GetFiles("*.html");
            if (files.Length > 0)
            {
                PersistingHtmlFiles(today);
            }
            else
            {
                files = dir.GetFiles("*.htm");
                if (files.Length > 0)
                {
                    PersistingHtmFiles(today);
                }
            }
        }
    }
}
