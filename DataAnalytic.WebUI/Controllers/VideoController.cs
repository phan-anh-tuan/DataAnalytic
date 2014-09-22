using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAnalytic.Domain.Abstract;
using DataAnalytic.Domain.Entities;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using DataAnalytic.WebUI.Utility;
using DataAnalytic.WebUI.Utility.Logging;
using DataAnalytic.WebUI.Utility.File;
using DataAnalytic.WebUI.Models;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Mvc;
using DataAnalytic.WebUI.Infrastructure;

namespace DataAnalytic.WebUI.Controllers
{
    public class VideoController : Controller
    {
        private IObjectRepository<Video> repository;
        private LogWriter logWriter = LoggingUtility.LogWriter;
        public int PageSize = AppConfiguration.PageSize;

        public VideoController(IObjectRepository<Video> videoRepository)
        {
            repository = videoRepository;
         }

        //
        // GET: /Video/
        
        public async Task<ActionResult> Index(string selectedTvChannel, int page = 1)
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(System.Threading.CancellationToken.None);

            if (result.Credential != null)
            {
                VideosListViewModel model = new VideosListViewModel()
                {
                    Videos = repository.GetDataSet.Where<Video>(v => string.IsNullOrEmpty(selectedTvChannel) || v.TvChannel == selectedTvChannel)
                            .OrderBy(v => v.URL)
                            .Skip((page - 1) * PageSize)
                            .Take(PageSize),
                    PagingInfo = new PagingInfo()
                    {
                        CurrentPage = page,
                        TotalItems = repository.GetDataSet.Where<Video>(v => string.IsNullOrEmpty(selectedTvChannel) || v.TvChannel == selectedTvChannel).Count(),
                        ItemsPerPage = PageSize
                    },
                    CurrentTvChannel = selectedTvChannel
                };

                //return View(repository.GetDataSet);
                if (Request.IsAjaxRequest())
                {
                    return Json(model, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View(model);
                }
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
        }

        public PartialViewResult GetVideos(string tvChannel = "All")
        {
            if (tvChannel.ToUpper().Equals("ALL"))
            {
                return PartialView(repository.GetDataSet);
            }
            else
            {
                return PartialView(repository.GetDataSet.Where<Video>(v => v.TvChannel == tvChannel));
            }
        }

        public PartialViewResult GetTvChannels(string selectedTvChannel = null)
        {
            ViewBag.SelectedTvChannel = selectedTvChannel;

            IEnumerable<string> tvChannels = repository.GetDataSet.Select(v => v.TvChannel).Distinct().OrderBy(x => x);
            return PartialView(tvChannels);
        }

        public string DownloadVideo(int ID)
        {
            try
            {

                Video video = repository.Find(ID);
                if (video != null && !video.IsAvailable)
                {
                    Uri uri = new Uri(video.URL);
                    string hostname = uri.Host;
                    string parentFolder = System.IO.Path.Combine(FileUtility.BaseFilePath, hostname);
                    if (!Directory.Exists(parentFolder))
                    {
                        Directory.CreateDirectory(parentFolder);
                    }
                    string childFolder = System.IO.Path.Combine(parentFolder, video.TvChannel); 
                    if (!Directory.Exists(childFolder))
                    {
                        Directory.CreateDirectory(childFolder);
                    }
                    string videoName = uri.PathAndQuery.Substring(uri.PathAndQuery.LastIndexOf("/") + 1);
                    FileUtility.DownloadFile(video.URL,string.Concat(childFolder,"/",videoName),true);
                    video.IsAvailable = true;
                    repository.SaveObject(video);
                    return "Downloaded";
                }
                else
                {
                    return "The video is no longer available in database";
                }
            }
            catch (Exception ex)
            {
                logWriter.Write(ex.Message);
                return "Error !!!";
            }
        }

        public ViewResult Edit(int videoId)
        {
            Video video = repository.GetDataSet.FirstOrDefault(v => v.ID == videoId);
            return View(video);
        }

        [HttpPost]
        public ActionResult Edit(Video video)
        {
            if (ModelState.IsValid)
            {
                repository.SaveObject(video);
                //TempData["message"] = string.Format("{0} has been saved", product.Name);
                if (Request.IsAjaxRequest())
                {
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                // there is something wrong with the data values
                return View(video);
            }
        }
    }
}
