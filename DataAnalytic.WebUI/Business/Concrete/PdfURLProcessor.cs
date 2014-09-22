using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAnalytic.WebUI.Business.Abstract;
using System.Net;
using System.IO;

namespace DataAnalytic.WebUI.Business.Concrete
{
    public class PdfURLProcessor : BaseURLProcessor
    {
        public override void Process(string url)
        {
    
        }

        public override string ToString()
        {
            return "Greating from Pdf File Processor";
        }
    }
}