using System;

namespace GetSourceCodeManifest
{
    /// <summary>
    /// Helper (mostly static) functions for pathing
    /// </summary>
    public class Pathing
    {

        /// <summary>
        /// Fixes a TFS path specification by adding a trailing slash (if needed) and a Windows wildcard ("*.*").
        /// </summary>
        public static string FixTfsPathSpec(string tfsPathSpec)
        {
            string sPathSpec = tfsPathSpec;
            if (sPathSpec.EndsWith("/*.*"))
            {
                return sPathSpec;
            }
            if (!sPathSpec.EndsWith("/"))
            {
                sPathSpec += "/";
            }
            if (!sPathSpec.EndsWith("*.*"))
            {
                sPathSpec += "*.*";
            }
            return sPathSpec;
        }


        /// <summary>
        /// Given a root path spec and a full TFS path, returns the relative path to resource with forward slashes
        /// </summary>
        public static string MakeRelativeTfsPath(string rootPathSpec, string fullTfsPath)
        {
            int iPathStart = fullTfsPath.IndexOf(rootPathSpec, StringComparison.OrdinalIgnoreCase);
            string sRelativePath = fullTfsPath;
            if (iPathStart > -1)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (fullTfsPath.Length > rootPathSpec.Length)
                {
                    sRelativePath = fullTfsPath.Substring(iPathStart + rootPathSpec.Length + 1);
                }
                else
                {
                    sRelativePath = "";
                }
            }
            return sRelativePath;
        }

        /// <summary>
        ///  Pulls off the trailing slash and wildcard
        /// </summary>
        public static string RemoveTfsFileWildcard(string pathSpec)
        {
            string sPath = pathSpec.Replace("/*.*", "");
            if (sPath.EndsWith("/"))
            {
                sPath = sPath.Substring(0, sPath.Length - 1);
            }
            return sPath;
        }

        /// <summary>
        /// Gets the last folder in a long TFS path
        /// </summary>
        /// <returns></returns>
        public static string GetLastFolderInTfsPath(string pathSpec)
        {
            string sPathSpec = RemoveTfsFileWildcard(pathSpec);
            string[] saPath = sPathSpec.Split('/');
            return saPath[saPath.Length - 1];
        }
    }
}
