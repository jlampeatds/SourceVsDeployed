using System;
using System.Configuration;
using System.IO;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Security.Cryptography;

namespace GetSourceCodeManifest
{
    class Program
    {
        /// <summary>
        /// Use a global expectations object to avoid beating on the expectations file.
        /// </summary>
        private static Expectations _expectations;

        /// <summary>
        /// Use a global manifest file object to avoid passing information through primary recursive functions.
        /// </summary>
        private static ManifestFile _manifestFile;

        /// <summary>
        /// Quick command-line application to pull file information from repositories.
        /// At the moment, only TFS is supported, and the only action supported is a raw list.
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        static int Main(string[] args)
        {
            try
            {
                // Simple command-line processing
                if (args.Length < 6)
                {
                    throw new NotSupportedException("This application requires more command-line arguments.");
                }
                string sTypeOfRepository = args[0].ToLower();
                if (sTypeOfRepository != "tfs")
                {
                    throw new NotImplementedException("Repository type " + sTypeOfRepository + " is not supported!");
                }
                string sOp = args[1].ToLower();
                if (!(sOp == "list" || sOp == "md5list"))
                {
                    throw new NotImplementedException("Operations " + sTypeOfRepository + " is not supported!");
                }
                string sTfsUri = args[2];
                string sPathSpec = "$/" + args[3];
                string sExpectationPath = args[4];
                _expectations = new Expectations(sExpectationPath);
                if (!_expectations.FileExists())
                {
                    throw new FileNotFoundException("Could not find Expectation File " + sExpectationPath + "!");
                }
                string sManifestPath = args[5];
                _manifestFile = new ManifestFile(sManifestPath);
                string sLocalDownloadPath = ConfigurationManager.AppSettings["LocalDownloadFolder"];
                if (!Directory.Exists(sLocalDownloadPath))
                {
                    throw new DirectoryNotFoundException("Could not find LocalDownloadFolder " + sLocalDownloadPath +                                                         "!");
                }
                if (!sLocalDownloadPath.EndsWith("\\"))
                {
                    sLocalDownloadPath += "\\";
                }

                // Prepare the manifest file
                _manifestFile.Delete();
                _manifestFile.WriteHeader(sTypeOfRepository, sOp, sTfsUri, sPathSpec, sExpectationPath, sManifestPath);

                // Connect to the TFS service (a future implementation supporting multiple source types would break this out)
                // from http://stackoverflow.com/questions/4757883/get-a-file-list-from-tfs 
                Console.Write("Connecting to " + sTfsUri + "...");
                var server = RegisteredTfsConnections.GetProjectCollection(new Uri(sTfsUri));
                var projects = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(server);
                var versionControl = (VersionControlServer) projects.GetService(typeof (VersionControlServer));
                Console.WriteLine("OK.");

                if (sOp == "md5list")
                {
                    Console.WriteLine("Using local download path " + sLocalDownloadPath + " for MD5 calculations.");                    
                }

                // Get a list of files and write it out to the manifest file
                sPathSpec = Pathing.FixTfsPathSpec(sPathSpec);
                Console.WriteLine("Getting a list of files that matches " + sPathSpec + "...");
                var items = versionControl.GetItems(sPathSpec);
                ProcessTfsFolder(versionControl, items, Pathing.RemoveTfsFileWildcard(sPathSpec), sPathSpec, sOp, sLocalDownloadPath);
                Console.WriteLine("File listing is complete. Check " + sManifestPath + " for results.");

            }

            catch (NotSupportedException nse)
            {
                // This is often caused by not having enough command-line parameters
                // Just print the complaint (no "failed") and the help message.
                Cli.PrintError(nse.Message);
                Cli.PrintHelp();
                return 2;
            }

            catch (Exception e)
            {
                Cli.PrintError("FAILED! (" + e.Message + ")");
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Lists the contents of a single TFS folder, recursively.
        /// </summary>
        private static void ProcessTfsFolder(VersionControlServer versionControl, ItemSet items, string rootPathSpec, string pathSpec, string requestedOperation, string localDownloadFolder)
        {

            foreach (var item in items.Items)
            {
                //Console.Write("  Checking " + item.ToString() + "...");
                Console.Write("  " + item.ServerItem + "...");

                if (item.ItemType == ItemType.File)
                {
                    Console.Write("is a file");
                    Item newestItem = item;
                    //newestDate = item.CheckinDate;
                    var fileName = Path.GetFileName(newestItem.ServerItem);
                    string sExpectation = _expectations.LookupFile(fileName);
                    if (requestedOperation == "md5list" && sExpectation == "md5")
                    {
                        if (fileName != null)
                        {
                            string sLocalFilePath = localDownloadFolder + fileName;
                            newestItem.DownloadFile(sLocalFilePath);
                            string sMd5Hash;
                            using (var md5 = MD5.Create())
                            {
                                using (var stream = File.OpenRead(sLocalFilePath))
                                {
                                    sMd5Hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                                    Console.Write(" with an MD5 hash of " + sMd5Hash + "");
                                }
                            }
                            File.Delete(sLocalFilePath);
                            _manifestFile.WriteEntry(sExpectation, Pathing.MakeRelativeTfsPath(rootPathSpec, newestItem.ServerItem), sMd5Hash);
                        }
                    }
                    else if (requestedOperation == "list" || requestedOperation == "md5list")  // Fallback for md5 if not needed
                    {
                        if (fileName != null)
                        {
                            _manifestFile.WriteEntry(sExpectation, Pathing.MakeRelativeTfsPath(rootPathSpec, newestItem.ServerItem), "");
                        }
                    }
                    // Not checking for invalid actions here - see code for that during argument processing
                    Console.WriteLine(" (should " + sExpectation + " on target).");
                }
                else if (item.ItemType == ItemType.Folder)
                {
                    Console.WriteLine("is a folder.  (Drilling in.)");
                    string sPathSpec = Pathing.RemoveTfsFileWildcard(pathSpec) + "/" + Pathing.GetLastFolderInTfsPath(item.ServerItem) + "/*.*";
                    var itemsSub = versionControl.GetItems(sPathSpec);
                    ProcessTfsFolder(versionControl, itemsSub, rootPathSpec, sPathSpec, requestedOperation, localDownloadFolder);
                }
                else
                {
                    Console.WriteLine("not a file or a folder.");
                }

            }

        }
    }
}
