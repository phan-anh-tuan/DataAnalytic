using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DataAnalytic.Domain.Entities
{
    public class DataSource
    {
        [HiddenInput(DisplayValue=false)]
        public int ID {get; set;}
        
        [Required(ErrorMessage="Please enter data source URL")]
        [MaxLength(256)]
        public string URL { get; set; }

        [Required(ErrorMessage = "Please enter data source type")]
        [MaxLength(256)]
        public string DataSourceType { get; set; }

        public bool IsActive { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
