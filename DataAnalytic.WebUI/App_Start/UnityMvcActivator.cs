using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.Unity.Mvc;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(DataAnalytic.WebUI.App_Start.UnityWebActivator), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(DataAnalytic.WebUI.App_Start.UnityWebActivator), "Shutdown")]

namespace DataAnalytic.WebUI.App_Start
{
    /// <summary>Provides the bootstrapping for integrating Unity with ASP.NET MVC.</summary>
    public static class UnityWebActivator
    {
        /// <summary>Integrates Unity when the application starts.</summary>
        public static void Start() 
        {
            IUnityContainer container = UnityConfig.GetConfiguredContainer();

            /************************************************************
             * custom code to register an instance of flat file logger
             * 
             ************************************************************/
            LogWriterFactory logWriterFactory = new LogWriterFactory();
            LogWriter logWriter = logWriterFactory.Create();
            
            container.RegisterInstance<LogWriter>("FlatFileLogger", logWriter);


            FilterProviders.Providers.Remove(FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().First());
            FilterProviders.Providers.Add(new UnityFilterAttributeFilterProvider(container));

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            // TODO: Uncomment if you want to use PerRequestLifetimeManager
            // Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(UnityPerRequestHttpModule));
        }

        /// <summary>Disposes the Unity container when the application is shut down.</summary>
        public static void Shutdown()
        {
            var container = UnityConfig.GetConfiguredContainer();
            container.Dispose();
        }
    }
}