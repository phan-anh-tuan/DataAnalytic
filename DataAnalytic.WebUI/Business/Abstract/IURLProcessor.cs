using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAnalytic.WebUI.Business.Abstract
{
    public interface IURLProcessor
    {
        void Process(string url);
        string BuildFilePath(string url);
        string ToString();
    }
}
