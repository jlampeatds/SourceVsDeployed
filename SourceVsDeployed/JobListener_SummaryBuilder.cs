using System;
using System.ComponentModel;
using Quartz;


namespace SourceVsDeployed
{
    // ReSharper disable once InconsistentNaming
    public class JobListener_SummaryBuilder : IJobListener
    {

        public event EventHandler JobExecutedHandler;
        public readonly Guid Id = Guid.NewGuid();

        /// <summary>
        /// Required by interface.  Randomized with GUID upon advise of the Internet.
        /// </summary>
        public string Name
        {
            get { return "JobListener_SummaryBuilder_" + Id; }
        }

        /// <summary>
        /// Required but empty
        /// </summary>
        /// <param name="context"></param>
        public void JobExecutionVetoed(
            IJobExecutionContext context
            )
        {
            // We don't care about these in this program
            // Log.Debug(Name + " JobExecutionVetoed: " + context);
        }

        /// <summary>
        /// Required but empty
        /// </summary>
        /// <param name="context"></param>
        public void JobToBeExecuted(
            IJobExecutionContext context
            )
        {
            // We don't care about these in this program
            // Log.Debug(Name + " JobToBeExecuted: " + context);
        }

        /// <summary>
        /// Called AFTER a job is executed.  We inspect what was provided here to see
        /// whether the job was successful or not.  
        /// NOTE: I used an exception-based HACK to get this to work after it looked like 
        /// Quartz would never raise this event otherwise! 
        /// </summary>
        /// <param name="context">the context</param>
        /// <param name="jobException">a jobException, should have an inner.inner Exception_JobComplete</param>
        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            // If it's the exception we threw to raise the "WasExecuted" event, parse it and ignore it!
            if (jobException.ToString().Contains("Exception_JobComplete"))
            {
                var jobComplete = (Exception_JobComplete)jobException.InnerException.InnerException;
                OnJobExecuted(new EventArgs_JobListener(jobComplete.Code, jobComplete.CustomMessage));
            }
            else
            {
                OnJobExecuted(new EventArgs_JobListener("66", "General failure!" + jobException));
            }

            // Too much information
            // Log.Debug(Name + " JobWasExecuted: " + context + "|||" + jobException);
        }

        /// <summary>
        /// Recommended per-class reference to log4net (http://www.codeproject.com/Articles/140911/log4net-Tutorial)
        /// </summary>
        // private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Event raiser (from http://stackoverflow.com/questions/14517563/quartz-schedulers-job-fired-but-joblistener-not-picking-up-event?rq=1)
        protected virtual void OnJobExecuted(EventArgs args)
        {
            // This code will prevent IllegalThreadContext exceptions
            EventHandler jobExecHandler = JobExecutedHandler;

            if (jobExecHandler != null)
            {
                var target = jobExecHandler.Target as ISynchronizeInvoke;

                if (target != null && target.InvokeRequired)
                {
                    target.Invoke(jobExecHandler, new object[] { this, args });
                }
                else
                {
                    jobExecHandler(this, args);
                }
            }
        }

    }

}
