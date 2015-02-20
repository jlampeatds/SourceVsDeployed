using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace SourceVsDeployed.Tests
{
    [TestClass]
    public class TestArguments
    {
        [TestMethod]
        public void TestArgumentsPopulate()
        {
            // Arguments is a static class with static properties.  

            // ReSharper disable once JoinDeclarationAndInitializer
            string[] saArgs;

            // Test pre-initialization values.  Populated values should be blank.  Temp paths should exist.
            Assert.IsNull(Arguments.Action);
            Assert.IsNull(Arguments.Manifest);
            Assert.IsNull(Arguments.Target);
            Assert.IsFalse(Arguments.ManifestIsAURL);
            Assert.IsFalse(Arguments.ReadyToRun);
            Assert.AreEqual(0, Arguments.ReturnCode);
            Assert.AreEqual("", Arguments.TemporaryFolder);

            // Test some valid arguments with a local manifest
            saArgs = new string[3];
            saArgs[0] = "parse";    // action
            saArgs[1] = "manifest_file.txt";    // manifest
            saArgs[2] = "http://www.yahoo.com/hello/world";    // target 
            Arguments.Populate(saArgs);
            Assert.AreEqual(saArgs[0], Arguments.Action);
            Assert.AreEqual(saArgs[1], Arguments.Manifest);
            Assert.AreEqual(saArgs[2] + "/", Arguments.Target);
            Assert.IsFalse(Arguments.ManifestIsAURL);
            Assert.IsTrue(Arguments.ReadyToRun);
            Assert.AreEqual(0,Arguments.ReturnCode);
            Assert.AreEqual("", Arguments.TemporaryFolder);

            // Test some valid arguments with a remote manifest
            saArgs = new string[3];
            saArgs[0] = "list";    // action
            saArgs[1] = "http://www.google.com/remote/manifest_file.txt";    // manifest
            saArgs[2] = "http://www.yahoo.com/hello/world";    // target 
            Arguments.Populate(saArgs);
            // Added a ToString() hack here to quiet down a compiler complaint about object/object vs. string/string comparison
            Assert.AreEqual(saArgs[0].ToString(CultureInfo.InvariantCulture), Arguments.Action);
            Assert.AreEqual(saArgs[1], Arguments.Manifest);
            Assert.AreEqual(saArgs[2] + "/", Arguments.Target);
            Assert.IsTrue(Arguments.ManifestIsAURL);
            Assert.IsTrue(Arguments.ReadyToRun);
            Assert.AreEqual(0, Arguments.ReturnCode);

            // Test some valid arguments with a remote manifest and only two parms
            saArgs = new string[2];
            saArgs[0] = "validate";    // action
            saArgs[1] = "http://www.google.com/remote/manifest_file.txt";    // manifest
            Arguments.Populate(saArgs);
            Assert.AreEqual(saArgs[0], Arguments.Action);
            Assert.AreEqual(saArgs[1], Arguments.Manifest);
            Assert.AreEqual("http://www.google.com/", Arguments.Target);
            Assert.IsTrue(Arguments.ManifestIsAURL);
            Assert.IsTrue(Arguments.ReadyToRun);
            Assert.AreEqual(0, Arguments.ReturnCode);

            // Test an invalid action
            saArgs = new string[2];
            saArgs[0] = "invalidate";    // action
            saArgs[1] = "http://www.google.com/remote/manifest_file.txt";    // manifest
            Arguments.Populate(saArgs);
            Assert.AreEqual(saArgs[0], Arguments.Action);
            Assert.AreEqual(saArgs[1], Arguments.Manifest);
            Assert.AreEqual("http://www.google.com/", Arguments.Target);
            Assert.IsTrue(Arguments.ManifestIsAURL);
            Assert.IsFalse(Arguments.ReadyToRun);
            Assert.AreEqual(1, Arguments.ReturnCode);

            // Test too few arguments (1)
            saArgs = new string[1];
            saArgs[0] = "validate";    // action
            Arguments.Populate(saArgs);
            Assert.AreEqual("", Arguments.Action);
            Assert.AreEqual("", Arguments.Manifest);
            Assert.AreEqual("", Arguments.Target);
            Assert.IsFalse(Arguments.ManifestIsAURL);
            Assert.IsFalse(Arguments.ReadyToRun);
            Assert.AreEqual(0, Arguments.ReturnCode);

            // Test too few arguments (0)
            saArgs = new string[0];
            Arguments.Populate(saArgs);
            Assert.AreEqual("", Arguments.Action);
            Assert.AreEqual("", Arguments.Manifest);
            Assert.AreEqual("", Arguments.Target);
            Assert.IsFalse(Arguments.ManifestIsAURL);
            Assert.IsFalse(Arguments.ReadyToRun);
            Assert.AreEqual(0, Arguments.ReturnCode);

        }

        [TestMethod]
        public void TestArgumentsMakeTarget()
        {
            // ReSharper disable once JoinDeclarationAndInitializer
            string[] saArgs;

            // Test some empty cases
            Assert.AreEqual("", Arguments.MakeTarget(null, null));
            Assert.AreEqual("", Arguments.MakeTarget(null, ""));
            saArgs = new string[0];
            Assert.AreEqual("", Arguments.MakeTarget(saArgs, ""));
            saArgs = new string[1];
            Assert.AreEqual("", Arguments.MakeTarget(saArgs, ""));
            saArgs = new string[2];
            Assert.AreEqual("", Arguments.MakeTarget(saArgs, ""));
            saArgs = new string[3];
            Assert.AreEqual("", Arguments.MakeTarget(saArgs, ""));

            // Test some common cases
            saArgs = new string[3];
            saArgs[0] = "list";    // action
            saArgs[1] = "http://www.google.com/remote/manifest_file.txt";    // manifest
            saArgs[2] = "http://www.yahoo.com/hello/world";    // target 
            Assert.AreEqual("http://www.yahoo.com/hello/world/", Arguments.MakeTarget(saArgs, ""));
            saArgs = new string[3];
            saArgs[0] = "list";    // action
            saArgs[1] = "http://www.google.com/remote/manifest_file.txt";    // manifest
            saArgs[2] = "http://www.yahoo.com/hello/world";    // target 
            Assert.AreEqual("http://www.yahoo.com/hello/world/", Arguments.MakeTarget(saArgs, "hello"));
            saArgs = new string[2];
            saArgs[0] = "list";    // action
            saArgs[1] = "http://www.google.com/remote/manifest_file.txt";    // manifest
            Assert.AreEqual("http://www.yahoo.com/", Arguments.MakeTarget(saArgs, "http://www.yahoo.com/hello/world"));

            // No target cases
            saArgs = new string[2];
            saArgs[0] = "list";    // action
            saArgs[1] = "http://www.google.com/remote/manifest_file.txt";    // manifest
            Assert.AreEqual("", Arguments.MakeTarget(saArgs, ""));
            saArgs = new string[2];
            saArgs[0] = "list";    // action
            saArgs[1] = "http://www.google.com/remote/manifest_file.txt";    // manifest
            Assert.AreEqual("", Arguments.MakeTarget(saArgs, "hello"));

        }

        [TestMethod]
        public void TestArgumentsIsAValidAction()
        {
            // Note that this is case sensitive
            Assert.IsTrue(Arguments.IsAValidAction("parse"));
            Assert.IsTrue(Arguments.IsAValidAction("list"));
            Assert.IsTrue(Arguments.IsAValidAction("validate"));
            Assert.IsFalse(Arguments.IsAValidAction(null));
            Assert.IsFalse(Arguments.IsAValidAction(""));
            Assert.IsFalse(Arguments.IsAValidAction("parse "));
            Assert.IsFalse(Arguments.IsAValidAction("parse list"));
            Assert.IsFalse(Arguments.IsAValidAction("toss"));
        }
    }
}
