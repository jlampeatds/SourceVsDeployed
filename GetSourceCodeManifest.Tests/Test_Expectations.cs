using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GetSourceCodeManifest.Tests
{
    [TestClass]
    public class TestExpectations
    {
        [TestMethod]
        public void TestExpectationsMaskMatches()
        {
            // Test blank string
            Assert.IsTrue(Expectations.MaskMatches("", ""));
            Assert.IsTrue(Expectations.MaskMatches("*", ""));
            Assert.IsTrue(Expectations.MaskMatches("*.*", ""));

            // Test naked wildcards
            Assert.IsTrue(Expectations.MaskMatches("*", "a"));
            Assert.IsTrue(Expectations.MaskMatches("*.*", "a"));
            Assert.IsTrue(Expectations.MaskMatches("*", "a."));
            Assert.IsTrue(Expectations.MaskMatches("*.*", "a."));
            Assert.IsTrue(Expectations.MaskMatches("*", ".a"));
            Assert.IsTrue(Expectations.MaskMatches("*.*", ".a"));
            Assert.IsTrue(Expectations.MaskMatches("*", "a.a"));
            Assert.IsTrue(Expectations.MaskMatches("*.*", "a.a"));

            // Test left-bounded wildcards
            Assert.IsTrue(Expectations.MaskMatches("b*", "ba"));
            Assert.IsFalse(Expectations.MaskMatches("b*.*", "ba"));
            Assert.IsTrue(Expectations.MaskMatches("b*", "ba."));
            Assert.IsTrue(Expectations.MaskMatches("b*.*", "ba."));
            Assert.IsTrue(Expectations.MaskMatches("b*", "b.a"));
            Assert.IsTrue(Expectations.MaskMatches("b*.*", "b.a"));
            Assert.IsTrue(Expectations.MaskMatches("b*", "ba.a"));
            Assert.IsTrue(Expectations.MaskMatches("b*.*", "ba.a"));
            Assert.IsFalse(Expectations.MaskMatches("b*", "cba"));
            Assert.IsFalse(Expectations.MaskMatches("b*.*", "cba"));
            Assert.IsFalse(Expectations.MaskMatches("b*", "cba."));
            Assert.IsFalse(Expectations.MaskMatches("b*.*", "cba."));
            Assert.IsFalse(Expectations.MaskMatches("b*", "cb.a"));
            Assert.IsFalse(Expectations.MaskMatches("b*.*", "cb.a"));
            Assert.IsFalse(Expectations.MaskMatches("b*", "cba.a"));
            Assert.IsFalse(Expectations.MaskMatches("b*.*", "a.a"));
            Assert.IsFalse(Expectations.MaskMatches("b*", "a"));
            Assert.IsFalse(Expectations.MaskMatches("b*.*", "a"));
            Assert.IsFalse(Expectations.MaskMatches("b*", "a."));
            Assert.IsFalse(Expectations.MaskMatches("b*.*", "a."));
            Assert.IsFalse(Expectations.MaskMatches("b*", ".a"));
            Assert.IsFalse(Expectations.MaskMatches("b*.*", ".a"));
            Assert.IsFalse(Expectations.MaskMatches("b*", "a.a"));
            Assert.IsFalse(Expectations.MaskMatches("b*.*", "a.a"));

            // Test right-bounded wildcards
            Assert.IsTrue(Expectations.MaskMatches("*b", "ab"));
            Assert.IsFalse(Expectations.MaskMatches("*.*b", "ab"));
            Assert.IsTrue(Expectations.MaskMatches("*b", "a.b"));
            Assert.IsTrue(Expectations.MaskMatches("*.*b", "a.b"));
            Assert.IsTrue(Expectations.MaskMatches("*b", ".ab"));
            Assert.IsTrue(Expectations.MaskMatches("*.*b", ".ab"));
            Assert.IsTrue(Expectations.MaskMatches("*b", "a.ab"));
            Assert.IsTrue(Expectations.MaskMatches("*.*b", "a.ab"));
            Assert.IsFalse(Expectations.MaskMatches("*b", "abc"));
            Assert.IsFalse(Expectations.MaskMatches("*.*b", "abc"));
            Assert.IsFalse(Expectations.MaskMatches("*b", "a.bc"));
            Assert.IsFalse(Expectations.MaskMatches("*.*b", "a.bc"));
            Assert.IsFalse(Expectations.MaskMatches("*b", ".abc"));
            Assert.IsFalse(Expectations.MaskMatches("*.*b", ".abc"));
            Assert.IsFalse(Expectations.MaskMatches("*b", "a.abc"));
            Assert.IsFalse(Expectations.MaskMatches("*.*b", "a.abc"));
            Assert.IsFalse(Expectations.MaskMatches("*b", "a"));
            Assert.IsFalse(Expectations.MaskMatches("*.*b", "a"));
            Assert.IsFalse(Expectations.MaskMatches("*b", "a."));
            Assert.IsFalse(Expectations.MaskMatches("*.*b", "a."));
            Assert.IsFalse(Expectations.MaskMatches("*b", ".a"));
            Assert.IsFalse(Expectations.MaskMatches("*.*b", ".a"));
            Assert.IsFalse(Expectations.MaskMatches("*b", "a.a"));
            Assert.IsFalse(Expectations.MaskMatches("*.*b", "a.a"));

        }
    }
}
