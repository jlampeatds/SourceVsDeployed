using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using log4net;
using Quartz;

namespace SourceVsDeployed
{
    // ReSharper disable once InconsistentNaming
    //    [DisallowConcurrentExecution]
    //    [PersistJobDataAfterExecution]
    class Job_CheckWebFile : IJob
    {

        /// <summary>
        /// Runs an instance of this job
        /// </summary>
        public void Execute(IJobExecutionContext context)
        {
            var declaringType = MethodBase.GetCurrentMethod().DeclaringType;
            if (declaringType != null)
                Log.Debug(declaringType.Name + "." + MethodBase.GetCurrentMethod().Name + "()");

            // Pull up the job context
            // JobKey key = context.JobDetail.Key;
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string url = dataMap.GetString("URL");
            string hash = dataMap.GetString("Hash");
            string expectation = dataMap.GetString("Expectation");
            string jobNumber = dataMap.GetString("JobNumber");
            string finalMessage = "(no message specified)";   // Will return a human-readable message
            Log.Debug("Job #" + jobNumber + " running " + expectation + " against URL " + url + " and hash " + hash);
            // resultCodes:
            // 0 = file exists and hash is OK
            // 1 = file exists but hash failed
            // 2 = file does not exist (404)
            // 4 = other error
            // -1 = still running
            int resultCode = -1;

            // HACK to slow down processes during testing
            // Thread.Sleep(Int32.Parse(jobNumber) * 1000);

            // Set this to keep the file from being deleted after processing
            bool keepFile = false; 

            // Set the temporary path.  This is checked to already exist in an earlier step. 
            string temporaryPath = Arguments.TemporaryFolder + "\\" + jobNumber;

            try
            {
                // Attempt to download
                Log.Debug("Trying to download url: " + url);
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, temporaryPath);
                }
                if (File.Exists(temporaryPath))
                {
                    Log.Debug("Download of " + url + " was successful.");

                    if (expectation == "md5")
                    {
                        // Attempt to calculate hash
                        Log.Debug("Calculating hash of " + temporaryPath);
                        string testHash;
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(temporaryPath))
                            {
                                testHash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                                // Log.Debug("Hash of " + temporaryPath + " is " + testHash + ".");
                            }
                        }
                        // Compare hashes
                        if (testHash == hash)
                        {
                            finalMessage = "Hashes match!";
                            resultCode = 0;
                        }
                        else
                        {
                            finalMessage = "Failed hash check! (Got " + testHash + ", expected " + hash + ".)";
                            resultCode = 1;
                            if (Arguments.KeepCorruptFiles)
                            {
                                keepFile = true;
                            }
                        } 
                    } 
                    else if (expectation == "exist")
                    {
                            finalMessage = "File exists!";
                            resultCode = 0;                                                    
                    }
                    else if (expectation == "missing")
                    {
                        finalMessage = "File is not missing!";
                        resultCode = 1;
                    }

                    // Delete the local file
                    // UNLESS the file is corrupt and we decided to keep a copy
                    if (keepFile)
                    {
                        string sLocalFileName = Arguments.TemporaryFolder + "\\" + Utility.PrepareLocalFilename(url,Arguments.Target);
                        Log.Debug("Renaming local file from " + temporaryPath + " to " + sLocalFileName + "...");
                        try
                        {
                            File.Move(temporaryPath, sLocalFileName);
                            Log.Debug("Renamed local file from " + temporaryPath + " to " + sLocalFileName + " OK.");
                        }
                        catch (Exception e)
                        {
                            Log.Warn("Could not rename local file from " + temporaryPath + " to " + sLocalFileName + ". (" + e.Message + ")");                            
                        }

                    } else
                    {
                        Log.Debug("Deleting local file.");
                        File.Delete(temporaryPath);                        
                    }

                }
                else
                {
                    finalMessage = "Download of url happened but local file does not exist.";
                }

            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.ProtocolError &&
                    webException.Response != null)
                {
                    var resp = (HttpWebResponse)webException.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        finalMessage = "Cannot be downloaded! (404)";
                    }
                    else
                    {
                        finalMessage = "Cannot be downloaded! (" + resp.StatusCode + ")";
                    }
                }
                else
                {
                    finalMessage = "Cannot be downloaded! " + webException;
                }

                // This is OK if a file is supposed to be missing.  Else return a code of "2"
                resultCode = expectation == "missing" ? 0 : 2;
            }
            catch (Exception exception)
            {
                finalMessage = "Encountered exception during download of url: " + exception;
                resultCode = 4;
            }

            Log.Debug(finalMessage);
            Log.Debug("Job #" + jobNumber + " has been released and will now exit.");
            throw new Exception_JobComplete(resultCode.ToString(CultureInfo.InvariantCulture),
                "Job #" + jobNumber + "|" + url + "|" + finalMessage);

        }

        /// <summary>
        /// Recommended per-class reference to log4net (http://www.codeproject.com/Articles/140911/log4net-Tutorial)
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


    }

}
