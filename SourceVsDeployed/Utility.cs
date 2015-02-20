using System;
using System.Text;

namespace SourceVsDeployed
{
    /// <summary>
    /// Static class filled with utility functions.
    /// </summary>
    public static class Utility
    {

        /// <summary>
        /// Returns the lower of two provided numbers 
        /// </summary>
        public static int LesserOf(int one, int two)
        {
            return two > one ? one : two;
        }

        /// <summary>
        /// Resolves macros from a template string.  
        /// Currently supports:
        /// [Target] Target URL and folder like www-acme-com+folder1+folder2
        /// [YYYY] Year like 2015
        /// [MM] Month like 02
        /// [DD] Date like 03
        /// [HH] Hour like 09
        /// [TT] Minute like 03
        /// [SS] Second like 07
        /// </summary>
        public static string ResolveMacros(string originalString, string target)
        {
            return ResolveMacros(originalString, target, DateTime.Now);
        }

        /// <summary>
        /// Resolves macros from a template string.  
        /// Use this in unit testing with a fixed date.  
        /// </summary>
        public static string ResolveMacros(string originalString, string target, DateTime dateTime)
        {
            string sResolved = originalString;
            sResolved = ReplaceString(sResolved, "[target]", PrepareTargetForUseInMacro(target));
            sResolved = ReplaceString(sResolved, "[YYYY]", ResolveDateMacro(dateTime, "YYYY"));
            sResolved = ReplaceString(sResolved, "[MM]", ResolveDateMacro(dateTime, "MM"));
            sResolved = ReplaceString(sResolved, "[DD]", ResolveDateMacro(dateTime, "DD"));
            sResolved = ReplaceString(sResolved, "[HH]", ResolveDateMacro(dateTime, "HH"));
            sResolved = ReplaceString(sResolved, "[TT]", ResolveDateMacro(dateTime, "TT"));
            sResolved = ReplaceString(sResolved, "[SS]", ResolveDateMacro(dateTime, "SS"));
            return sResolved;
        }

        /// <summary>
        /// Resolves a single date macro.
        /// </summary>
        public static string ResolveDateMacro(DateTime dateTime, string macro)
        {
            if (macro == null)
            {
                return "[NULL]";
            }
            if (macro == "")
            {
                return "[BLANK]";
            }
            switch (macro.ToUpper())
            {
                case "YYYY":
                    return dateTime.ToString("yyyy");
                case "MM":
                    return dateTime.ToString("MM");
                case "DD":
                    return dateTime.ToString("dd");
                case "HH":
                    return dateTime.ToString("HH");
                case "TT":
                    return dateTime.ToString("mm");
                case "SS":
                    return dateTime.ToString("ss");
            }
            return "[" + macro + "]";
        }

        /// <summary>
        /// Given a target URL, returns a macro-ready value like www-acme-com+folder1+folder2
        /// </summary>
        public static string PrepareTargetForUseInMacro(string target)
        {
            if (target != null)
            {
                string sNiceTarget = target.ToLower();
                string sFinalTarget = "";
                // Chop off the protocol
                sNiceTarget = sNiceTarget.Replace("http://", "").Replace("https://", "");
                // Chop off a trailing slash, if any
                if (sNiceTarget.EndsWith("/"))
                {
                    sNiceTarget = sNiceTarget.Substring(0, sNiceTarget.Length - 1);
                }
                // Replace dots for dashes and slashes for plusses
                sNiceTarget = sNiceTarget.Replace(".", "-").Replace("/", "+");
                // Kill off most other special characters (could do this more elegantly with regex but meh)
                const string sAllowedChars = "abcdefghijklmnopqrstuvwxyz01234567890-+";
                for (int i = 0; i < sNiceTarget.Length; i++)
                {
                    if (sAllowedChars.Contains(sNiceTarget.Substring(i,1)))
                    {
                        sFinalTarget += sNiceTarget.Substring(i, 1);
                    }
                }
                return sFinalTarget;

            }
            return "";
        }

        /// <summary>
        /// Performs a case-insensitive replacement of one string for another.
        /// </summary>
        public static string ReplaceString(string originalString, string oldValue, string newValue)
        {
            return ReplaceString(originalString, oldValue, newValue, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Performs a replacement of one string for another.  Can be case-insensitive or not.
        /// Code imported from http://stackoverflow.com/questions/244531/is-there-an-alternative-to-string-replace-that-is-case-insensitive 
        /// Added null checking and blank string checking
        /// </summary>
        public static string ReplaceString(string originalString, string oldValue, string newValue, StringComparison comparison)
        {
            if (originalString != null && oldValue != null && newValue != null)
            {
                // No original string? Return a blank string.
                if (originalString.Length == 0)
                {
                    return "";
                }
                // No string to look for? Return the original string.
                if (oldValue.Length == 0)
                {
                    return originalString;
                }
                var sb = new StringBuilder();

                int previousIndex = 0;
                int index = originalString.IndexOf(oldValue, comparison);
                while (index != -1)
                {
                    sb.Append(originalString.Substring(previousIndex, index - previousIndex));
                    sb.Append(newValue);
                    index += oldValue.Length;

                    previousIndex = index;
                    index = originalString.IndexOf(oldValue, index, comparison);
                }
                sb.Append(originalString.Substring(previousIndex));

                return sb.ToString();
            }
            return "";
        }


        /// <summary>
        /// Simply makes sure the provided string has a trailing forward slash (for a URL base).  
        /// </summary>
        /// <param name="urlBase"></param>
        /// <returns></returns>
        public static string EnsureHasTrailingSlash(string urlBase)
        {
            if (urlBase == null)
            {
                return "";
            }
            return urlBase.EndsWith("/") ? urlBase : urlBase + "/";
        }

        /// <summary>
        /// Checks to see if a string refers to a valid URL
        /// </summary>
        public static bool IsAValidUrl(string target)
        {
            if (target != null)
            {
                string sTarget = target.ToLower();
                // We only want http or https URLs
                if (target.StartsWith("http://") || target.StartsWith("https://"))
                {
                    // If it looks OK, try to validate it
                    Uri result;
                    if (Uri.TryCreate(sTarget, UriKind.RelativeOrAbsolute, out result))
                    {
                        return true;
                    }
                }                
            }
            return false;
        }



        /// <summary>
        /// Returns original expectation unless 
        /// Action is "list" and Expectation is "md5", in which case it will
        /// return "exists" instead.  
        /// </summary>
        public static string LesserExpectation(string action, string expectation)
        {
            if (action != null && expectation != null)
            {
                if (action == "list" && expectation == "md5")
                {
                    return "exist";
                }
                return expectation;
                
            }
            return "";
        }

        /// <summary>
        /// Consumes a URL like "http://www.yahoo.com/sub/out.txt, a base URL, and produces a local filename like sub_out.txt
        /// </summary>
        public static string PrepareLocalFilename(string fullUrl, string baseUrl)
        {
            return ReplaceString(fullUrl, baseUrl, "").Replace("/","_!_");        
        }
    }
}
