using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using log4net;

namespace SourceVsDeployed
{
    public class Manifest
    {

        // Statistics about file processing
        public int LinesTotal {get { return _linesTotal; } }
        public int LinesComments {get { return _linesComments; } }
        public int LinesIgnored {get { return _linesIgnored; } }
        public int LinesErrored { get { return _linesErrored; } }
        public int LinesBlank { get { return _linesBlank; } }
        public int LinesParsed {get { return _linesParsed; } }
        private int _linesTotal;
        private int _linesComments;
        private int _linesIgnored;
        private int _linesErrored;
        private int _linesBlank;
        private int _linesParsed;

        // Manifest file represented as an array and properties (could do this as an array of objects but meh)
        private string[,] _saManifest;

        /// <summary>
        /// Returns the entry object for a numbered manifest entry, or null there is no entry for the provided index.
        /// </summary>
        public ManifestEntry GetEntry(int index)
        {
            // Check that the manifest array isn't empty and that the index will resolve
            if (_saManifest != null)
            {
                if (index > -1 && index < _saManifest.GetLength(0))
                {
                    return new ManifestEntry(Utility.LesserExpectation(Arguments.Action, _saManifest[index, 0]), 
                        _saManifest[index, 1], 
                        _saManifest[index, 2]);
                }
            }
            return null;
        }

        /// <summary>
        /// The target URL associated with this manifest.
        /// </summary>
        public string Target { get; set; }

        // Set the maximum number of entries that we can check
        // Performance note (from testing on a Windows 7 machine in Sep 2014)
        // - Size of application when this is set to manifestLengthMax and I loaded:
        //   - 5 entries: 18MB memory
        //   - 500,000 entries: 78MB memory (from a 22.2MB text file)
        // ReSharper disable once CSharpWarnings::CS0618
        public int ManifestLengthMax
        {
            get { return _manifestLengthMax; }
        }
        private readonly int _manifestLengthMax;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Manifest(string target)
        {
            Target = target;
            _manifestLengthMax = Int32.Parse(ConfigurationManager.AppSettings["MaxURLs"]);
        }

        /// <summary>
        /// Attempts to download the provided manifest file
        /// </summary>
        /// <param name="manifestUrl">URL of manifest file to download</param>
        /// <returns>Returns either the local path of the manifest file or an empty string if there was an issue.</returns>
        public string DownloadManifest(string manifestUrl)
        {
            string manifestChecksumUrl = MakeChecksumPath(manifestUrl);
            string manifestPath = "";

            // Pull temporary path in from app.config
            // ReSharper disable once CSharpWarnings::CS0618
            string temporaryPath = Utility.ResolveMacros(ConfigurationManager.AppSettings["ManifestTempFile"], Target);
            string temporaryChecksumPath = MakeChecksumPath(temporaryPath);
            try
            {
                Log.Debug("Trying to download manifest: " + manifestUrl);
                using (var client = new WebClient())
                {
                    client.DownloadFile(manifestUrl, temporaryPath);
                }
                if (File.Exists(temporaryPath))
                {
                    Log.Debug("Download of manifest to " + temporaryPath + " was successful.");

                    // If all OK so far, download the manifest checksum file
                    try
                    {
                        Log.Debug("Trying to download manifest checksum: " + manifestChecksumUrl);
                        using (var client = new WebClient())
                        {
                            File.Delete(temporaryChecksumPath);
                            client.DownloadFile(manifestChecksumUrl, temporaryChecksumPath);
                        }
                        if (File.Exists(temporaryChecksumPath))
                        {
                            Log.Debug("Download of manifest checksum to " + temporaryChecksumPath + " was successful.");
                            // If both the manifest download and checksum download worked, set the "OK" (non-blank) return code
                            manifestPath = temporaryPath;
                        }
                        else
                        {
                            Log.Debug("Download of manifest checksum happened but local file " + temporaryChecksumPath + " does not exist.");
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Debug("Encountered exception during download of manifest checksum: " + exception);
                        Log.Warn("Could not download manifest checksum file: " + manifestChecksumUrl + "!");
                    }

                }
                else
                {
                    Log.Debug("Download of manifest happened but local file " + temporaryPath + " does not exist.");
                }
            }
            catch (Exception exception)
            {
                Log.Debug("Encountered exception during download of manifest: " + exception);
            }
            return manifestPath;
        }

        /// <summary>
        /// Turns a path like d:\frog.txt into d:\frog_md5.txt
        /// or http://server/path/file.txt into http://server/path/file_md5.txt
        /// by splitting on the period and appending to the second-to-last segement
        /// </summary>
        public static string MakeChecksumPath(string originalPath)
        {
            string[] saPath = originalPath.Split('.');
            string sFinalPath = "";
            if (saPath.Length > 1)
            {
                for (int i = 0; i < saPath.Length; i++)
                {
                    // Second-to-last segment gets the checksum name appended to it
                    if (i == saPath.Length - 2)
                    {
                        saPath[i] += "_md5";
                    }
                    // If this is the first segment, do not prepend a period.  Otherwise, DO prepend it.
                    if (i == 0)
                    {
                        sFinalPath += saPath[i];
                    }
                    else
                    {
                        sFinalPath += "." + saPath[i];                            
                    }
                }
            }
            return sFinalPath;
        }

        /// <summary>
        /// Validates that this is an authorized manifest and then parses all entries into a local string array
        /// </summary>
        /// <param name="manifestPath">Path of local manifest file</param>
        public bool ValidateAndParseDownloadedManifest(string manifestPath)
        {
            // Clear out the internal manifest array
            _saManifest = null;

            try
            {
                // Validate the contents of the manifest file
                string manifestChecksumPath = MakeChecksumPath(manifestPath);
                string manifestChecksum = File.ReadAllText(manifestChecksumPath).Substring(0, 32);
                    // Quick way to ignore training linefeeds
                //const string manifestChecksumSalt = ";4Mw4{~/*twEs8,<3Pred";
                string manifestChecksumPrepend = ConfigurationManager.AppSettings["ManifestMD5Prepend"];
                // Attempt to calculate hash
                Log.Debug("Calculating salted hash of manifest.");
                string manifestActualChecksum;
                using (var md5 = MD5.Create())
                {
                    var manifestText = File.ReadAllText(manifestPath);
                    // [system.Text.Encoding]::UTF8 in Powershell
                    var utf8 = new System.Text.UTF8Encoding();
                    byte[] manifestTextBuffer = utf8.GetBytes(manifestChecksumPrepend + manifestText);
                    manifestActualChecksum =
                        BitConverter.ToString(md5.ComputeHash(manifestTextBuffer)).Replace("-", "").ToLower();
                    // Log.Debug("Hash of " + temporaryPath + " is " + testHash + ".");
                }

                // Compare hashes
                if (manifestActualChecksum != manifestChecksum)
                {
                    Log.Error("Manifest file failed hash check! (Got " + manifestActualChecksum + ", expected " +
                              manifestChecksum + ".)");
                    _linesTotal = _linesComments = _linesIgnored = _linesBlank = _linesParsed = 0;
                    return false;
                }
                Log.Debug("Manifest hash is OK - proceeding!");

                string[,] saManifest = ParseFileToArray(manifestPath);

                Log.Debug("Deleting temporary manifest and checksum.");
                File.Delete(manifestPath);
                File.Delete(manifestChecksumPath);

                if (saManifest == null)
                {
                    return false;
                }
                _saManifest = saManifest;
                return true;
            }
            catch (Exception e)
            {
                Log.Debug("Caught exception in ValidateAndParseDownloadedManifest: " + e.Message);
                Log.Warn("Could not open local manifest or checksum file(s).");
            }
            return false;

        }

        /// <summary>
        /// Parses a manifest file delimited by tabs.  If successful (returns true), use GetEntry calls above to use the manifest.
        /// </summary>
        /// <param name="manifestPath"></param>
        /// <returns></returns>
        public bool ParseFile(string manifestPath)
        {
            _saManifest = ParseFileToArray(manifestPath);
            return _saManifest != null;
        }

        /// <summary>
        /// Parses a manifest file (delimited by tabs) and returns an appropriate 3-column string array.
        /// Will return NULL if the file is unparsable.  
        /// </summary>
        private string[,] ParseFileToArray(string manifestPath)
        {
            if (!File.Exists(manifestPath))
            {
                Log.Error("Manifest file " + manifestPath + " does not exist!");
                return null;
            }
            try
            {
                int linesTotal = 0;
                int linesComments = 0;
                int linesIgnored = 0;
                int linesBlank = 0;
                int linesParsed = 0;
                int linesErrored = 0;
                string line;
                const string commentChar = "'";
                var saManifest = new string[_manifestLengthMax, 3];

                // Read the file and parse it line by line.
                Log.Debug("Starting read of " + manifestPath);
                var file = new StreamReader(manifestPath);
                while ((line = file.ReadLine()) != null)
                {
                    linesTotal++;
                    line = line.Trim();
                    // Blank line
                    if (line.Length == 0)
                    {
                        linesBlank++;
                    }
                    else
                    {
                        // Comment line
                        if (line.StartsWith(commentChar))
                        {
                            linesComments++;
                        }
                        else
                        {
                            // Line format: 
                            //   Expectation[tab]RelativePath[tab]MD5(optional)
                            // Example:
                            //   ignore root/sub/file.txt c36ee71a9dd26d6f3fea9531b48ff140

                            // Parse line
                            try
                            {
                                if (!line.Contains("\t"))
                                {
                                    Log.Warn("Could not find delimiter in line #" + linesTotal + ".");
                                    linesErrored++;
                                }
                                else
                                {
                                    // Check to see if we have too many entries
                                    if (linesParsed >= _manifestLengthMax)
                                    {
                                        Log.Debug("Ignoring line #" + linesTotal + " because it would exceed the maximum number of allowed checks! (" + _manifestLengthMax + ")");
                                        linesIgnored++;
                                    }
                                    else
                                    {
                                        // All seems to be OK, so parse the line
                                        string[] saLine = line.Split('\t');
                                        saManifest[linesParsed, 0] = saLine[0];
                                        saManifest[linesParsed, 1] = saLine[1];
                                        // Check for MD5
                                        if (saLine.Length > 2)
                                        {
                                            saManifest[linesParsed, 2] = saLine[2];
                                        }
                                        linesParsed++;
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                Log.Warn("Caught exception parsing line #" + linesTotal + " (" + exception + ").");
                                linesErrored++;
                            }

                        }
                    }

                }

                file.Close();

                // Pass all parameters up, print a little trace message and leave
                _linesTotal = linesTotal;
                _linesComments = linesComments;
                _linesIgnored = linesIgnored;
                _linesErrored = linesErrored;
                _linesBlank = linesBlank;
                _linesParsed = linesParsed;
                Log.Info(string.Format("Parsed {0} entries from a {1} line manifest with {2} errors.", LinesParsed, LinesTotal, linesErrored));

                // All done.  Was it good enough?  (At least one line parsed and no parse errors.)  
                if (_linesIgnored > 0)
                {
                    Log.Warn(string.Format("Ignored {0} lines.", LinesIgnored));
                }
                return linesParsed > 0 && linesErrored == 0 ? saManifest : null;

            }
            catch (Exception e)
            {
                Log.Error("Failed to parse manifest file " + manifestPath + "! (" + e.Message + ")");
                return null;    
            }
        }

        public bool TemporaryFolderHasBeenCreatedOrExists()
        {
            try
            {
                string sFolderPath = Arguments.TemporaryFolder;
                // Look for an existing folder
                if (Directory.Exists(sFolderPath))
                {
                    return true;
                }
                // Else, try to create the folder and then make it really exists
                if (sFolderPath != null)
                {
                    Directory.CreateDirectory(sFolderPath);
                    if (Directory.Exists(sFolderPath))
                    {
                        return true;
                    }
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
            return false;
        }

        /// <summary>
        /// Recommended per-class reference to log4net (http://www.codeproject.com/Articles/140911/log4net-Tutorial)
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// If we have elected to save corrupt files, and there is at least one corrupt file, 
        /// rename the current temporary folder to a permanent corrupt folder.  
        /// </summary>
        internal void RetainCorruptFolder(int numberOfCorruptEntries)
        {
            if (numberOfCorruptEntries > 0 && Arguments.KeepCorruptFiles)
            {
                string sCorruptFolderPath = Utility.ResolveMacros(ConfigurationManager.AppSettings["CorruptFolder"], Target);
                try
                {
                    Directory.Move(Arguments.TemporaryFolder, sCorruptFolderPath);
                    Log.Info("Saved " + numberOfCorruptEntries + " corrupt files in " + sCorruptFolderPath + " OK.");
                }
                catch (Exception e)
                {
                    Log.Warn("Could not rename temp folder from " + Arguments.TemporaryFolder + " to " + sCorruptFolderPath + "! (" + e.Message + ")");
                }
            }
            else
            {
                Log.Debug("No need to save corrupt files.  Deleting temp folder instead...");
                try
                {
                    Directory.Delete(Arguments.TemporaryFolder);
                    Log.Debug("Deleted temporary folder OK.");
                }
                catch (Exception e)
                {
                    Log.Warn("Could not delete temp folder " + Arguments.TemporaryFolder + "! (" + e.Message + ")");
                }
            }
        }
    }
}
