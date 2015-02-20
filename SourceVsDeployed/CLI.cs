using System;
using System.IO;
using System.Reflection;

namespace SourceVsDeployed
{
// ReSharper disable once InconsistentNaming
    public class CLI
    {

        /// <summary>
        /// Displays help to the console
        /// </summary>
        internal static void DisplayHelp()
        {
            try
            {
                string exeName = Path.GetFileName(Assembly.GetEntryAssembly().Location);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("SourceVsDeployed v" + typeof(Program).Assembly.GetName().Version);
                Console.WriteLine("  Consumes a manifest file that describes how files should be deployed");
                Console.WriteLine("  and checks it against actually deployed content.");
                Console.WriteLine("  Written 2015 by Jonathan Lampe.  Often fed by GetSourceCodeManifest.");
                Console.WriteLine("");
                Console.WriteLine("  Usage:");
                Console.WriteLine("    " + exeName + " [action] [manifest] [target]");
                Console.WriteLine("      [action]    parse     = just check for a valid manifest file");
                Console.WriteLine("                  list      = consume manifest but do not perform md5 checks");
                Console.WriteLine("                  validate  = consumer manifest and perform a full validation");
                Console.WriteLine("      [manifest]  EITHER: ");
                Console.WriteLine("                     The full URL of the CSV file containing a remote manifest.");
                Console.WriteLine("                     The path of the TSV file containing a local manifest.");
                Console.WriteLine("      [target]    The base URL to check. If left blank and the manifest is a URL,");
                Console.WriteLine("                  then the base URL will be derived from the manifest URL.");
                Console.WriteLine();
                Console.WriteLine("  Example:");
                Console.WriteLine("    GetSourceCodeManifest tfs list http://tfs02:808 Proj\\Trunk\\Web exp.txt ptw.txt");
                Console.WriteLine("");
                Console.WriteLine("  Advanced settings configured in " + exeName + ".config:");
                Console.WriteLine("    Threads           Maximum simultaneous file downloads.");
                Console.WriteLine("    MaxURLs           Maximum URLs to test.");
                Console.WriteLine("    ManifestTempFile  Name of temporary file used with remote manifests.");
                Console.WriteLine("    TempFolder        Temporary folder to use to process downloaded files.");
                Console.WriteLine("    CorruptFolder     Folder to keep corrupt files in.");
                Console.WriteLine("    KeepCorruptFiles  Whether or not to keep copies of corrupt files.");
                Console.WriteLine("");
                Console.WriteLine("  Advanced settings configured in Log4NetConfig.xml:");
                Console.WriteLine("    FileAppender      Full path to log file with full details and whether to append");
                Console.WriteLine("    level             Debug level: DEBUG, INFO, WARN or ERROR");
                Console.WriteLine("");
                Console.ResetColor();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
                // Added Exception handler to get through Unit Tests
            }
        }
    }
}
