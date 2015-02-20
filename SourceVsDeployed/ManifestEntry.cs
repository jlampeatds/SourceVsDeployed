namespace SourceVsDeployed
{
    /// <summary>
    /// Represents a single manifest entry: expectation, relative path and optional hash
    /// </summary>
    public class ManifestEntry
    {
        public string Expectation;
        public string RelativePath;
        public string Md5;

        /// <summary>
        /// Constructor
        /// </summary>
        public ManifestEntry(string initialExpectation, string initialRelativePath, string initialOptionalMd5)
        {
            Expectation = initialExpectation;
            RelativePath = initialRelativePath;
            Md5 = initialOptionalMd5;
        }
    }
}
