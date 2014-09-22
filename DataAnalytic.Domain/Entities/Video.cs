using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DataAnalytic.Domain.Entities
{
    public class Video
    {
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }

        [Required(ErrorMessage = "Please enter tv channel")]
        [MaxLength(256)]
        public string TvChannel { get; set; }

        [Required(ErrorMessage = "Please enter video URL")]
        [MaxLength(256)]
        public string URL { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }
        
        //comma delimited list of tags
        public string Tags { get; set; }

        public string YouTubeId { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
