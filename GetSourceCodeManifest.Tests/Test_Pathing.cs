using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GetSourceCodeManifest.Tests
{
    [TestClass]
    public class TestPathing
    {
        [TestMethod]
        public void TestPathingFixTfsPathSpec()
        {
            // ReSharper disable once JoinDeclarationAndInitializer
            string response;

            // Test blank string
            response = Pathing.FixTfsPathSpec("");
            Assert.AreEqual("/*.*", response);

            // Test unusual but legal cases
            response = Pathing.FixTfsPathSpec("*.*");
            Assert.AreEqual("*.*/*.*", response);
            response = Pathing.FixTfsPathSpec("/*.*");
            Assert.AreEqual("/*.*", response);
            response = Pathing.FixTfsPathSpec("/");
            Assert.AreEqual("/*.*", response);

            // Test usual cases
            const string baseString = "https://tfsserver:808/TF/DSColl";
            response = Pathing.FixTfsPathSpec(baseString);
            Assert.AreEqual(baseString + "/*.*", response);
            response = Pathing.FixTfsPathSpec(baseString + "/");
            Assert.AreEqual(baseString + "/*.*", response);
            response = Pathing.FixTfsPathSpec(baseString + "/*.*");
            Assert.AreEqual(baseString + "/*.*", response);

        }

        [TestMethod]
        public void TestPathingMakeRelativeTfsPath()
        {

            // ReSharper disable once JoinDeclarationAndInitializer
            string response;

            // Test blank strings
            response = Pathing.MakeRelativeTfsPath("","");
            Assert.AreEqual("", response);
            response = Pathing.MakeRelativeTfsPath("$\\proj\\branch\\sub1\\sub2", "");
            Assert.AreEqual("", response);
            response = Pathing.MakeRelativeTfsPath("", "$\\proj\\branch\\sub1\\sub2");
            Assert.AreEqual("\\proj\\branch\\sub1\\sub2", response);

            // Test usual cases
            response = Pathing.MakeRelativeTfsPath("$\\proj\\branch", "$\\proj\\branch\\sub1");
            Assert.AreEqual("sub1", response);
            response = Pathing.MakeRelativeTfsPath("$\\proj\\branch", "$\\proj\\branch\\sub1\\sub2");
            Assert.AreEqual("sub1\\sub2", response);

        }

        [TestMethod]
        public void TestPathingRemoveTfsFileWildcard()
        {

            // ReSharper disable once JoinDeclarationAndInitializer
            string response;

            // Test blank strings
            response = Pathing.RemoveTfsFileWildcard("");
            Assert.AreEqual("", response);

            // Test unusual but legal cases
            response = Pathing.RemoveTfsFileWildcard("*.*");
            Assert.AreEqual("*.*", response);
            response = Pathing.RemoveTfsFileWildcard("/*.*");
            Assert.AreEqual("", response);
            response = Pathing.RemoveTfsFileWildcard("/");
            Assert.AreEqual("", response);

            // Test usual cases
            response = Pathing.RemoveTfsFileWildcard("$/proj/branch/sub1");
            Assert.AreEqual("$/proj/branch/sub1", response);
            response = Pathing.RemoveTfsFileWildcard("$/proj/branch/sub1/");
            Assert.AreEqual("$/proj/branch/sub1", response);
            response = Pathing.RemoveTfsFileWildcard("$/proj/branch/sub1/*.*");
            Assert.AreEqual("$/proj/branch/sub1", response);
            response = Pathing.RemoveTfsFileWildcard("$/proj/branch/sub1/sub2");
            Assert.AreEqual("$/proj/branch/sub1/sub2", response);
            response = Pathing.RemoveTfsFileWildcard("$/proj/branch/sub1/sub2/");
            Assert.AreEqual("$/proj/branch/sub1/sub2", response);
            response = Pathing.RemoveTfsFileWildcard("$/proj/branch/sub1/sub2/*.*");
            Assert.AreEqual("$/proj/branch/sub1/sub2", response);

        }

        [TestMethod]
        public void TestPathingGetLastFolderInTfsPath()
        {

            // ReSharper disable once JoinDeclarationAndInitializer
            string response;

            // Test blank strings
            response = Pathing.GetLastFolderInTfsPath("");
            Assert.AreEqual("", response);

            response = Pathing.GetLastFolderInTfsPath("$/proj/branch/sub1");
            Assert.AreEqual("sub1", response);
            response = Pathing.GetLastFolderInTfsPath("$/proj/branch/sub1/");
            Assert.AreEqual("sub1", response);
            response = Pathing.GetLastFolderInTfsPath("$/proj/branch/sub1/*.*");
            Assert.AreEqual("sub1", response);
            response = Pathing.GetLastFolderInTfsPath("$/proj/branch/sub1/sub2");
            Assert.AreEqual("sub2", response);
            response = Pathing.GetLastFolderInTfsPath("$/proj/branch/sub1/sub2/");
            Assert.AreEqual("sub2", response);
            response = Pathing.GetLastFolderInTfsPath("$/proj/branch/sub1/sub2/*.*");
            Assert.AreEqual("sub2", response);

        }

    }
}
