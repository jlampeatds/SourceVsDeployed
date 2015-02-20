using System.IO;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GetSourceCodeManifest
{

    /// <summary>
    /// Handles expectations
    /// </summary>
    public class Expectations
    {
        /// <summary>
        /// Maximum number of expectations the program supports - just bump this up if it's not enough
        /// </summary>
        private const int MaxExpectations = 4096;

        /// <summary>
        /// Caches expectations masks so we only have to hit the expectations file once.
        /// </summary>
        private string[,] _masks;

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
        public Expectations(string expectationsFilePath)
        {
            _filePath = expectationsFilePath;
        }


        /// <summary>
        /// Reads the contents of the expectation file into a string array
        /// </summary>
        public void MakeMasksFromFile()
        {
            // If we already have a cached list, don't redo the work
            if (_masks == null)
            {
                // Support up to MaxExpectations lines
                var stTempMasks = new string[MaxExpectations, 2];
                string sLine;
                int lineNumber = 0;
                var caSpace = new char[1];
                caSpace[0] = ' ';

                // Read the file and crap it into the cache array line by line.
                var file = new StreamReader(_filePath);
                while ((sLine = file.ReadLine()) != null)
                {
                    sLine = sLine.Trim();
                    if (sLine.Length > 0 && !sLine.StartsWith("'"))
                    {
                        string[] saLineSplit = sLine.Split(caSpace, 2);
                        if (saLineSplit.Length == 2)
                        {
                            if (lineNumber < MaxExpectations)
                            {
                                stTempMasks[lineNumber, 0] = saLineSplit[0].ToLower();
                                stTempMasks[lineNumber, 1] = saLineSplit[1].ToLower();
                                lineNumber++;
                            }
                            else
                            {
                                Cli.PrintError("\r\nSorry, this program only supports up to " + MaxExpectations + " expectations at the moment.\r\n");
                            }
                        }
                        else
                        {
                            Cli.PrintError("\r\nUnparsable entry in " + _filePath + ": " + sLine + "\r\n");
                        }
                    }
                }

                // Clean up the open file and resize^b^b^b^b^b^bcopy the large array (can't use resize on 2d arrays)
                file.Close();
                _masks = new string[lineNumber, 2];
                for (int i = 0; i < lineNumber; i++)
                {
                    _masks[i, 0] = stTempMasks[i, 0];
                    _masks[i, 1] = stTempMasks[i, 1];
                }
            }
        }

        /// <summary>
        /// Returns true if the expectations file exists
        /// </summary>
        public bool FileExists()
        {
            if (File.Exists(_filePath))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Given a filename returns an appropriate "expectation" (i.e., what is expected of the file in production)
        /// </summary>
        public string LookupFile(string fileName)
        {
            string sExpectation = "";

            // Check to see if expectation global cache has been populated yet
            MakeMasksFromFile();

            // Check each expectation mask until we have a match
            for (int i = 0; i < _masks.GetLength(0); i++)
            {
                if (MaskMatches(_masks[i, 0], fileName))
                {
                    // If we have a match, save the expectation and end the loop
                    // (The list works on first match, with a catch-all at the end.)  
                    sExpectation = _masks[i, 1];
                    i = _masks.GetLength(0);
                }
            }
            return sExpectation;
        }

        /// <summary>
        /// Checks to see if a filename matches a provided mask.  
        /// Uses the old VisualBasic trick (Google it) to do this.  
        /// </summary>
        public static bool MaskMatches(string mask, string fileName)
        {
            // Windows wildcards
            if (mask == "*.*" || mask == "*")
            {
                return true;
            }
            if (Operators.LikeString(fileName, mask, CompareMethod.Text))
            {
                return true;
            }
            return false;
        }


    }
}
