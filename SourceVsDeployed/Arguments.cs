using System.Configuration;
using System.Reflection;

using log4net;

namespace SourceVsDeployed
{
    /// <summary>
    /// Represents command-line and other global arguments
    /// </summary>
    public static class Arguments
    {
        /// <summary>
        /// Represents a command-line return code.  
        /// </summary>
        public static int ReturnCode { get; set; }

        /// <summary>
        /// Represents a command-line return code.  
        /// </summary>
        public static bool ReadyToRun { get; set; }

        /// <summary>
        /// The requested action.  Argument #1.
        /// </summary>
        public static string Action { get; set; }

        /// <summary>
        /// The manifest with detailed instructions.  Argument #2.
        /// </summary>
        public static string Manifest { get; set; }

        /// <summary>
        /// A (sometimes optional) target URL.  Argument #3.
        /// </summary>
        public static string Target { get; set; }

        /// <summary>
        /// Returns true if the current Manifest is a valid URL.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static bool ManifestIsAURL {
            get
            {
                return Utility.IsAValidUrl(Manifest);                   
            }
        }

        /// <summary>
        /// Populate static values based on original command-line arguments.
        /// (Originally a constructor when this was a non-static object.)
        /// </summary>
        public static void Populate(string[] args)
        {
            // Reset state properties
            // Default is a general failure
            ReturnCode = 1;
            ReadyToRun = false;

            // Reset caches
            _temporaryFolder = "";
            _keepCorruptFilesHasBeenSet = false;

            // Reset value properties
            Action = "";
            Manifest = "";
            Target = "";

            // Display help if no arguments or too few are provided
            if (args.Length < 2)
            {
                Log.Debug("Displaying help and exiting.");
                CLI.DisplayHelp();
                ReturnCode = 0;
                return;
            }
            Action = args[0].ToLower();
            Manifest = args[1].ToLower();
            Target = MakeTarget(args, Manifest);

            // Perform a little validation before proceeding
            if (!Utility.IsAValidUrl(Target))
            {
                Log.Error("Could not read or calculate a valid target!");
                return;                
            }
            if (!IsAValidAction(Action))
            {
                Log.Error("Can not perform invalid action \"" + Action + "\"!");
                return;                                
            }

            // Tell the calling program things are good to go
            ReturnCode = 0;
            ReadyToRun = true;

        }

        /// <summary>
        /// Given an argument array of (possibly) at least three arguments
        /// and a manifest, return our best guess at an appropriate target.
        /// (An explicitly provided Target (arg #3) will win, otherwise we'll look
        /// at the provided Manifest and derive an entry if possible.)
        /// If we cannot figure out a valid entry, we will return a blank string.
        /// </summary>
        public static string MakeTarget(string[] args, string manifestArg)
        {
            if (args == null || manifestArg == null)
            {
                return "";
            }
            // If a target was provided, use that
            if (args.Length > 2)
            {
                Log.Info("Target " + args[2] + " was explicitly provided.");
                return Utility.EnsureHasTrailingSlash(args[2]);
            }
            // Else, try to derive a URL from the Manifest, but only if it's a valid URL
            if (Utility.IsAValidUrl(manifestArg))
            {
                // Parse protocol, host and port from the incoming URL
                try
                {
                    string[] urlSplit = manifestArg.Split('/');
                    string urlBase = urlSplit[0] + "/" + urlSplit[1] + "/" + urlSplit[2] + "/";
                    Log.Info("Target " + urlBase + " was derived from manifest.");
                    return Utility.EnsureHasTrailingSlash(urlBase);
                }
                catch
                {
                    Log.Error("Attempted but failed to derive Target from manifest.");
                    return "";
                }
            }
            Log.Error("No Target provided and cannot derive one from manifest.");
            return "";
        }


        /// <summary>
        /// Checks to see if string refers to a valid action
        /// </summary>
        public static bool IsAValidAction(string action)
        {
            switch (action)
            {
                case "parse":
                case "list":
                case "validate":
                    return true;
            }
            return false;
        }

        private static bool _keepCorruptFiles;             // = false
        private static bool _keepCorruptFilesHasBeenSet;   // = false

        /// <summary>
        /// Indicates whether or not we're keeping corrupt files.  
        /// (This is a bit of a hack.)
        /// </summary>
        public static bool KeepCorruptFiles
        {
            get
            {
                // Cache the local value so we only hit AppSettings once
                if (!_keepCorruptFilesHasBeenSet)
                {
                    if (ConfigurationManager.AppSettings["KeepCorruptFiles"] == "true") { _keepCorruptFiles = true; }
                    _keepCorruptFilesHasBeenSet = true;
                }
                return _keepCorruptFiles;
            }
        }


        private static string _temporaryFolder = "";

        /// <summary>
        /// Returns the value of the temporary folder after interpreting the macros from the *.config file.
        /// </summary>
        public static string TemporaryFolder
        {
            get
            {
                // Cache the temporary value so we aren't hitting the AppSettings often
                if (_temporaryFolder.Length == 0)
                {
                    _temporaryFolder = Utility.ResolveMacros(ConfigurationManager.AppSettings["TempFolder"], Target);
                }
                return _temporaryFolder;
            }
        }



        /// <summary>
        /// Recommended per-class reference to log4net (http://www.codeproject.com/Articles/140911/log4net-Tutorial)
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


    }
}
