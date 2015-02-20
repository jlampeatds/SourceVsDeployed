using System;
using System.IO;

namespace GetSourceCodeManifest
{
    /// <summary>
    /// Handles interaction with manifest file.
    /// </summary>
    public class ManifestFile
    {
        /// <summary>
        /// Internal storage of manifest file path.  Can only be set in the constructor.  
        /// </summary>
        private readonly string _filePath;

        /// <summary>
        /// Manifest file path.  Can only be set in the constructor.  
        /// </summary>
        public string FilePath { 
            get
            {
                return _filePath;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="manifestFilePath"></param>
        public ManifestFile(string manifestFilePath)
        {
            _filePath = manifestFilePath;
        }

        /// <summary>
        /// Writes a single line in a manifest file.
        /// </summary>
        public void WriteLine(string oneLine)
        {
            File.AppendAllText(_filePath, oneLine);
        }

        /// <summary>
        /// Writes a single entry in a manifest file.
        /// Entries are PATH, MD5.
        /// Delimited by TAB.
        /// </summary>
        public void WriteEntry(string expectation, string relativePath, string md5Hash)
        {
            if (expectation != "ignore")
            {
                WriteLine(expectation + "\t" + relativePath + "\t" + md5Hash + "\r\n");
            }
        }

        /// <summary>
        /// Writes a quick header to the manifest file
        /// </summary>
        public void WriteHeader(string repository, string operation, string address, string sourcedir, string expectfile, string manifestfile)
        {
            string sHeader = "" +
                "' - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -\r\n" +
                "' Manifest file created by GetSourceCodeManifest for SourceVsDeployed on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" +
                "' ...using arguments: \r\n" +
                "'   repository   = " + repository + "\r\n" +
                "'   operation    = " + operation + "\r\n" +
                "'   address      = " + address + "\r\n" +
                "'   sourcedir    = " + sourcedir + "\r\n" +
                "'   expectfile   = " + expectfile + "\r\n" +
                "'   manifestfile = " + manifestfile + "\r\n" +
                "' - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -\r\n";
            WriteLine(sHeader);
        }

        /// <summary>
        /// Deletes the manifest file.
        /// </summary>
        public void Delete()
        {
            File.Delete(_filePath);
        }
    }
}
