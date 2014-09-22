using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;
using Quartz.Util;


[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(DataAnalytic.WebUI.App_Start.QuartzWebActivator), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(DataAnalytic.WebUI.App_Start.QuartzWebActivator), "Shutdown")]
namespace DataAnalytic.WebUI.App_Start
{
    public static class QuartzWebActivator
    {
        public static void Start()
        {
            var properties = new NameValueCollection();
            properties["quartz.plugin.triggHistory.type"] = "Quartz.Plugin.History.LoggingJobHistoryPlugin";

            properties["quartz.plugin.jobInitializer.type"] = "Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin";
            properties["quartz.plugin.jobInitializer.fileNames"] = "~/quartzjobs.xml";
            properties["quartz.plugin.jobInitializer.failOnFileNotFound"] = "true";
            properties["quartz.plugin.jobInitializer.scanInterval"] = "120";

            // construct a scheduler factory
            ISchedulerFactory schedFact = new StdSchedulerFactory(properties);

            // get a scheduler
            IScheduler sched = schedFact.GetScheduler();
            sched.Start();

            IList<string> jobgroups = sched.GetJobGroupNames();
            

            //// define the job and tie it to our HelloJob class
            //IJobDetail job = JobBuilder.Create<DataAnalytic.WebUI.Business.Concrete.HelloJob>()
            //    .WithIdentity("myJob", "group1")
            //    .Build();

            //// Trigger the job to run now, and then every 40 seconds
            //ITrigger trigger = TriggerBuilder.Create()
            //  .WithIdentity("myTrigger", "group1")
            //  .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Monday,15,0))
            //  //.StartNow()
            //  //.WithSimpleSchedule(x => x
            //  //    .WithIntervalInSeconds(10)
            //  //    .RepeatForever())
            //  .Build();

            //sched.ScheduleJob(job, trigger);
        }
    }
}