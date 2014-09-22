using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using DataAnalytic.WebUI.App_Start;

namespace DataAnalytic.WebUI.Utility.Logging
{
    public class LoggingUtility
    {
        private static IUnityContainer container = UnityConfig.GetConfiguredContainer();
        public static LogWriter LogWriter = container.Resolve<LogWriter>("FlatFileLogger");
    }
}