using System;

namespace GetSourceCodeManifest
{

    /// <summary>
    /// Handles interaction with command-line console interface.  
    /// </summary>
    public static class Cli
    {

        /// <summary>
        /// Prints an error message in red
        /// </summary>
        /// <param name="errorMessage"></param>
        public static void PrintError(string errorMessage)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ResetColor();
        }

        /// <summary>
        /// Print a useful help message.  
        /// </summary>
        public static void PrintHelp()
        {
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("GetSourceCodeManifest v" + typeof(Program).Assembly.GetName().Version);
            Console.WriteLine("  Creates a manifest file that describes the contents of a source code branch.");
            Console.WriteLine("  Written 2015 by Jonathan Lampe.  Use to feed SourceVsDeployed.exe.");
            Console.WriteLine();
            Console.WriteLine("  Usage:");
            Console.WriteLine("    GetSourceCodeManifest [repository] [operation] [address] [sourcedir] [expectfile] [manifestfile]");
            Console.WriteLine("      [repository]   - type of repository: tfs (only)");
            Console.WriteLine("      [operation]    - operation to perform: list or listmd5");
            Console.WriteLine("      [address]      - URL of repository");
            Console.WriteLine("      [sourcedir]    - folder path and file mask: e.g., Proj\\Trunk");
            Console.WriteLine("      [expectfile]   - name of file describing what to expect with various files in production");
            Console.WriteLine("      [manifestfile] - name of manifest file to create (or overwrite)");
            Console.WriteLine();
            Console.WriteLine("  Example:");
            Console.WriteLine("    GetSourceCodeManifest tfs list http://tfs02:808 Proj\\Trunk\\Web exp.txt ptw.txt");
            Console.WriteLine();
            Console.WriteLine("  Advanced settings available in the .config file:");
            Console.WriteLine("    LocalDownloadFolder - local temp folder used in MD5 calculations");
            Console.WriteLine("    (TFS authentication uses current user's credentials.)");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}
