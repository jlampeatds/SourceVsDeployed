using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SourceVsDeployed.Tests
{
    [TestClass]
    public class TestUtility
    {
        [TestMethod]
        public void TestUtilityEnsureHasTrailingSlash()
        {
            Assert.AreEqual("", Utility.EnsureHasTrailingSlash(null));
            Assert.AreEqual("/", Utility.EnsureHasTrailingSlash(""));
            Assert.AreEqual("/", Utility.EnsureHasTrailingSlash("/"));
            Assert.AreEqual("http://toast/", Utility.EnsureHasTrailingSlash("http://toast"));
            Assert.AreEqual("http://toast/", Utility.EnsureHasTrailingSlash("http://toast/"));
            Assert.AreEqual("http://toast//", Utility.EnsureHasTrailingSlash("http://toast//"));
            Assert.AreEqual("http://toast/cars/dogs/frogs/", Utility.EnsureHasTrailingSlash("http://toast/cars/dogs/frogs"));
            Assert.AreEqual("http://toast/cars/dogs/frogs/", Utility.EnsureHasTrailingSlash("http://toast/cars/dogs/frogs/"));
        }

        [TestMethod]
        public void TestUtilityIsAValidUrl()
        {
            // Invalid URLs
            Assert.IsFalse(Utility.IsAValidUrl(null));
            Assert.IsFalse(Utility.IsAValidUrl(""));
            Assert.IsFalse(Utility.IsAValidUrl("dog"));
            Assert.IsFalse(Utility.IsAValidUrl("mailto:hello@there.com"));

            // Valid URLs
            Assert.IsTrue(Utility.IsAValidUrl("http://blue"));
            Assert.IsTrue(Utility.IsAValidUrl("https://blue"));
            Assert.IsTrue(Utility.IsAValidUrl("http://www.blue.eu"));
            Assert.IsTrue(Utility.IsAValidUrl("https://www.blue.eu"));
            Assert.IsTrue(Utility.IsAValidUrl("http://www.blue.eu:3452"));
            Assert.IsTrue(Utility.IsAValidUrl("https://www.blue.eu:3452"));
            Assert.IsTrue(Utility.IsAValidUrl("http://www.blue.eu/"));
            Assert.IsTrue(Utility.IsAValidUrl("https://www.blue.eu/"));
            Assert.IsTrue(Utility.IsAValidUrl("http://www.blue.eu:3452/"));
            Assert.IsTrue(Utility.IsAValidUrl("https://www.blue.eu:3452/"));
            Assert.IsTrue(Utility.IsAValidUrl("http://www.blue.eu/dog/cat/frog.txt"));
            Assert.IsTrue(Utility.IsAValidUrl("https://www.blue.eu/dog/cat/frog.txt"));
            Assert.IsTrue(Utility.IsAValidUrl("http://www.blue.eu:3452/dog/cat/frog.txt"));
            Assert.IsTrue(Utility.IsAValidUrl("https://www.blue.eu:3452/dog/cat/frog.txt"));
        }

        [TestMethod]
        public void TestUtilityLesserOf()
        {

            // Real cases
            Assert.AreEqual(1, Utility.LesserOf(2, 1));
            Assert.AreEqual(1, Utility.LesserOf(1, 2));
            Assert.AreEqual(1, Utility.LesserOf(1, 1));
            Assert.AreEqual(-3, Utility.LesserOf(1, -3));
            Assert.AreEqual(-3, Utility.LesserOf(-3, -3));
            Assert.AreEqual(-3, Utility.LesserOf(-3, -3));

        }

        [TestMethod]
        public void TestUtilityLesserExpectation()
        {

            // Null cases
            Assert.AreEqual("", Utility.LesserExpectation(null, null));
            Assert.AreEqual("", Utility.LesserExpectation("", null));
            Assert.AreEqual("", Utility.LesserExpectation(null, ""));
            Assert.AreEqual("", Utility.LesserExpectation("frog", null));
            Assert.AreEqual("", Utility.LesserExpectation(null, "frog"));

            // Valid cases
            Assert.AreEqual("exist", Utility.LesserExpectation("list", "md5"));
            Assert.AreEqual("md5", Utility.LesserExpectation("validate", "md5"));
            Assert.AreEqual("md5", Utility.LesserExpectation("parse", "md5"));
            Assert.AreEqual("missing", Utility.LesserExpectation("list", "missing"));
            Assert.AreEqual("missing", Utility.LesserExpectation("validate", "missing"));

            // Invalid cases
            Assert.AreEqual("fakeexpectation", Utility.LesserExpectation("md5", "fakeexpectation"));
            Assert.AreEqual("fakeexpectation", Utility.LesserExpectation("list", "fakeexpectation"));

        }

        [TestMethod]
        public void TestUtilityPrepareTargetForUseInMacro()
        {
            // Empty cases
            Assert.AreEqual("", Utility.PrepareTargetForUseInMacro(null));
            Assert.AreEqual("", Utility.PrepareTargetForUseInMacro(""));
            Assert.AreEqual("", Utility.PrepareTargetForUseInMacro("http://"));
            Assert.AreEqual("", Utility.PrepareTargetForUseInMacro("https://"));
            Assert.AreEqual("", Utility.PrepareTargetForUseInMacro("$"));
            Assert.AreEqual("", Utility.PrepareTargetForUseInMacro("$%&@$"));

            // Valid replacement cases
            Assert.AreEqual("www-yahoo-com", Utility.PrepareTargetForUseInMacro("http://www.yahoo.com"));
            Assert.AreEqual("www-yahoo-com", Utility.PrepareTargetForUseInMacro("http://www.yahoo.com/"));
            Assert.AreEqual("www-yahoo-com+blue+dog", Utility.PrepareTargetForUseInMacro("http://www.yahoo.com/blue/dog"));
            Assert.AreEqual("www-yahoo-com+blue+dog-aspxfrog252452", Utility.PrepareTargetForUseInMacro("http://www.yahoo.com/blue/dog.aspx?frog=252452"));
            
        }

        [TestMethod]
        public void TestUtilityReplaceString()
        {
            // Note that we're only testing the case-insensitive method

            // Null cases
            Assert.AreEqual("", Utility.ReplaceString(null, null, null));
            Assert.AreEqual("", Utility.ReplaceString("", null, null));
            Assert.AreEqual("", Utility.ReplaceString(null, "", null));
            Assert.AreEqual("", Utility.ReplaceString(null, null, ""));
            Assert.AreEqual("", Utility.ReplaceString("", "", null));
            Assert.AreEqual("", Utility.ReplaceString(null, "", ""));
            Assert.AreEqual("", Utility.ReplaceString("", null, ""));
            Assert.AreEqual("", Utility.ReplaceString("", "", ""));
            Assert.AreEqual("", Utility.ReplaceString("ace", null, null));
            Assert.AreEqual("", Utility.ReplaceString(null, "ace", null));
            Assert.AreEqual("", Utility.ReplaceString(null, null, "ace"));
            Assert.AreEqual("", Utility.ReplaceString("ace", "ac", null));
            Assert.AreEqual("", Utility.ReplaceString(null, "ac", "ace"));
            Assert.AreEqual("", Utility.ReplaceString("ace", null, "ac"));

            // Expected cases
            Assert.AreEqual("Be", Utility.ReplaceString("ace", "aC", "B"));
            Assert.AreEqual("aB", Utility.ReplaceString("ace", "cE", "B"));
            Assert.AreEqual("aDDDb", Utility.ReplaceString("aceCecEb", "CE", "D"));

        }

        [TestMethod]
        public void TestUtilityResolveDateMacro()
        {
            // Note that we're NOT invoking the "now" method
            DateTime dt = DateTime.Parse("2001-03-04 08:07:05");

            // Test null cases
            Assert.AreEqual("[NULL]", Utility.ResolveDateMacro(dt, null));

            // Test blank case
            Assert.AreEqual("[BLANK]", Utility.ResolveDateMacro(dt, ""));

            // Test valid dates
            Assert.AreEqual("2001", Utility.ResolveDateMacro(dt, "yYyY"));
            Assert.AreEqual("03", Utility.ResolveDateMacro(dt, "Mm"));
            Assert.AreEqual("04", Utility.ResolveDateMacro(dt, "dD"));
            Assert.AreEqual("08", Utility.ResolveDateMacro(dt, "Hh"));
            Assert.AreEqual("07", Utility.ResolveDateMacro(dt, "tT"));
            Assert.AreEqual("05", Utility.ResolveDateMacro(dt, "Ss"));

            // Test a misfire
            Assert.AreEqual("[XxX]", Utility.ResolveDateMacro(dt, "XxX"));
        }

        [TestMethod]
        public void TestUtilityResolveMacros()
        {

            // Note that we're NOT invoking the "now" method
            DateTime dt = DateTime.Parse("2001-03-04 08:07:05");

            // Test null cases
            Assert.AreEqual("", Utility.ResolveMacros(null, null, dt));
            Assert.AreEqual("", Utility.ResolveMacros("", null, dt));
            Assert.AreEqual("", Utility.ResolveMacros(null, "", dt));
            Assert.AreEqual("a2001nfRoG", Utility.ResolveMacros("a[yyYy]n[taRget]fRoG", null, dt));
            Assert.AreEqual("", Utility.ResolveMacros(null, "blUe", dt));

            // Test valid dates and targets (note that targets get lower-cased!)
            Assert.AreEqual("a2001nbluefRoG", Utility.ResolveMacros("a[yyYy]n[taRget]fRoG", "blUe", dt));
            Assert.AreEqual("temp_manifest_blue_20010304-080705.csv", 
                Utility.ResolveMacros("temp_manifest_[Target]_[YYYY][MM][DD]-[HH][TT][SS].csv", "blUe", dt));

        }


        [TestMethod]
        public void TestUtilityPrepareLocalFilename()
        {
            // Empty cases
            Assert.AreEqual("", Utility.PrepareLocalFilename(null, null));
            Assert.AreEqual("", Utility.PrepareLocalFilename("", null));
            Assert.AreEqual("", Utility.PrepareLocalFilename(null, ""));
            Assert.AreEqual("http:_!__!_", Utility.PrepareLocalFilename("http://", ""));
            Assert.AreEqual("https:_!__!_", Utility.PrepareLocalFilename("https://", ""));
            Assert.AreEqual("", Utility.PrepareLocalFilename("", "http://"));
            Assert.AreEqual("", Utility.PrepareLocalFilename("", "https://"));

            // Valid replacement cases
            Assert.AreEqual("", Utility.PrepareLocalFilename("http://www.yahoo.com", "http://www.yahoo.com"));
            Assert.AreEqual("_!_", Utility.PrepareLocalFilename("http://www.yahoo.com/", "http://www.yahoo.com"));
            Assert.AreEqual("dog.txt", Utility.PrepareLocalFilename("http://www.yahoo.com/dog.txt", "http://www.yahoo.com/"));
            Assert.AreEqual("dog.txt", Utility.PrepareLocalFilename("http://www.yahoo.com/sub/dog.txt", "http://www.yahoo.com/sub/"));
            Assert.AreEqual("sub2_!_sub3_!_dog.txt", Utility.PrepareLocalFilename("http://www.yahoo.com/sub/sub2/sub3/dog.txt", "http://www.yahoo.com/sub/"));

        }

    
    
    }
}
