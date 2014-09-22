using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAnalytic.Domain.Entities;

namespace DataAnalytic.WebUI.Models
{
    public class VideosListViewModel
    {
        public IEnumerable<Video> Videos { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public string CurrentTvChannel { get; set; }
    }
}