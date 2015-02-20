using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Quartz;
using Quartz.Impl;
using log4net;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Log4NetConfig.xml", Watch = true)]

namespace SourceVsDeployed
{
    /// <summary>
    /// Main application thread.  As is common with Console apps, "static Main()" is the entry point.  
    /// </summary>
    public class Program
    {

        // Variables used to count attempts to retreive and process remote files.
        public static int CountChecked = 0;
        public static int CountOk = 0;
        public static int CountCorrupt = 0;
        public static int CountMissing = 0;
        public static int CountOtherError = 0;        
        
        /// <summary>
        /// Entry point.  See "CLI.DisplayHelp" for rough documentation.
        /// </summary>
        /// <param name="args">Arguments from command line</param>
        public static int Main(string[] args)
        {
            // Parse arguments 
            Arguments.Populate(args);
            if (!Arguments.ReadyToRun)
            {
                return Arguments.ReturnCode;
            }

            // Set up manifest object
            var manifest = new Manifest(Arguments.Target);

            // If the manifest is remote, pull it down and parse it
            if (Arguments.ManifestIsAURL)
            {
                string sLocalManifestPath = manifest.DownloadManifest(Arguments.Manifest);
                if (sLocalManifestPath.Length == 0)
                {
                    Log.Error("Could not access manifest using provided URL " + Arguments.Manifest + "!");
                    return 1;
                }
                // Validate and parse manifest file
                if(!manifest.ValidateAndParseDownloadedManifest(sLocalManifestPath))
                {
                    Log.Error("Could not validate or parse manifest file " + sLocalManifestPath + " after downloading!");
                    return 1;
                }

            }
            else
            {
                // Else, this is a local manifest file.  Parse it and proceed.
                if (!manifest.ParseFile(Arguments.Manifest))
                {
                    Log.Error("Could not validate or parse local manifest " + Arguments.Manifest + "!");
                    return 1;
                }                            
            }

            // If we were only told to parse the manifest file, we can quit now
            if (Arguments.Action == "parse")
            {
                Log.Info("Manifest file parsed OK!");
                return 0;
            }

            // Make sure temporary work folder exists
            if (!manifest.TemporaryFolderHasBeenCreatedOrExists())
            {
                return 1;   // Error text comes from manifest
            }
            
            // Set maximum number of download threads
            // ReSharper disable once CSharpWarnings::CS0618
            int threadCount = Int32.Parse(ConfigurationManager.AppSettings["Threads"]);
            Log.Info("Using up to " + threadCount + " threads to download and test remote content.");

            // Create the scheduler factory and a factory instance
            // Enforce a maximum number of threads and force them to run in the background
            var props = new NameValueCollection();
            props["quartz.threadPool.makeThreadsDaemons"] = "true";
            props["quartz.scheduler.makeSchedulerThreadDaemon"] = "true";
            props["quartz.threadPool.threadCount"] = threadCount.ToString(CultureInfo.InvariantCulture);
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory(props);
            IScheduler scheduler = schedulerFactory.GetScheduler();

            // Add in a listener to pick up job results as they come in
            var globalSummaryBuilder = new JobListener_SummaryBuilder();
            globalSummaryBuilder.JobExecutedHandler += HandleGlobalJobEvents;
            scheduler.ListenerManager.AddJobListener(globalSummaryBuilder);

            // Start the scheduler so that it can start executing jobs
            scheduler.Start();

            // Fire up an initial group of tasks
            int iJob;
            for (iJob = 0; iJob < Utility.LesserOf(threadCount, manifest.LinesParsed); iJob++)
            {
                var ssb = SimpleScheduleBuilder.Create();
                var jobkey = new JobKey("Job_" + iJob, "JobGroup_CheckWebFile");
                var ma = manifest.GetEntry(iJob);
                IJobDetail job = JobBuilder.Create(typeof(Job_CheckWebFile)).WithIdentity(jobkey).UsingJobData("URL", Arguments.Target + ma.RelativePath).UsingJobData("Hash", ma.Md5).UsingJobData("Expectation", ma.Expectation).UsingJobData("JobNumber", iJob.ToString(CultureInfo.InvariantCulture)).Build();
                ITrigger trigger = TriggerBuilder.Create().WithSchedule(ssb.WithRepeatCount(0)).StartNow().WithIdentity("Trigger_" + iJob, "Trigger_CheckWebFile").Build();

                // Use a section like this to implement a local listener
                // Code from http://stackoverflow.com/questions/14517563/quartz-schedulers-job-fired-but-joblistener-not-picking-up-event?rq=1
                //var localSummaryBuilder = new JobListener_SummaryBuilder();
                //localSummaryBuilder.JobExecutedHandler += HandleLocalJobEvents;
                //IMatcher<JobKey> matcher = KeyMatcher<JobKey>.KeyEquals(jobkey);
                //scheduler.ListenerManager.AddJobListener(localSummaryBuilder, matcher);

                // Schedule the job (and start since we're using an immediate trigger)
                scheduler.ScheduleJob(job, trigger);
                Log.Debug("Scheduled task " + iJob + "!");

            }
            Log.Debug("Done scheduling initial set of jobs. Waiting for more threads to open up if we need them.");

            // As these tasks complete, add more tasks to the mix
            while (iJob < manifest.LinesParsed)
            {
                if (scheduler.GetCurrentlyExecutingJobs().Count < threadCount)
                {
                    Log.Debug("...scheduler has room (at " + scheduler.GetCurrentlyExecutingJobs().Count + "), adding job #" + iJob + " now.");
                    var ssb = SimpleScheduleBuilder.Create();
                    var ma = manifest.GetEntry(iJob);
                    IJobDetail job = JobBuilder.Create(typeof(Job_CheckWebFile)).WithIdentity("Job_" + iJob, "Job_CheckWebFile").UsingJobData("URL", Arguments.Target + ma.RelativePath).UsingJobData("Hash", ma.Md5).UsingJobData("Expectation", ma.Expectation).UsingJobData("JobNumber", iJob.ToString(CultureInfo.InvariantCulture)).Build();
                    ITrigger trigger = TriggerBuilder.Create().WithSchedule(ssb.WithRepeatCount(0)).StartNow().WithIdentity("Trigger_" + iJob, "Trigger_CheckWebFile").Build();
                    scheduler.ScheduleJob(job, trigger);
                    iJob++;
                }
                Thread.Sleep(20);

            }
            Log.Debug("Done scheduling all jobs.");

            // Now wait until all threads are complete
            Thread.Sleep(5000);  // Arbitrary value used to give at least one job time to start if we have more threads than URLs
            Log.Debug("All jobs have been started.  Now waiting for all jobs to complete.");
            while (scheduler.GetCurrentlyExecutingJobs().Count > 0)
            {
                Thread.Sleep(500);
                Log.Debug("...waiting for " + scheduler.GetCurrentlyExecutingJobs().Count + " jobs to complete.");
            }

            //A nice way to stop the scheduler, waiting for jobs that are running to finish
            scheduler.Shutdown(true);

            // If necessary, rename the temp folder
            manifest.RetainCorruptFolder(CountCorrupt);

            // Display a final status message and set a return code appropriately  
            Log.Info("Checked " + CountChecked +
                     " URLs. OK: " + CountOk +
                     ", Corrupt: " + CountCorrupt +
                     ", Missing: " + CountMissing +
                     ", Other Error: " + CountOtherError
                );
            if ((CountCorrupt + CountMissing + CountOtherError) == 0)
            {
                Log.Info("MANIFEST VALIDATED.");
                return 0;
            }
            Log.Error("MANIFEST FAILED VALIDATION.");
            return 1;
        }

        /// <summary>
        /// Used to report on job completions in near real time.  As each thread completes, it will call back to this.
        /// </summary>
        public static void HandleGlobalJobEvents(object sender, EventArgs e)
        {
            var thisLock = new Object();
            lock (thisLock)
            {
                // Log.Debug("HandleGlobalJobEvents(" + sender + "," + e + ")");
                if (e != null)
                {
                    var eventArgsJobListener = (EventArgs_JobListener)e;
                    int code = Int32.Parse(eventArgsJobListener.Code);
                    string[] message = eventArgsJobListener.Message.Split('|');
                    CountChecked++;
                    if (code == 0)
                    {
                        Log.Debug(message[1] + " - OK.");
                        CountOk++;
                    }
                    else if (code == 1)
                    {
                        Log.Warn(message[1] + " - Corrupt!");
                        CountCorrupt++;
                    }
                    else if (code == 2)
                    {
                        Log.Warn(message[1] + " - Missing!");
                        CountMissing++;
                    }
                    else
                    {
                        Log.Warn(message[1] + " - OTHER ERROR! (see log)");
                        CountOtherError++;
                    }
                }
            }

            // Sender should be of type QuartzWebFileChecker.JobListener_SummaryBuilder            
        }

        /// <summary>
        /// Recommended per-class reference to log4net (http://www.codeproject.com/Articles/140911/log4net-Tutorial)
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    }
}
